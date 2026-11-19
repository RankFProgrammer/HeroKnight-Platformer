using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement & Jump")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Dodge Roll")]
    [SerializeField] private KeyCode rollKey = KeyCode.LeftShift;
    [SerializeField] private float rollSpeed = 12.5f;
    [SerializeField] private float rollDuration = 0.32f;
    [SerializeField] private float rollCooldown = 0.65f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsTransform;

    private Rigidbody2D rb;
    private Health health;
    private PlayerBlock playerBlock;
    private PlayerCombat playerCombat;
    private float moveInput;
    private bool isGrounded;
    private bool isRolling = false;
    private float nextRollTime = 0f;

    public bool IsRolling => isRolling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        playerBlock = GetComponent<PlayerBlock>();
        playerCombat = GetComponent<PlayerCombat>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visualsTransform == null && animator != null)
            visualsTransform = animator.transform;
    }

    private void Update()
    {
        if (health != null && (health.IsDead || health.IsStunned))
        {
            moveInput = 0f;
            return;
        }

        if (isRolling) return;

        if (Input.GetKeyDown(rollKey) && isGrounded && Time.time >= nextRollTime)
        {
            if (playerCombat == null || !playerCombat.IsAttacking)
            {
                StartCoroutine(DodgeRollRoutine());
                return;
            }
        }

        if (playerBlock != null && playerBlock.IsBlocking)
        {
            moveInput = 0f;
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0.01f && visualsTransform != null)
            visualsTransform.localScale = new Vector3(1f, 1f, 1f);
        else if (moveInput < -0.01f && visualsTransform != null)
            visualsTransform.localScale = new Vector3(-1f, 1f, 1f);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetFloat("YVelocity", rb.linearVelocity.y);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    private void FixedUpdate()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isRolling) return;

        if (health != null && (health.IsDead || health.IsStunned))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private IEnumerator DodgeRollRoutine()
    {
        isRolling = true;
        nextRollTime = Time.time + rollCooldown;

        if (health != null)
            health.IsInvulnerable = true;

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        if (animator != null)
            animator.SetTrigger("Roll");

        float rollDir = visualsTransform != null ? Mathf.Sign(visualsTransform.localScale.x) : 1f;
        if (moveInput != 0f)
        {
            rollDir = Mathf.Sign(moveInput);
            if (visualsTransform != null)
                visualsTransform.localScale = new Vector3(rollDir, 1f, 1f);
        }

        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            rb.linearVelocity = new Vector2(rollDir * rollSpeed, rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        if (health != null)
            health.IsInvulnerable = false;

        isRolling = false;
    }
}