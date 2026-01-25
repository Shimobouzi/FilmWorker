using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3Dアウトライン（メッシュ複製+少し拡大+単色マテリアル）
/// - Rigidbody不使用のまま見た目だけ強調
/// - SkinnedMeshRendererにも対応（同じスケルトン参照）
/// </summary>
public sealed class MeshOutlineHighlighter3D : MonoBehaviour
{
    [Header("Outline")]
    [SerializeField] bool highlighted = false;

    [Tooltip("拡大率。0.03 で 3% だけ膨らませる")]
    [SerializeField] float thickness = 0.03f;

    [SerializeField] Color color = Color.white;

    [Tooltip("アウトライン用マテリアル（未指定ならURP Unlit/Standardで生成）")]
    [SerializeField] Material outlineMaterial;

    readonly List<GameObject> outlineObjects = new();

    static readonly int ColorPropBaseColor = Shader.PropertyToID("_BaseColor");
    static readonly int ColorPropColor = Shader.PropertyToID("_Color");
    static readonly int CullProp = Shader.PropertyToID("_Cull");

    void Awake()
    {
        BuildOutline();
        Apply(highlighted);
    }

    void OnValidate()
    {
        if (!Application.isPlaying) return;
        Apply(highlighted);
    }

    public void SetHighlighted(bool on)
    {
        highlighted = on;
        Apply(on);
    }

    public void SetColor(Color c)
    {
        color = c;
        Apply(highlighted);
    }

    public void SetThickness(float t)
    {
        thickness = Mathf.Max(0f, t);
        foreach (var go in outlineObjects)
        {
            if (go == null) continue;
            go.transform.localScale = Vector3.one * (1f + thickness);
        }
    }

    void BuildOutline()
    {
        Cleanup();

        var mat = GetOrCreateOutlineMaterial();

        foreach (var mr in GetComponentsInChildren<MeshRenderer>(true))
        {
            var mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            var go = new GameObject("__Outline_Mesh");
            go.transform.SetParent(mr.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * (1f + thickness);

            var omf = go.AddComponent<MeshFilter>();
            omf.sharedMesh = mf.sharedMesh;

            var omr = go.AddComponent<MeshRenderer>();
            omr.sharedMaterial = mat;

            outlineObjects.Add(go);
        }

        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr.sharedMesh == null) continue;

            var go = new GameObject("__Outline_Skinned");
            go.transform.SetParent(smr.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * (1f + thickness);

            var osmr = go.AddComponent<SkinnedMeshRenderer>();
            osmr.sharedMesh = smr.sharedMesh;
            osmr.bones = smr.bones;
            osmr.rootBone = smr.rootBone;
            osmr.updateWhenOffscreen = smr.updateWhenOffscreen;
            osmr.sharedMaterial = mat;

            outlineObjects.Add(go);
        }
    }

    Material GetOrCreateOutlineMaterial()
    {
        if (outlineMaterial != null) return outlineMaterial;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Standard");

        outlineMaterial = new Material(shader)
        {
            name = "Outline_Material_Runtime"
        };

        ApplyMaterialParams(outlineMaterial);
        return outlineMaterial;
    }

    void Apply(bool on)
    {
        if (outlineMaterial != null)
            ApplyMaterialParams(outlineMaterial);

        foreach (var go in outlineObjects)
        {
            if (go == null) continue;
            go.SetActive(on);
        }
    }

    void ApplyMaterialParams(Material mat)
    {
        if (mat == null) return;

        if (mat.HasProperty(ColorPropBaseColor))
            mat.SetColor(ColorPropBaseColor, color);
        else if (mat.HasProperty(ColorPropColor))
            mat.SetColor(ColorPropColor, color);

        // FrontCull（URP Unlit想定）。無い場合は何もしない。
        if (mat.HasProperty(CullProp))
            mat.SetInt(CullProp, (int)UnityEngine.Rendering.CullMode.Front);
    }

    void Cleanup()
    {
        for (int i = outlineObjects.Count - 1; i >= 0; i--)
        {
            if (outlineObjects[i] == null) continue;
            Destroy(outlineObjects[i]);
        }
        outlineObjects.Clear();
    }

    void OnDestroy()
    {
        Cleanup();

        // runtime生成のみ破棄（Inspectorで渡したMaterialは破棄しない）
        if (outlineMaterial != null && outlineMaterial.name == "Outline_Material_Runtime")
        {
            Destroy(outlineMaterial);
            outlineMaterial = null;
        }
    }
}