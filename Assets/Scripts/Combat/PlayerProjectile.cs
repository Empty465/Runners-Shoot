using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private float remainingLifetime;
    private Transform ownerTransform;

    public void Initialize(Vector2 projectileDirection, float projectileSpeed, float projectileDamage, float lifetime, Transform owner)
    {
        direction = projectileDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        remainingLifetime = lifetime;
        ownerTransform = owner;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ownerTransform != null && (other.transform == ownerTransform || other.transform.IsChildOf(ownerTransform)))
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        Debug.Log($"[PlayerWeapon] Projectile Hit: {other.name}");
        Destroy(gameObject);
    }
}
