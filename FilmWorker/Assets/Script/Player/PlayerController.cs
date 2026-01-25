using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float jumpPower = 8f;
    public float gravity = -20f;

    [Header("Collision (3D)")]
    public LayerMask groundLayer;
    public float skinWidth = 0.05f;

    [Header("Plane Lock")]
    [Tooltip("XY平面固定。Z座標を初期値に固定します。")]
    public bool lockZ = true;

    float yVelocity;
    float lockedZ;

    IInputProvider input;
    BoxCollider col;

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        if (lockZ) lockedZ = transform.position.z;
    }

    void Start()
    {
        // HumanInputProvider 等が付いている想定
        SetInput(GetComponent<IInputProvider>());
    }

    public void SetInput(IInputProvider provider)
    {
        input = provider;
    }

    void Update()
    {
        if (input == null) return;

        float dt = Time.deltaTime;

        HandleHorizontal(dt);
        HandleVertical(dt);

        if (lockZ)
        {
            var p = transform.position;
            p.z = lockedZ;
            transform.position = p;
        }
    }

    void HandleHorizontal(float dt)
    {
        float h = input.GetHorizontal();
        if (Mathf.Approximately(h, 0f)) return;

        Vector3 dir = Vector3.right * Mathf.Sign(h);
        float distance = Mathf.Abs(h * moveSpeed * dt);

        if (!CheckCollision(dir, distance))
        {
            transform.position += dir * distance;
        }
    }

    void HandleVertical(float dt)
    {
        bool grounded = IsGrounded();

        if (grounded && yVelocity < 0f)
            yVelocity = -2f; // 接地時の張り付き

        if (grounded && input.GetJumpDown())
            yVelocity = jumpPower;

        yVelocity += gravity * dt;

        float move = yVelocity * dt;
        Vector3 dir = Vector3.up * Mathf.Sign(move);
        float distance = Mathf.Abs(move);

        if (!CheckCollision(dir, distance))
        {
            transform.position += Vector3.up * move;
        }
        else
        {
            // 天井/床に当たったら速度リセット（スナップは未実装）
            yVelocity = 0f;
        }
    }

    bool IsGrounded()
    {
        Bounds b = col.bounds;

        // 足元から下方向へ短くRaycast
        Vector3 origin = b.center;
        origin.y = b.min.y + Mathf.Max(0.001f, skinWidth);

        float distance = 0.1f + skinWidth;

        return Physics.Raycast(origin, Vector3.down, distance, groundLayer, QueryTriggerInteraction.Ignore);
    }

    bool CheckCollision(Vector3 dir, float distance)
    {
        Bounds b = col.bounds;

        Vector3 center = b.center;
        Vector3 halfExtents = b.extents;

        // BoxCast は「開始時点でめり込んでいる」ケースに弱いので、
        // skinWidth を距離側に足し、extents はそのままにしておく（最小改修）
        return Physics.BoxCast(
            center,
            halfExtents,
            dir.normalized,
            Quaternion.identity,
            distance + skinWidth,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }
}
