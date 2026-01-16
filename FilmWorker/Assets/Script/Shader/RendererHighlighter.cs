using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Renderer/SkinnedMeshRenderer 向けの強調表示。
/// MaterialPropertyBlock で色/発光を上書きしてハイライトする。
/// </summary>
[DisallowMultipleComponent]
public sealed class RendererHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] Color highlightColor = new Color(1f, 0.95f, 0.2f, 1f);
    [SerializeField] Color emissionColor = new Color(1f, 0.8f, 0.1f, 1f);
    [SerializeField, Min(0f)] float emissionIntensity = 1.5f;

    [Header("Runtime")]
    [SerializeField] bool highlighted;

    Renderer[] renderers;
    readonly Dictionary<Renderer, MaterialPropertyBlock> originalBlocks = new();

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        CacheOriginalBlocks();
        Apply();
    }

    public void SetHighlighted(bool value)
    {
        highlighted = value;
        Apply();
    }

    public bool IsHighlighted => highlighted;

    void CacheOriginalBlocks()
    {
        originalBlocks.Clear();
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            var block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            originalBlocks[r] = block;
        }
    }

    void Apply()
    {
        if (renderers == null) return;

        if (!highlighted)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;

                if (originalBlocks.TryGetValue(r, out var original))
                    r.SetPropertyBlock(original);
                else
                    r.SetPropertyBlock(null);
            }
            return;
        }

        var highlightBlock = new MaterialPropertyBlock();
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(highlightBlock);

            // 代表的なプロパティ名を両方触る（VRM/URP/MToonなど差異吸収）
            highlightBlock.SetColor(BaseColorId, highlightColor);
            highlightBlock.SetColor(ColorId, highlightColor);
            highlightBlock.SetColor(EmissionColorId, emissionColor * emissionIntensity);

            r.SetPropertyBlock(highlightBlock);
        }
    }
}
