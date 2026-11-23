using UnityEngine;

/// <summary>
/// Pohyb hráče v bočním pohledu (jeskyně). A/D + skok (Space nebo W), gravitace.
/// Hráč musí mít Rigidbody2D + Collider2D a tag "Player". Platformám přiřaď vrstvu "Ground".
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SideViewMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 9f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float coyoteTime = 0.1f;       // krátce po sejití z hrany lze ještě skočit
    [SerializeField] private float jumpBufferTime = 0.1f;   // stisk těsně před dopadem se zapamatuje
    [SerializeField] private LayerMask groundLayer;

    [Header("Vzhled (volitelné)")]
    [SerializeField] private SpriteRenderer sprite;         // otáčení doleva/doprava
    [SerializeField] private Animator animator;             // parametry: "Speed" (float), "Grounded" (bool)

    private Rigidbody2D rb;
    private Collider2D col;
    private float inputX;
    private float coyoteTimer;
    private float jumpBufferTimer;

    public bool IsGrounded { get; private set; }

    /// <summary>Směr doleva/doprava. Použije souboj pro útok a střelbu.</summary>
    public Vector2 Facing { get; private set; } = Vector2.right;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.linearVelocity = Vector2.zero;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (inputX != 0f)
            Facing = new Vector2(Mathf.Sign(inputX), 0f);

        if (sprite != null)
            sprite.flipX = Facing.x < 0f;

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool("Grounded", IsGrounded);
        }
    }

    void FixedUpdate()
    {
        PlayerStats stats = PlayerStats.Instance;

        if (stats != null && stats.IsDead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        IsGrounded = CheckGround();

        if (IsGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.fixedDeltaTime;

        float speed = moveSpeed * (stats != null ? stats.SpeedMultiplier : 1f);
        Vector2 v = rb.linearVelocity;
        v.x = inputX * speed;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            v.y = jumpForce;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        rb.linearVelocity = v;
    }

    // Kontrola země pod nohama podle collideru hráče (netřeba extra GroundCheck objekt)
    bool CheckGround()
    {
        Bounds b = col.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y - 0.03f);
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.06f);
        return Physics2D.OverlapBox(center, size, 0f, groundLayer) != null;
    }
}
