using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float jumpPower = 8f;
    public float gravity = -20f;

    [Header("Collision")]
    public LayerMask groundLayer;
    public float skinWidth = 0.05f;

    float yVelocity;
    IInputProvider input;
    BoxCollider2D col;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }

    public void ResetMotion()
    {
        yVelocity = 0f;
    }

    void Start()
    {
        // HumanInputProvider を自動でセット（すでに SetInput 済みなら上書きしない）
        if (input == null)
            SetInput(GetComponent<IInputProvider>());
    }

    public void SetInput(IInputProvider provider)
    {
        input = provider;
    }

    void Update()
    {
        if (input == null) return;
        if (col == null) return;

        float dt = Time.deltaTime;

        HandleHorizontal(dt);
        HandleVertical(dt);
    }

    void HandleHorizontal(float dt)
    {
        float h = input.GetHorizontal();
        if (h == 0) return;

        Vector2 dir = Vector2.right * Mathf.Sign(h);
        float distance = Mathf.Abs(h * moveSpeed * dt);

        MoveWithCollision(dir, distance);
    }

    void HandleVertical(float dt)
    {
        bool grounded = IsGrounded();

        if (grounded && yVelocity < 0)
            yVelocity = -2f; // 地面に張り付かせる

        if (grounded && input.GetJumpDown())
            yVelocity = jumpPower;

        yVelocity += gravity * dt;

        float move = yVelocity * dt;
        float distance = Mathf.Abs(move);
        if (distance <= 0f) return;

        Vector2 dir = move > 0f ? Vector2.up : Vector2.down;

        if (MoveWithCollision(dir, distance))
            yVelocity = 0f;
    }

    bool IsGrounded()
    {
        Bounds bounds = col.bounds;
        var distance = skinWidth + 0.05f;

        var hit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            Vector2.down,
            distance,
            groundLayer
        );

        return hit.collider != null;
    }

    bool MoveWithCollision(Vector2 dir, float distance)
    {
        if (distance <= 0f) return false;
        if (dir == Vector2.zero) return false;

        Bounds bounds = col.bounds;
        Vector2 origin = bounds.center;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            bounds.size,
            0f,
            dir,
            distance + skinWidth,
            groundLayer
        );

        // すでにめり込んでいる場合は、距離計算で押し戻す
        if (hit.collider != null && hit.distance <= 0f)
        {
            var separation = col.Distance(hit.collider);
            if (separation.isOverlapped)
            {
                transform.position += (Vector3)(separation.normal * separation.distance);
                Physics2D.SyncTransforms();
            }

            return true;
        }

        float moveDistance = distance;
        if (hit.collider != null)
            moveDistance = Mathf.Max(0f, hit.distance - skinWidth);

        if (moveDistance > 0f)
            transform.position += (Vector3)(dir * moveDistance);

        return hit.collider != null;
    }
}
