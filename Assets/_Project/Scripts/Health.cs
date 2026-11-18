using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float destroyDelay = 3f; // Set to 0 to keep corpses permanently

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentHealth;
    private bool isDead = false;
    private Color originalColor;
    private PlayerBlock playerBlock;

    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerBlock = GetComponent<PlayerBlock>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage, float attackerX = 0f, bool hasAttackerPos = false, int shieldDamage = 1)
    {
        if (isDead) return;

        if (playerBlock != null && playerBlock.TryBlockHit(attackerX, hasAttackerPos, shieldDamage))
        {
            return;
        }

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! HP left: {currentHealth}");

        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
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

        Debug.Log($"{gameObject.name} defeated!");

        if (animator != null)
            animator.SetBool("IsDead", true);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
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

        if (destroyDelay > 0f)
            Destroy(gameObject, destroyDelay);
    }
}