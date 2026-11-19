using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float popForce = 4f;
    [SerializeField] private float magnetRange = 2.5f;
    [SerializeField] private float magnetSpeed = 8f;

    private Rigidbody2D rb;
    private Transform player;
    private float pickupDelay = 0.25f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Pop upward in a random arc when spawned
        if (rb != null)
        {
            Vector2 randomDir = new Vector2(Random.Range(-1.2f, 1.2f), Random.Range(1f, 1.5f));
            rb.AddForce(randomDir * popForce, ForceMode2D.Impulse);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (pickupDelay > 0f)
        {
            pickupDelay -= Time.deltaTime;
            return;
        }

        // Gently pull coin toward the player when close
        if (player != null && Vector2.Distance(transform.position, player.position) <= magnetRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickupDelay > 0f) return;

        if (other.CompareTag("Player"))
        {
            if (CoinManager.Instance != null)
                CoinManager.Instance.AddCoins(coinValue);

            Destroy(gameObject);
        }
    }
}