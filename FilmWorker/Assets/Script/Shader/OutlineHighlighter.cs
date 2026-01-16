using UnityEngine;

/// <summary>
/// SpriteRenderer を対象に、子オブジェクトの SpriteRenderer 複製で擬似アウトライン表示を行う。
/// （シェーダ不要。アウトラインは塗りつぶし方式で「縁取り風」）
/// </summary>
[DisallowMultipleComponent]
public sealed class OutlineHighlighter : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField, Min(1.0f)] private float outlineScale = 1.06f;
    [SerializeField] private int sortingOrderOffset = -1;

    [Header("Runtime")]
    [SerializeField] private bool highlighted;

    private SpriteRenderer _target;
    private SpriteRenderer _outline;
    private Transform _outlineTf;

    private void Awake()
    {
        _target = GetComponentInChildren<SpriteRenderer>(true);
        if (_target == null)
        {
            Debug.LogWarning($"{nameof(OutlineHighlighter)}: SpriteRenderer が見つかりません: {name}", this);
            enabled = false;
            return;
        }

        EnsureOutlineRenderer();
        ApplyHighlightedState();
    }

    private void LateUpdate()
    {
        if (_outline == null || _target == null) return;

        // 見た目の同期（スプライト差し替え・反転・ソート順変更に追従）
        _outline.sprite = _target.sprite;
        _outline.flipX = _target.flipX;
        _outline.flipY = _target.flipY;
        _outline.sortingLayerID = _target.sortingLayerID;
        _outline.sortingOrder = _target.sortingOrder + sortingOrderOffset;

        // スケール維持
        if (_outlineTf != null)
            _outlineTf.localScale = Vector3.one * outlineScale;
    }

    public void SetHighlighted(bool value)
    {
        highlighted = value;
        ApplyHighlightedState();
    }

    public bool IsHighlighted => highlighted;

    private void EnsureOutlineRenderer()
    {
        var child = transform.Find("__Outline");
        if (child == null)
        {
            var go = new GameObject("__Outline");
            go.transform.SetParent(transform, false);
            child = go.transform;
        }

        _outlineTf = child;
        _outline = child.GetComponent<SpriteRenderer>();
        if (_outline == null) _outline = child.gameObject.AddComponent<SpriteRenderer>();

        // 初期設定
        _outline.color = outlineColor;
        _outline.sprite = _target.sprite;
        _outline.sortingLayerID = _target.sortingLayerID;
        _outline.sortingOrder = _target.sortingOrder + sortingOrderOffset;

        // Material は基本的に同じでOK（色で縁を表現）
        _outline.sharedMaterial = _target.sharedMaterial;

        _outlineTf.localPosition = Vector3.zero;
        _outlineTf.localRotation = Quaternion.identity;
        _outlineTf.localScale = Vector3.one * outlineScale;
    }

    private void ApplyHighlightedState()
    {
        if (_outline != null)
            _outline.enabled = highlighted;
    }
}