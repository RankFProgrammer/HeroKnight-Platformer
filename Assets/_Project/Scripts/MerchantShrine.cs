using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MerchantShrine : MonoBehaviour
{
    [SerializeField] private GameObject interactPrompt; // Optional world-space "[E] Shop" text/icon
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (ShopUI.Instance != null)
                ShopUI.Instance.ToggleShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
        if (ShopUI.Instance != null && ShopUI.Instance.IsOpen)
            ShopUI.Instance.CloseShop();
    }
}