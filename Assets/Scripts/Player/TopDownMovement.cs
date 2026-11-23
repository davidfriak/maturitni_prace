using UnityEngine;

/// Pohyb hráče v pohledu shora.

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Vzhled (volitelné)")]
    [SerializeField] private Animator animator;   // parametry: "Speed" (float), "MoveX", "MoveY" (float)

    private Rigidbody2D rb;
    private Vector2 input;

    /// <summary>Poslední směr pohybu (4 směry). Použije souboj pro útok a střelbu.</summary>
    public Vector2 Facing { get; private set; } = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (input != Vector2.zero)
        {
            Facing = Mathf.Abs(input.x) > Mathf.Abs(input.y)
                ? new Vector2(Mathf.Sign(input.x), 0f)
                : new Vector2(0f, Mathf.Sign(input.y));
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", input.sqrMagnitude);
            animator.SetFloat("MoveX", Facing.x);
            animator.SetFloat("MoveY", Facing.y);
        }
    }

    void FixedUpdate()
    {        
        PlayerStats stats = PlayerStats.Instance;

        if (stats != null && stats.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = moveSpeed * (stats != null ? stats.SpeedMultiplier : 1f);
        rb.linearVelocity = input.normalized * speed;
    }
}
