using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("Panel & Quick-Toggle")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private KeyCode debugToggleKey = KeyCode.B; // Lets you open shop anywhere while testing

    [Header("Blade Upgrade UI")]
    [SerializeField] private TextMeshProUGUI bladeTitleText;
    [SerializeField] private TextMeshProUGUI bladeCostText;
    [SerializeField] private Button bladeBuyButton;

    [Header("Armor Upgrade UI")]
    [SerializeField] private TextMeshProUGUI armorTitleText;
    [SerializeField] private TextMeshProUGUI armorCostText;
    [SerializeField] private Button armorBuyButton;

    [Header("Health Potion UI")]
    [SerializeField] private TextMeshProUGUI potionTitleText;
    [SerializeField] private TextMeshProUGUI potionCostText;
    [SerializeField] private Button potionBuyButton;

    [Header("Sword Wave Unlock UI")]
    [SerializeField] private TextMeshProUGUI waveTitleText;
    [SerializeField] private TextMeshProUGUI waveCostText;
    [SerializeField] private Button waveBuyButton;

    public bool IsOpen => shopPanel != null && shopPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void Start()
    {
        if (bladeBuyButton != null) bladeBuyButton.onClick.AddListener(OnBuyBlade);
        if (armorBuyButton != null) armorBuyButton.onClick.AddListener(OnBuyArmor);
        if (potionBuyButton != null) potionBuyButton.onClick.AddListener(OnBuyPotion);
        if (waveBuyButton != null) waveBuyButton.onClick.AddListener(OnBuySwordWave);

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradesChanged += RefreshUI;
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradesChanged -= RefreshUI;
    }

    private void Update()
    {
        if (Input.GetKeyDown(debugToggleKey))
        {
            ToggleShop();
        }
        else if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    public void ToggleShop()
    {
        if (IsOpen) CloseShop();
        else OpenShop();
    }

    public void OpenShop()
    {
        if (shopPanel == null) return;
        shopPanel.SetActive(true);
        RefreshUI();
    }

    public void CloseShop()
    {
        if (shopPanel == null) return;
        shopPanel.SetActive(false);
    }

    public void RefreshUI()
    {
        if (UpgradeManager.Instance == null || CoinManager.Instance == null) return;

        var upg = UpgradeManager.Instance;
        int coins = CoinManager.Instance.CurrentCoins;

        if (bladeTitleText != null) bladeTitleText.text = $"Flaming Blade (Lv.{upg.BladeLevel})";
        if (bladeCostText != null) bladeCostText.text = $"{upg.BladeCost} Coins";
        if (bladeBuyButton != null) bladeBuyButton.interactable = coins >= upg.BladeCost;

        if (armorTitleText != null) armorTitleText.text = $"Reinforced Armor (Lv.{upg.ArmorLevel})";
        if (armorCostText != null) armorCostText.text = $"{upg.ArmorCost} Coins";
        if (armorBuyButton != null) armorBuyButton.interactable = coins >= upg.ArmorCost;

        if (potionTitleText != null) potionTitleText.text = $"Health Potion [H] (Owned: {upg.PotionCount})";
        if (potionCostText != null) potionCostText.text = $"{upg.PotionCost} Coins";
        if (potionBuyButton != null) potionBuyButton.interactable = coins >= upg.PotionCost;

        if (waveTitleText != null) waveTitleText.text = upg.HasSwordWave ? "Arcane Sword Wave (UNLOCKED)" : "Arcane Sword Wave";
        if (waveCostText != null) waveCostText.text = upg.HasSwordWave ? "OWNED" : $"{upg.SwordWaveCost} Coins";
        if (waveBuyButton != null) waveBuyButton.interactable = !upg.HasSwordWave && coins >= upg.SwordWaveCost;
    }

    private void OnBuyBlade() { UpgradeManager.Instance?.BuyBladeUpgrade(); RefreshUI(); }
    private void OnBuyArmor() { UpgradeManager.Instance?.BuyArmorUpgrade(); RefreshUI(); }
    private void OnBuyPotion() { UpgradeManager.Instance?.BuyPotion(); RefreshUI(); }
    private void OnBuySwordWave() { UpgradeManager.Instance?.BuySwordWave(); RefreshUI(); }
}