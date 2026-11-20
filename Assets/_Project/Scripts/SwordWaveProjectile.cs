using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SwordWaveProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 13f;
    [SerializeField] private int damage = 22;
    [SerializeField] private float knockback = 6f;
    [SerializeField] private float lifetime = 1.6f;
    [SerializeField] private LayerMask enemyLayers;

    private float direction = 1f;

    public void Launch(float dir, int bonusDamage)
    {
        direction = Mathf.Sign(dir);
        if (direction == 0f) direction = 1f;
        damage += bonusDamage;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction * speed, 0f);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayers) == 0) return;

        Health enemyHealth = other.GetComponent<Health>();
        if (enemyHealth != null && !enemyHealth.IsDead)
        {
            enemyHealth.TakeDamage(damage, transform.position.x, true, 1, knockback);
            Destroy(gameObject);
        }
    }
}