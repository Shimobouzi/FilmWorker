using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シェーダ依存(MToon等)しない3Dアウトライン。
/// メッシュ/スキンメッシュを複製して少し膨らませ、アウトライン用マテリアルで描画する。
/// </summary>
[DisallowMultipleComponent]
public sealed class MeshOutlineHighlighter : MonoBehaviour
{
    [Header("Outline")]
    [SerializeField] Color outlineColor = new Color(1f, 0.95f, 0.2f, 1f);
    [SerializeField, Min(1f)] float outlineScale = 1.03f;

    [Header("Shader")]
    [SerializeField] string outlineShaderName = "FilmWorker/OutlineUnlit";

    [Header("Runtime")]
    [SerializeField] bool highlighted;

    Transform outlineRoot;
    Material outlineMaterial;
    readonly List<Renderer> outlineRenderers = new();

    const int CullFront = 1;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    void Awake()
    {
        BuildIfNeeded();
        Apply();
    }

    public void SetHighlighted(bool value)
    {
        highlighted = value;
        Apply();
    }

    public bool IsHighlighted => highlighted;

    void BuildIfNeeded()
    {
        if (outlineRoot != null) return;

        var existing = transform.Find("__Outline3D");
        if (existing != null)
            outlineRoot = existing;
        else
        {
            var go = new GameObject("__Outline3D");
            outlineRoot = go.transform;
            outlineRoot.SetParent(transform, false);
        }

        outlineRoot.localPosition = Vector3.zero;
        outlineRoot.localRotation = Quaternion.identity;
        outlineRoot.localScale = Vector3.one;

        outlineRenderers.Clear();

        var shader = Shader.Find(outlineShaderName);
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader != null)
            outlineMaterial = new Material(shader);

        if (outlineMaterial != null)
        {
            if (outlineMaterial.HasProperty(BaseColorId)) outlineMaterial.SetColor(BaseColorId, outlineColor);
            if (outlineMaterial.HasProperty(ColorId)) outlineMaterial.SetColor(ColorId, outlineColor);

            // URP/Unlit などフォールバック時に Cull を合わせる（対応していれば）
            if (outlineMaterial.HasProperty("_Cull"))
                outlineMaterial.SetFloat("_Cull", CullFront);
            if (outlineMaterial.HasProperty("_CullMode"))
                outlineMaterial.SetFloat("_CullMode", CullFront);

            // 少し手前に出す（Z競合の軽減）
            outlineMaterial.renderQueue = 3010;
        }

        // MeshRenderer
        foreach (var src in GetComponentsInChildren<MeshRenderer>(true))
        {
            if (src == null) continue;
            if (src.transform == outlineRoot) continue;

            var mf = src.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            var dstGo = new GameObject("__Outline");
            dstGo.layer = src.gameObject.layer;
            dstGo.transform.SetParent(src.transform, false);
            dstGo.transform.localPosition = Vector3.zero;
            dstGo.transform.localRotation = Quaternion.identity;
            dstGo.transform.localScale = Vector3.one * outlineScale;

            var dstMf = dstGo.AddComponent<MeshFilter>();
            dstMf.sharedMesh = mf.sharedMesh;

            var dstMr = dstGo.AddComponent<MeshRenderer>();
            dstMr.sharedMaterial = outlineMaterial;
            dstMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dstMr.receiveShadows = false;

            outlineRenderers.Add(dstMr);
        }

        // SkinnedMeshRenderer
        foreach (var src in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (src == null) continue;
            if (src.transform == outlineRoot) continue;
            if (src.sharedMesh == null) continue;

            var dstGo = new GameObject("__Outline");
            dstGo.layer = src.gameObject.layer;
            dstGo.transform.SetParent(src.transform, false);
            dstGo.transform.localPosition = Vector3.zero;
            dstGo.transform.localRotation = Quaternion.identity;
            dstGo.transform.localScale = Vector3.one * outlineScale;

            var dst = dstGo.AddComponent<SkinnedMeshRenderer>();
            dst.sharedMesh = src.sharedMesh;
            dst.bones = src.bones;
            dst.rootBone = src.rootBone;
            dst.updateWhenOffscreen = src.updateWhenOffscreen;
            dst.quality = src.quality;
            dst.sharedMaterial = outlineMaterial;
            dst.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dst.receiveShadows = false;

            outlineRenderers.Add(dst);
        }

        // 一旦無効化しておく
        SetOutlineVisible(false);
    }

    void Apply()
    {
        BuildIfNeeded();

        if (outlineMaterial != null)
        {
            if (outlineMaterial.HasProperty(BaseColorId)) outlineMaterial.SetColor(BaseColorId, outlineColor);
            if (outlineMaterial.HasProperty(ColorId)) outlineMaterial.SetColor(ColorId, outlineColor);
        }

        SetOutlineVisible(highlighted);
    }

    void SetOutlineVisible(bool visible)
    {
        if (outlineRoot != null)
            outlineRoot.gameObject.SetActive(visible);

        for (int i = 0; i < outlineRenderers.Count; i++)
        {
            var r = outlineRenderers[i];
            if (r == null) continue;
            r.enabled = visible;
        }
    }

    void OnDestroy()
    {
        if (outlineMaterial != null)
            Destroy(outlineMaterial);
    }
}
