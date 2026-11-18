using System.Collections;
using UnityEngine;

public class PlayerBlock : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private int maxBlockedHits = 2;
    [SerializeField] private float guardBreakCooldown = 2.5f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentBlockedHits = 0;
    private bool isBlocking = false;
    private bool isGuardBroken = false;

    public bool IsBlocking => isBlocking;

    private void Update()
    {
        if (isGuardBroken)
        {
            SetBlocking(false);
            return;
        }

        if (Input.GetMouseButton(1))
        {
            SetBlocking(true);
        }
        else
        {
            SetBlocking(false);
        }
    }

    private void SetBlocking(bool state)
    {
        if (isBlocking == state) return;

        isBlocking = state;
        if (animator != null)
            animator.SetBool("IsBlocking", isBlocking);
    }

    public bool TryBlockHit(float attackerX, bool hasAttackerPos, int shieldDamage = 1)
    {
        if (!isBlocking || isGuardBroken)
            return false;

        if (hasAttackerPos && visualsTransform != null)
        {
            float facingDir = Mathf.Sign(visualsTransform.localScale.x);
            float dirToAttacker = Mathf.Sign(attackerX - transform.position.x);
            if (facingDir != dirToAttacker)
                return false;
        }

        currentBlockedHits += shieldDamage;
        Debug.Log($"Blocked hit! ({currentBlockedHits}/{maxBlockedHits})");

        if (animator != null)
            animator.SetTrigger("BlockImpact");

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
        Debug.Log($"GUARD BROKEN! Shield disabled for {guardBreakCooldown}s!");

        if (spriteRenderer != null)
        {
            Color orig = spriteRenderer.color;
            spriteRenderer.color = new Color(1f, 0.65f, 0f);
            yield return new WaitForSeconds(0.25f);
            spriteRenderer.color = orig;
        }

        yield return new WaitForSeconds(guardBreakCooldown);

        currentBlockedHits = 0;
        isGuardBroken = false;
        Debug.Log("Shield recovered!");
    }
}