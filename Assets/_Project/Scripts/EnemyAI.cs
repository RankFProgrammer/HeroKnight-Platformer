using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum AIState
    {
        Guard,
        Telegraph,
        Charge,
        Attacking,
        Recover
    }


    [SerializeField] private float detectionRange = 6.5f;
    [SerializeField] private float loseAggroRange = 10f;
    [SerializeField] private float telegraphDuration = 0.35f;
    [SerializeField] private float chargeSpeed = 5.5f;

    [SerializeField] private float attackRange = 1.35f;
    [SerializeField] private float attackWindup = 0.35f;
    [SerializeField] private float recoveryDuration = 0.85f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private int shieldDamage = 1;
    [SerializeField] private bool canBeInterrupted = true;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer;


    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsTransform;
    [SerializeField] private GameObject alertIcon; 

    private Rigidbody2D rb;
    private Health health;
    private Transform player;
    private AIState currentState = AIState.Guard;
    private Coroutine activeRoutine;
    private float facingSign = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visualsTransform == null && animator != null)
            visualsTransform = animator.transform;

        if (alertIcon != null)
            alertIcon.SetActive(false);
    }

    private void Start()
    {
        if (animator != null)
            animator.SetBool("IsGrounded", true);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            if (alertIcon != null) alertIcon.SetActive(false);
            return;
        }

        if (health != null && health.IsStunned)
        {
            if (canBeInterrupted && currentState != AIState.Guard)
            {
                StopActiveRoutine();
                currentState = AIState.Recover;
                activeRoutine = StartCoroutine(RecoverRoutine(0.35f));
            }
            return;
        }

        if (player == null)
        {
            StandStill();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case AIState.Guard:
                StandStill();
                if (animator != null) animator.SetBool("IsAlert", false);

                if (distanceToPlayer <= detectionRange)
                {
                    FaceTarget(player.position.x);
                    StartStateRoutine(AIState.Telegraph, TelegraphRoutine());
                }
                break;

            case AIState.Telegraph:
                StandStill();
                FaceTarget(player.position.x);
                break;

            case AIState.Charge:
                if (distanceToPlayer > loseAggroRange)
                {
                    currentState = AIState.Guard;
                    if (animator != null) animator.SetBool("IsAlert", false);
                    StandStill();
                    return;
                }

                if (distanceToPlayer <= attackRange)
                {
                    StandStill();
                    FaceTarget(player.position.x);
                    StartStateRoutine(AIState.Attacking, AttackRoutine());
                }
                else
                {
                    float dirX = Mathf.Sign(player.position.x - transform.position.x);
                    rb.linearVelocity = new Vector2(dirX * chargeSpeed, rb.linearVelocity.y);
                    if (animator != null) animator.SetFloat("Speed", 1f);
                    Flip(dirX);
                }
                break;

            case AIState.Attacking:
            case AIState.Recover:
                StandStill();
                break;
        }
    }

    private void StandStill()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (animator != null) animator.SetFloat("Speed", 0f);
    }

    private void StartStateRoutine(AIState newState, IEnumerator routine)
    {
        StopActiveRoutine();
        currentState = newState;
        activeRoutine = StartCoroutine(routine);
    }

    private void StopActiveRoutine()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        if (visualsTransform != null)
            visualsTransform.localScale = new Vector3(facingSign, 1f, 1f);

        if (alertIcon != null)
            alertIcon.SetActive(false);
    }

    private IEnumerator TelegraphRoutine()
    {
        if (animator != null)
            animator.SetBool("IsAlert", true);

        if (alertIcon != null)
            alertIcon.SetActive(true);

        float halfTime = telegraphDuration * 0.5f;
        float elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfTime;
            float scaleX = Mathf.Lerp(1f, 1.16f, t);
            float scaleY = Mathf.Lerp(1f, 0.84f, t);
            if (visualsTransform != null)
                visualsTransform.localScale = new Vector3(facingSign * scaleX, scaleY, 1f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfTime;
            float scaleX = Mathf.Lerp(1.16f, 1f, t);
            float scaleY = Mathf.Lerp(0.84f, 1f, t);
            if (visualsTransform != null)
                visualsTransform.localScale = new Vector3(facingSign * scaleX, scaleY, 1f);
            yield return null;
        }

        if (visualsTransform != null)
            visualsTransform.localScale = new Vector3(facingSign, 1f, 1f);

        if (alertIcon != null)
            alertIcon.SetActive(false);

        currentState = AIState.Charge;
        activeRoutine = null;
    }

    private IEnumerator AttackRoutine()
    {
        if (animator != null)
            animator.SetInteger("ComboStep", 1);
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackWindup);

        if (health != null && (health.IsDead || (canBeInterrupted && health.IsStunned)))
        {
            activeRoutine = null;
            yield break;
        }

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

        currentState = AIState.Recover;
        activeRoutine = StartCoroutine(RecoverRoutine(recoveryDuration));
    }

    private IEnumerator RecoverRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (player != null && Vector2.Distance(transform.position, player.position) <= loseAggroRange)
        {
            currentState = AIState.Charge;
        }
        else
        {
            currentState = AIState.Guard;
            if (animator != null) animator.SetBool("IsAlert", false);
        }

        activeRoutine = null;
    }

    private void FaceTarget(float targetX)
    {
        float dirX = Mathf.Sign(targetX - transform.position.x);
        Flip(dirX);
    }

    private void Flip(float dirX)
    {
        if (dirX > 0.01f)
            facingSign = -1f;
        else if (dirX < -0.01f)
            facingSign = 1f;

        if (visualsTransform != null)
            visualsTransform.localScale = new Vector3(facingSign, 1f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseAggroRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, hitRadius);
        }
    }
}