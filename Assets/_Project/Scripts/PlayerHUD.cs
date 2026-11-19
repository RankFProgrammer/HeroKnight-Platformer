using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Target Player")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerBlock playerBlock;

    [Header("Health Bar Images")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image damageChipImage;
    [SerializeField] private float chipSpeed = 2.5f;

    [Header("Shield Guard Pips (4 Slots)")]
    [SerializeField] private Image[] shieldPips;
    [SerializeField] private Color activePipColor = new Color(0.25f, 0.75f, 1f);
    [SerializeField] private Color brokenPipColor = new Color(0.85f, 0.2f, 0.2f);
    [SerializeField] private Color emptyPipColor = new Color(0.18f, 0.18f, 0.22f, 0.7f);

    private float targetHealthFill = 1f;

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<Health>();
                if (playerBlock == null)
                    playerBlock = playerObj.GetComponent<PlayerBlock>();
            }
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            targetHealthFill = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
            if (healthFillImage != null) healthFillImage.fillAmount = targetHealthFill;
            if (damageChipImage != null) damageChipImage.fillAmount = targetHealthFill;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void Update()
    {
        // Smooth delayed damage bar drain
        if (damageChipImage != null && healthFillImage != null)
        {
            if (damageChipImage.fillAmount > healthFillImage.fillAmount)
            {
                damageChipImage.fillAmount = Mathf.MoveTowards(
                    damageChipImage.fillAmount,
                    healthFillImage.fillAmount,
                    chipSpeed * Time.deltaTime
                );
            }
            else
            {
                damageChipImage.fillAmount = healthFillImage.fillAmount;
            }
        }

        // Update Shield Guard pips
        if (playerBlock != null && shieldPips != null)
        {
            int remaining = playerBlock.RemainingBlocks;
            bool broken = playerBlock.IsGuardBroken;

            for (int i = 0; i < shieldPips.Length; i++)
            {
                if (shieldPips[i] == null) continue;

                if (broken)
                    shieldPips[i].color = brokenPipColor;
                else if (i < remaining)
                    shieldPips[i].color = activePipColor;
                else
                    shieldPips[i].color = emptyPipColor;
            }
        }
    }

    private void UpdateHealthBar(int current, int max)
    {
        targetHealthFill = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        if (healthFillImage != null)
            healthFillImage.fillAmount = targetHealthFill;
    }
}