using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinText;

    public int CurrentCoins { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        CurrentCoins += amount;
        UpdateUI();
    }

    public bool SpendCoins(int amount)
    {
        if (CurrentCoins < amount) return false;

        CurrentCoins -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = CurrentCoins.ToString();
    }
}