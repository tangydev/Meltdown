using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float acceleration = 60f;
    public float deceleration = 50f;
    public float airControl = 0.65f;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.2f;

    [Header("Shrinking")]
    public float shrinkSpeed = 0.01f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.08f;

    // Internal
    Rigidbody2D rb;
    float playerSize = 1f;
    float horizontalInput;
    float coyoteTimeCounter;
    float jumpBufferCounter;
    bool isGrounded;
    bool isJumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // --- Input ---
        horizontalInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontalInput = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontalInput = 1f;

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // Shrink
        playerSize -= Time.deltaTime * shrinkSpeed;

        print(playerSize);
        if (playerSize <= 0)
        {
            // you lose UI element and restart to title screen
            UnityEngine.SceneManagement.SceneManager.LoadScene("Melted");
        }
    }

    void FixedUpdate()
    {
        // Ground check (raycast is more reliable than collision events)
        isGrounded = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            (playerSize * 0.5f) + groundCheckDistance,
            groundLayer
        );

        // Coyote time
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.fixedDeltaTime;

        // Horizontal movement with acceleration + air control
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        if (!isGrounded)
            accelRate *= airControl;

        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right);

        // DEBUG - remove once jump is confirmed working
        Debug.Log($"grounded={isGrounded} coyote={coyoteTimeCounter:F2} buffer={jumpBufferCounter:F2} isJumping={isJumping}");

        // Jump
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            isJumping = true;
        }

        // Better falling feel
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !Keyboard.current.kKey.isPressed)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * Time.fixedDeltaTime;
        }

        // Reset jump flag when falling
        if (rb.linearVelocity.y < 0f)
            isJumping = false;
    }

    void LateUpdate()
    {
        transform.localScale = new Vector3(playerSize, playerSize, 1f);
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        float distance = (Application.isPlaying ? playerSize * 0.5f : 0.5f) + groundCheckDistance;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * distance);
    }
}