using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeath : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged += OnHealthChanged;
            if (enemyHealth.IsDead)
            {
                Die();
            }
        }
    }

    private void OnHealthChanged(float currentHealth)
    {
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        EnemyAttack enemyAttack = GetComponent<EnemyAttack>();
        if (enemyAttack != null)
        {
            enemyAttack.enabled = false;
        }

        BasicEnemyController enemyController = GetComponent<BasicEnemyController>();
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        EnemyStun enemyStun = GetComponent<EnemyStun>();
        if (enemyStun != null)
        {
            enemyStun.enabled = false;
        }

        foreach (Collider2D enemyCollider in GetComponents<Collider2D>())
        {
            enemyCollider.enabled = false;
        }

        Rigidbody2D enemyRigidbody = GetComponent<Rigidbody2D>();
        if (enemyRigidbody != null)
        {
            enemyRigidbody.simulated = false;
        }

        Debug.Log("[Enemy] Died");
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged -= OnHealthChanged;
        }
    }
}
