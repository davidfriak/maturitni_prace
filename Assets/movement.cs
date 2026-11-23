using UnityEngine;

public class movement : MonoBehaviour
{
    public enum Mode
    {
        TopDown,
        SideOn
    }

    public Mode mode = Mode.TopDown;

    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private bool grounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (mode == Mode.TopDown)
            TopDownMovement();
        else
            SideMovement();
    }

    void TopDownMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(x, y).normalized;
        rb.linearVelocity = movement * moveSpeed;
    }

    void SideMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            x * moveSpeed,
            rb.linearVelocity.y
        );

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            grounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0 &&
            collision.contacts[0].normal.y > 0.5f)
        {
            grounded = true;
        }
    }

    public void SetMode(Mode newMode)
    {
        mode = newMode;

        if (mode == Mode.TopDown)
        {
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = 2;
        }
    }
}