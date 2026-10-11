using UnityEngine;

public class PlayerExecution : MonoBehaviour
{
    [Header("Execution")]
    [SerializeField] private float executionRange = 2f;
    [SerializeField] private float healthThresholdRatio = 0.3f;
    [SerializeField] private LayerMask targetLayers = ~0;

    [Header("Execution Rewards")]
    [SerializeField] private float healthReward = 40f;
    [SerializeField] private int ammoReward = 5;

    private PlayerHealth playerHealth;
    private PlayerWeapon playerWeapon;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    // Called by PlayerMelee on an E tap; returns true when an enemy was executed.
    public bool TryExecute()
    {
        EnemyHealth target = FindExecutionTarget();
        if (target == null)
        {
            return false;
        }

        EnemyStun enemyStun = target.GetComponent<EnemyStun>();
        if (enemyStun == null || !enemyStun.IsStunned)
        {
            return false;
        }

        float healthThreshold = target.MaxHealth * Mathf.Clamp01(healthThresholdRatio);
        if (target.CurrentHealth > healthThreshold)
        {
            return false;
        }

        target.TakeDamage(target.CurrentHealth);
        playerHealth?.RestoreHealth(healthReward);
        playerWeapon?.RestoreAmmo(ammoReward);
        Debug.Log("[PlayerExecution] EXECUTED");
        return true;
    }

    private EnemyHealth FindExecutionTarget()
    {
        Collider2D[] nearbyColliders = targetLayers.value == 0
            ? Physics2D.OverlapCircleAll(transform.position, executionRange)
            : Physics2D.OverlapCircleAll(transform.position, executionRange, targetLayers);
        EnemyHealth closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D nearbyCollider in nearbyColliders)
        {
            EnemyHealth enemyHealth = nearbyCollider.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, enemyHealth.transform.position);
            if (distance < closestDistance)
            {
                closestTarget = enemyHealth;
                closestDistance = distance;
            }
        }

        return closestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, executionRange);
    }
}
