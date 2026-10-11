using UnityEngine;

public class EnemyBurnEffect : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private float damagePerTick;
    private float tickInterval;
    private float nextTickTime;
    private int remainingTicks;

    public bool IsBurning => remainingTicks > 0;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    public void Apply(float burnDamage, int tickCount, float interval)
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        if (enemyHealth == null || enemyHealth.IsDead || burnDamage <= 0f || tickCount <= 0 || interval <= 0f)
        {
            return;
        }

        damagePerTick = burnDamage;
        tickInterval = interval;
        remainingTicks = Mathf.Max(remainingTicks, tickCount);
        nextTickTime = Time.time + tickInterval;
    }

    private void Update()
    {
        if (remainingTicks <= 0 || Time.time < nextTickTime)
        {
            return;
        }

        if (enemyHealth == null || enemyHealth.IsDead)
        {
            remainingTicks = 0;
            return;
        }

        enemyHealth.TakeDamage(damagePerTick);
        remainingTicks--;
        nextTickTime = Time.time + tickInterval;
    }
}
