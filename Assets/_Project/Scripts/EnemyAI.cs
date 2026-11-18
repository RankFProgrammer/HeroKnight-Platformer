using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement & Patrol")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolDistance = 3f;

    [Header("Detection & Combat")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.35f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackWindup = 0.35f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private int shieldDamage = 1;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsTransform;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 startPos;
    private int patrolDirection = 1;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    private void Start()
    {
        animator.SetBool("IsGrounded", true);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetFloat("Speed", 0f);
            FaceTarget(player.position.x);

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformAttack());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        float offsetFromStart = transform.position.x - startPos.x;

        if (offsetFromStart >= patrolDistance)
            patrolDirection = -1;
        else if (offsetFromStart <= -patrolDistance)
            patrolDirection = 1;

        rb.linearVelocity = new Vector2(patrolDirection * moveSpeed, rb.linearVelocity.y);
        animator.SetFloat("Speed", 1f);
        Flip(patrolDirection);
    }

    private void ChasePlayer()
    {
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * (moveSpeed * 1.3f), rb.linearVelocity.y);
        animator.SetFloat("Speed", 1f);
        Flip(dirX);
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackWindup);

        if (attackPoint != null)
        {
            Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, hitRadius, playerLayer);
            if (hitPlayer != null)
            {
                Health playerHealth = hitPlayer.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage, transform.position.x, true, shieldDamage);
                }
            }
        }

        yield return new WaitForSeconds(0.25f);
        isAttacking = false;
    }

    private void FaceTarget(float targetX)
    {
        float dirX = Mathf.Sign(targetX - transform.position.x);
        Flip(dirX);
    }

    private void Flip(float dirX)
    {
        if (dirX > 0.01f)
            visualsTransform.localScale = new Vector3(-1f, 1f, 1f);
        else if (dirX < -0.01f)
            visualsTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, hitRadius);
        }
    }
}