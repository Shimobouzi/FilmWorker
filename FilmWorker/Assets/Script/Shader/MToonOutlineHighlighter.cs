using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VRM10/MToon10 系マテリアルのアウトライン機能を使って強調表示する。
/// MaterialPropertyBlock ではキーワード切替ができないため、Renderer.materials (インスタンス) に対して
/// _OutlineWidth/_OutlineColor 等を一時的に上書きして、解除時に復元する。
/// </summary>
[DisallowMultipleComponent]
public sealed class MToonOutlineHighlighter : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] Color outlineColor = new Color(1f, 0.95f, 0.2f, 1f);
    [SerializeField, Range(0f, 0.05f)] float outlineWidth = 0.01f;
    [SerializeField, Range(0f, 1f)] float outlineLightingMix = 0f;

    [Header("Keyword")]
    [SerializeField] bool useScreenSpaceOutline;

    [Header("Runtime")]
    [SerializeField] bool highlighted;

    Renderer[] renderers;
    readonly List<Material> targetMaterials = new();
    readonly Dictionary<Material, MaterialState> original = new();

    static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    static readonly int OutlineLightingMixId = Shader.PropertyToID("_OutlineLightingMix");

    const string KeywordWorld = "_MTOON_OUTLINE_WORLD";
    const string KeywordScreen = "_MTOON_OUTLINE_SCREEN";

    [Serializable]
    struct MaterialState
    {
        public bool hasOutlineColor;
        public bool hasOutlineWidth;
        public bool hasOutlineLightingMix;
        public Color outlineColor;
        public float outlineWidth;
        public float outlineLightingMix;
        public bool keywordWorld;
        public bool keywordScreen;
    }

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        CacheTargetMaterials();
        Apply();
    }

    public void SetHighlighted(bool value)
    {
        highlighted = value;
        Apply();
    }

    public bool IsHighlighted => highlighted;

    void CacheTargetMaterials()
    {
        targetMaterials.Clear();
        original.Clear();

        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            // materials を触ることで、この Renderer 専用の Material インスタンスを得る
            var mats = r.materials;
            if (mats == null) continue;

            for (int m = 0; m < mats.Length; m++)
            {
                var mat = mats[m];
                if (mat == null) continue;
                if (!IsSupported(mat)) continue;

                if (!original.ContainsKey(mat))
                {
                    original.Add(mat, CaptureState(mat));
                    targetMaterials.Add(mat);
                }
            }
        }
    }

    static bool IsSupported(Material mat)
    {
        // MToon10 (Built-in / URP) 両方がこのプロパティ名を持つ。
        // _OutlineLightingMix は無い場合もあるので必須にはしない。
        return mat.HasProperty(OutlineWidthId) && mat.HasProperty(OutlineColorId);
    }

    static MaterialState CaptureState(Material mat)
    {
        var state = new MaterialState
        {
            hasOutlineColor = mat.HasProperty(OutlineColorId),
            hasOutlineWidth = mat.HasProperty(OutlineWidthId),
            hasOutlineLightingMix = mat.HasProperty(OutlineLightingMixId),
            keywordWorld = mat.IsKeywordEnabled(KeywordWorld),
            keywordScreen = mat.IsKeywordEnabled(KeywordScreen),
        };

        if (state.hasOutlineColor) state.outlineColor = mat.GetColor(OutlineColorId);
        if (state.hasOutlineWidth) state.outlineWidth = mat.GetFloat(OutlineWidthId);
        if (state.hasOutlineLightingMix) state.outlineLightingMix = mat.GetFloat(OutlineLightingMixId);

        return state;
    }

    void Apply()
    {
        if (targetMaterials.Count == 0) return;

        if (!highlighted)
        {
            Restore();
            return;
        }

        for (int i = 0; i < targetMaterials.Count; i++)
        {
            var mat = targetMaterials[i];
            if (mat == null) continue;

            if (mat.HasProperty(OutlineWidthId))
                mat.SetFloat(OutlineWidthId, outlineWidth);

            if (mat.HasProperty(OutlineColorId))
                mat.SetColor(OutlineColorId, outlineColor);

            if (mat.HasProperty(OutlineLightingMixId))
                mat.SetFloat(OutlineLightingMixId, outlineLightingMix);

            if (useScreenSpaceOutline)
            {
                mat.DisableKeyword(KeywordWorld);
                mat.EnableKeyword(KeywordScreen);
            }
            else
            {
                mat.DisableKeyword(KeywordScreen);
                mat.EnableKeyword(KeywordWorld);
            }
        }
    }

    void Restore()
    {
        foreach (var pair in original)
        {
            var mat = pair.Key;
            if (mat == null) continue;

            var state = pair.Value;

            if (state.hasOutlineWidth)
                mat.SetFloat(OutlineWidthId, state.outlineWidth);

            if (state.hasOutlineColor)
                mat.SetColor(OutlineColorId, state.outlineColor);

            if (state.hasOutlineLightingMix)
                mat.SetFloat(OutlineLightingMixId, state.outlineLightingMix);

            if (state.keywordWorld) mat.EnableKeyword(KeywordWorld);
            else mat.DisableKeyword(KeywordWorld);

            if (state.keywordScreen) mat.EnableKeyword(KeywordScreen);
            else mat.DisableKeyword(KeywordScreen);
        }
    }

    void OnDisable()
    {
        if (highlighted)
            Restore();
    }
}
