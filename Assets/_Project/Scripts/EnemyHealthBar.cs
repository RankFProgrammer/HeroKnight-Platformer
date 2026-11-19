using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health targetHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject barContainer;

    [Header("Behavior")]
    [SerializeField] private bool hideWhenFull = true;

    private void Awake()
    {
        if (targetHealth == null)
            targetHealth = GetComponentInParent<Health>();

        if (barContainer == null)
            barContainer = gameObject;
    }

    private void Start()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += HandleHealthChanged;
            UpdateVisuals(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void LateUpdate()
    {
        // Keep bar upright and never flipped even if parent flips
        transform.rotation = Quaternion.identity;
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        float signX = parentScale.x < 0f ? -1f : 1f;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * signX;
        transform.localScale = localScale;
    }

    private void HandleHealthChanged(int current, int max)
    {
        UpdateVisuals(current, max);
    }

    private void UpdateVisuals(int current, int max)
    {
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        if (fillImage != null)
            fillImage.fillAmount = ratio;

        if (barContainer != null)
        {
            if (current <= 0)
                barContainer.SetActive(false);
            else if (hideWhenFull)
                barContainer.SetActive(current < max);
            else
                barContainer.SetActive(true);
        }
    }
}