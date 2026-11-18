using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement & Jump")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsTransform;

    private Rigidbody2D rb;
    private PlayerBlock playerBlock;
    private float moveInput;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerBlock = GetComponent<PlayerBlock>();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool isBlocking = playerBlock != null && playerBlock.IsBlocking;

        if (Input.GetButtonDown("Jump") && isGrounded && !isBlocking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (!isBlocking)
        {
            if (moveInput > 0)
            {
                visualsTransform.localScale = new Vector3(1, 1, 1);
            }
            else if (moveInput < 0)
            {
                visualsTransform.localScale = new Vector3(-1, 1, 1);
            }
        }

        animator.SetFloat("Speed", isBlocking ? 0f : Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
    }

    private void FixedUpdate()
    {
        bool isBlocking = playerBlock != null && playerBlock.IsBlocking;
        float currentSpeed = isBlocking ? 0f : moveSpeed;

        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}