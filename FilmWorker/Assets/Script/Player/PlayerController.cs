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

    void Start()
    {
        // HumanInputProvider を自動でセット
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
    }

    void HandleHorizontal(float dt)
    {
        float h = input.GetHorizontal();
        if (h == 0) return;

        Vector2 dir = Vector2.right * Mathf.Sign(h);
        float distance = Mathf.Abs(h * moveSpeed * dt);

        if (!CheckCollision(dir, distance))
        {
            transform.position += (Vector3)(dir * distance);
        }
    }

    void HandleVertical(float dt)
    {
        bool grounded = IsGrounded();

        if (grounded && yVelocity < 0)
            yVelocity = -2f; // 地面に吸着させる

        if (grounded && input.GetJumpDown())
            yVelocity = jumpPower;

        yVelocity += gravity * dt;

        float move = yVelocity * dt;
        Vector2 dir = Vector2.up * Mathf.Sign(move);
        float distance = Mathf.Abs(move);

        if (!CheckCollision(dir, distance))
        {
            transform.position += Vector3.up * move;
        }
        else
        {
            // 天井 or 地面に当たったら速度リセット
            yVelocity = 0;
        }
    }

    bool IsGrounded()
    {
        Bounds bounds = col.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
        float distance = 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, groundLayer);
        return hit.collider != null;
    }

    bool CheckCollision(Vector2 dir, float distance)
    {
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

        return hit.collider != null;
    }
}
