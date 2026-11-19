using System.Collections;
using UnityEngine;

public class PlayerBlock : MonoBehaviour
{
    [Header("Block Settings")]
    [SerializeField] private KeyCode blockKey = KeyCode.Mouse1;
    [SerializeField] private int maxBlockedHits = 4;
    [SerializeField] private float guardBreakDuration = 1.5f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Health health;
    private bool isBlocking = false;
    private bool isGuardBroken = false;
    private int currentBlockedHits = 0;
    private Color originalColor;

    public bool IsBlocking => isBlocking;
    public int RemainingBlocks => Mathf.Max(0, maxBlockedHits - currentBlockedHits);
    public int MaxBlocks => maxBlockedHits;
    public bool IsGuardBroken => isGuardBroken;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visualsTransform == null && animator != null)
            visualsTransform = animator.transform;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (health != null && (health.IsDead || health.IsStunned))
        {
            SetBlocking(false);
            return;
        }

        if (isGuardBroken)
        {
            SetBlocking(false);
            return;
        }

        bool wantsBlock = Input.GetKey(blockKey);
        SetBlocking(wantsBlock);
    }

    private void SetBlocking(bool state)
    {
        if (isBlocking == state) return;
        isBlocking = state;

        if (animator != null)
            animator.SetBool("IsBlocking", isBlocking);
    }

    public bool TryBlockHit(float attackerX, bool hasAttackerPos, int shieldDamage)
    {
        if (!isBlocking || isGuardBroken) return false;

        if (hasAttackerPos && visualsTransform != null)
        {
            float facingDir = Mathf.Sign(visualsTransform.localScale.x);
            float dirToAttacker = Mathf.Sign(attackerX - transform.position.x);
            if (dirToAttacker != 0f && facingDir != dirToAttacker)
                return false;
        }

        currentBlockedHits += shieldDamage;

        if (animator != null)
            animator.SetTrigger("BlockImpact");

        if (CombatFeel.Instance != null)
            CombatFeel.Instance.TriggerImpact(0.03f, 0.06f, 0.08f);

        if (spriteRenderer != null)
            StartCoroutine(FlashShield());

        if (currentBlockedHits >= maxBlockedHits)
        {
            StartCoroutine(GuardBreakRoutine());
        }

        return true;
    }

    private IEnumerator GuardBreakRoutine()
    {
        isGuardBroken = true;
        SetBlocking(false);
        Debug.Log("GUARD BROKEN!");

        if (animator != null)
            animator.SetTrigger("Hurt");

        yield return new WaitForSeconds(guardBreakDuration);

        currentBlockedHits = 0;
        isGuardBroken = false;
    }

    private IEnumerator FlashShield()
    {
        spriteRenderer.color = new Color(0.75f, 0.9f, 1f);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}