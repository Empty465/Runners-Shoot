using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private float remainingLifetime;
    private Transform ownerTransform;
    private bool appliesOverheat;
    private float burnDamagePerTick;
    private int burnTickCount;
    private float burnTickInterval;
    private float armorOnHit;
    private PlayerHealth ownerHealth;

    public void Initialize(
        Vector2 projectileDirection,
        float projectileSpeed,
        float projectileDamage,
        float lifetime,
        Transform owner,
        bool overheat = false,
        float burnDamage = 0f,
        int burnTicks = 0,
        float burnInterval = 0f,
        float armorReward = 0f,
        PlayerHealth shooterHealth = null)
    {
        direction = projectileDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        remainingLifetime = lifetime;
        ownerTransform = owner;
        appliesOverheat = overheat;
        burnDamagePerTick = burnDamage;
        burnTickCount = burnTicks;
        burnTickInterval = burnInterval;
        armorOnHit = armorReward;
        ownerHealth = shooterHealth;
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

        if (other.GetComponentInParent<PlayerProjectile>() != null)
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            if (appliesOverheat)
            {
                if (!enemyHealth.IsDead)
                {
                    EnemyBurnEffect burnEffect = enemyHealth.GetComponent<EnemyBurnEffect>();
                    if (burnEffect == null)
                    {
                        burnEffect = enemyHealth.gameObject.AddComponent<EnemyBurnEffect>();
                    }

                    burnEffect.Apply(burnDamagePerTick, burnTickCount, burnTickInterval);
                }

                if (ownerHealth != null)
                {
                    ownerHealth.RestoreArmor(armorOnHit);
                }
            }
        }

        Debug.Log($"[PlayerWeapon] Projectile Hit: {other.name}");
        Destroy(gameObject);
    }
}
