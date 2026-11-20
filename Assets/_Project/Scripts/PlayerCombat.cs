using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combo Timing")]
    [SerializeField] private float comboResetTime = 1.0f;
    [SerializeField] private float[] swingDurations = { 0.42f, 0.42f, 0.56f };
    [SerializeField] private float[] hitActiveDelays = { 0.14f, 0.14f, 0.20f };

    [Header("Per-Hit Stats (Hit 1, Hit 2, Hit 3)")]
    [SerializeField] private int[] comboDamages = { 20, 25, 40 };
    [SerializeField] private float[] comboKnockbacks = { 7f, 8.5f, 13f };

    [Header("Magic & Enchantment Visuals")]
    [SerializeField] private GameObject swordWavePrefab;
    [SerializeField] private GameObject bladeEnchantGlow; // Optional child Light2D or ParticleSystem

    [Header("Hitbox")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.85f;
    [SerializeField] private LayerMask enemyLayers;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsTransform;

    private PlayerBlock playerBlock;
    private Health health;
    private int currentComboStep = 0;
    private float lastAttackTime = -10f;
    private bool isAttacking = false;
    private Coroutine attackRoutine;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        playerBlock = GetComponent<PlayerBlock>();
        health = GetComponent<Health>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visualsTransform == null && animator != null)
            visualsTransform = animator.transform;

        if (bladeEnchantGlow != null)
            bladeEnchantGlow.SetActive(false);
    }

    private void Update()
    {
        if (ShopUI.Instance != null && ShopUI.Instance.IsOpen) return;
        if (health != null && (health.IsDead || health.IsStunned))
        {
            if (isAttacking) CancelAttack();
            return;
        }

        if (playerBlock != null && playerBlock.IsBlocking) return;

        if (!isAttacking && currentComboStep > 0 && Time.time - lastAttackTime > comboResetTime)
        {
            currentComboStep = 0;
        }

        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            StartComboAttack();
        }
    }

    public void EnableWeaponEnchantment(int bladeLevel)
    {
        if (bladeEnchantGlow != null && bladeLevel > 0)
            bladeEnchantGlow.SetActive(true);
    }

    private void StartComboAttack()
    {
        currentComboStep++;
        if (currentComboStep > 3)
            currentComboStep = 1;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(ExecuteSwingRoutine(currentComboStep));
    }

    private IEnumerator ExecuteSwingRoutine(int step)
    {
        isAttacking = true;
        int index = Mathf.Clamp(step - 1, 0, 2);

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetInteger("ComboStep", step);
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(hitActiveDelays[index]);

        if (health != null && (health.IsDead || health.IsStunned))
        {
            CancelAttack();
            yield break;
        }

        ApplySwingHitbox(index, step);
        TryFireSwordWave();

        float remainingTime = Mathf.Max(0.05f, swingDurations[index] - hitActiveDelays[index]);
        yield return new WaitForSeconds(remainingTime);

        lastAttackTime = Time.time;
        isAttacking = false;
        attackRoutine = null;
    }

    private void TryFireSwordWave()
    {
        if (swordWavePrefab == null || attackPoint == null || UpgradeManager.Instance == null) return;
        if (!UpgradeManager.Instance.CanCastSwordWave) return;

        float facingDir = visualsTransform != null ? Mathf.Sign(visualsTransform.localScale.x) : 1f;
        GameObject waveObj = Instantiate(swordWavePrefab, attackPoint.position, Quaternion.identity);
        SwordWaveProjectile wave = waveObj.GetComponent<SwordWaveProjectile>();
        if (wave != null)
            wave.Launch(facingDir, UpgradeManager.Instance.BonusDamage / 2);

        UpgradeManager.Instance.NotifySwordWaveCast();
    }

    private void ApplySwingHitbox(int index, int step)
    {
        if (attackPoint == null) return;

        int bonusDamage = UpgradeManager.Instance != null ? UpgradeManager.Instance.BonusDamage : 0;
        int damage = comboDamages[index] + bonusDamage;
        float knockback = comboKnockbacks[index];
        float facingDir = visualsTransform != null ? Mathf.Sign(visualsTransform.localScale.x) : 1f;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        bool landedHit = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsDead)
            {
                enemyHealth.TakeDamage(damage, transform.position.x, true, 1, knockback);
                landedHit = true;
            }
        }

        if (landedHit && CombatFeel.Instance != null)
        {
            if (step == 3)
                CombatFeel.Instance.TriggerImpact(0.065f, 0.15f, 0.18f, facingDir);
            else
                CombatFeel.Instance.TriggerImpact(0.03f, 0.07f, 0.11f, facingDir);
        }
    }

    private void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}