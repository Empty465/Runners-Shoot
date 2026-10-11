using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    public event Action<float> HealthChanged;
    public float MaxHealth => maxHealth;
    public float CurrentHealth
    {
        get
        {
            EnsureInitialized();
            return currentHealth;
        }
        private set => currentHealth = value;
    }
    public bool IsDead => CurrentHealth <= 0f;

    private float currentHealth;
    private bool initialized;

    // Other components' OnEnable can run before this Awake, so initialize lazily too.
    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    public void ConfigureMaxHealth(float health)
    {
        EnsureInitialized();
        if (IsDead)
        {
            return;
        }

        maxHealth = Mathf.Max(1f, health);
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        HealthChanged?.Invoke(CurrentHealth);
        Debug.Log($"[Enemy] Damaged: {CurrentHealth}/{MaxHealth}");

        if (IsDead)
        {
            Debug.Log("[Enemy] Health Depleted");
        }
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth);
    }
}
