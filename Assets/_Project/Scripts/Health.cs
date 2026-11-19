using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float destroyDelay = 3f;

    [Header("Knockback & Poise")]
    [SerializeField] private float knockbackResistance = 1f;
    [SerializeField] private float defaultKnockbackForce = 7f;
    [SerializeField] private float hurtStunDuration = 0.2f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Loot Drops")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoins = 2;
    [SerializeField] private int maxCoins = 4;

    private int currentHealth;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isInvulnerable = false;
    private Color originalColor;
    private Rigidbody2D rb;
    private PlayerBlock playerBlock;

    public bool IsDead => isDead;
    public bool IsStunned => isStunned;
    public bool IsInvulnerable { get => isInvulnerable; set => isInvulnerable = value; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public Action<int, int> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        playerBlock = GetComponent<PlayerBlock>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage, float attackerX = 0f, bool hasAttackerPos = false, int shieldDamage = 1, float customKnockback = -1f)
    {
        if (isDead || isInvulnerable) return;

        if (playerBlock != null && playerBlock.TryBlockHit(attackerX, hasAttackerPos, shieldDamage))
        {
            ApplyKnockback(attackerX, hasAttackerPos, 3.5f);
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {damage} damage! HP left: {currentHealth}");

        float kbForce = customKnockback >= 0f ? customKnockback : defaultKnockbackForce;
        ApplyKnockback(attackerX, hasAttackerPos, kbForce);

        if (CombatFeel.Instance != null)
            CombatFeel.Instance.TriggerImpact(0.04f, 0.12f, 0.12f, hasAttackerPos ? Mathf.Sign(transform.position.x - attackerX) : 0f);

        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("Hurt");

            StartCoroutine(StunRoutine());
        }
    }

    private void ApplyKnockback(float attackerX, bool hasAttackerPos, float force)
    {
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic || !hasAttackerPos || force <= 0f) return;

        float pushDir = Mathf.Sign(transform.position.x - attackerX);
        if (pushDir == 0f) pushDir = 1f;

        float finalForce = force / Mathf.Max(0.25f, knockbackResistance);
        rb.linearVelocity = new Vector2(pushDir * finalForce, 2.2f);
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;
        yield return new WaitForSeconds(hurtStunDuration);
        isStunned = false;
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (CombatFeel.Instance != null)
            CombatFeel.Instance.TriggerImpact(0.07f, 0.2f, 0.2f);

        if (animator != null)
            animator.SetBool("IsDead", true);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.StopAllCoroutines();
            ai.enabled = false;
        }

        if (playerBlock != null)
            playerBlock.enabled = false;

        MonoBehaviour playerController = GetComponent("PlayerController") as MonoBehaviour;
        if (playerController != null)
            playerController.enabled = false;

        MonoBehaviour playerCombat = GetComponent("PlayerCombat") as MonoBehaviour;
        if (playerCombat != null)
            playerCombat.enabled = false;

        if (coinPrefab != null)
        {
            int count = UnityEngine.Random.Range(minCoins, maxCoins + 1);
            for (int i = 0; i < count; i++)
            {
                Instantiate(coinPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }

        if (destroyDelay > 0f)
            Destroy(gameObject, destroyDelay);
    }
}