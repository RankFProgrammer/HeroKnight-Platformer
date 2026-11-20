using System;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("1. Sharpened / Flaming Blade")]
    [SerializeField] private int bladeBaseCost = 10;
    [SerializeField] private int bladeCostIncrease = 5;
    [SerializeField] private int bonusDamagePerLevel = 8;

    [Header("2. Reinforced Armor")]
    [SerializeField] private int armorBaseCost = 10;
    [SerializeField] private int armorCostIncrease = 5;
    [SerializeField] private int bonusHealthPerLevel = 25;

    [Header("3. Health Potion")]
    [SerializeField] private int potionCost = 8;
    [SerializeField] private int potionHealAmount = 40;
    [SerializeField] private KeyCode usePotionKey = KeyCode.H;

    [Header("4. Arcane Sword Wave (Magic Unlock)")]
    [SerializeField] private int swordWaveCost = 25;
    [SerializeField] private float swordWaveCooldown = 2.0f;

    [Header("Player References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerCombat playerCombat;

    public int BladeLevel { get; private set; }
    public int ArmorLevel { get; private set; }
    public int PotionCount { get; private set; }
    public bool HasSwordWave { get; private set; }

    public int BonusDamage => BladeLevel * bonusDamagePerLevel;
    public int BladeCost => bladeBaseCost + (BladeLevel * bladeCostIncrease);
    public int ArmorCost => armorBaseCost + (ArmorLevel * armorCostIncrease);
    public int PotionCost => potionCost;
    public int SwordWaveCost => swordWaveCost;

    public float LastSwordWaveTime { get; private set; } = -10f;
    public bool CanCastSwordWave => HasSwordWave && Time.time >= LastSwordWaveTime + swordWaveCooldown;

    public Action OnUpgradesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (playerHealth == null || playerCombat == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (playerHealth == null) playerHealth = player.GetComponent<Health>();
                if (playerCombat == null) playerCombat = player.GetComponent<PlayerCombat>();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(usePotionKey))
        {
            UsePotion();
        }
    }

    public bool BuyBladeUpgrade()
    {
        if (CoinManager.Instance == null || !CoinManager.Instance.SpendCoins(BladeCost))
            return false;

        BladeLevel++;
        if (playerCombat != null)
            playerCombat.EnableWeaponEnchantment(BladeLevel);

        OnUpgradesChanged?.Invoke();
        return true;
    }

    public bool BuyArmorUpgrade()
    {
        if (CoinManager.Instance == null || !CoinManager.Instance.SpendCoins(ArmorCost))
            return false;

        ArmorLevel++;
        if (playerHealth != null)
            playerHealth.UpgradeMaxHealth(bonusHealthPerLevel, true);

        OnUpgradesChanged?.Invoke();
        return true;
    }

    public bool BuyPotion()
    {
        if (CoinManager.Instance == null || !CoinManager.Instance.SpendCoins(potionCost))
            return false;

        PotionCount++;
        OnUpgradesChanged?.Invoke();
        return true;
    }

    public bool BuySwordWave()
    {
        if (HasSwordWave || CoinManager.Instance == null || !CoinManager.Instance.SpendCoins(swordWaveCost))
            return false;

        HasSwordWave = true;
        OnUpgradesChanged?.Invoke();
        return true;
    }

    public void UsePotion()
    {
        if (PotionCount <= 0 || playerHealth == null || playerHealth.IsDead) return;
        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return;

        PotionCount--;
        playerHealth.Heal(potionHealAmount);
        OnUpgradesChanged?.Invoke();
    }

    public void NotifySwordWaveCast()
    {
        LastSwordWaveTime = Time.time;
    }
}