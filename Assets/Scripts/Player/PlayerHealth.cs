using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Armor")]
    [SerializeField] private float maxArmor = 100f;
    [SerializeField] private float startingArmor;

    public event Action<float> HealthChanged;
    public event Action<float> ArmorChanged;
    public event Action<bool> DamageTaken;
    public event Action Died;
    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public float MaxArmor => maxArmor;
    public float CurrentArmor { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        maxArmor = Mathf.Max(0f, maxArmor);
        CurrentHealth = maxHealth;
        CurrentArmor = Mathf.Clamp(startingArmor, 0f, maxArmor);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
        {
            return;
        }

        if (CurrentArmor > 0f)
        {
            CurrentArmor = Mathf.Max(0f, CurrentArmor - damage);
            DamageTaken?.Invoke(true);
            ArmorChanged?.Invoke(CurrentArmor);
            Debug.Log($"[Player] Armor Damaged: {CurrentArmor}/{MaxArmor}");
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        DamageTaken?.Invoke(false);
        HealthChanged?.Invoke(CurrentHealth);
        Debug.Log($"[Player] Damaged: {CurrentHealth}/{MaxHealth}");

        if (IsDead)
        {
            Debug.Log("[Player] Defeated");
            Died?.Invoke();
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

    public void RestoreArmor(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        CurrentArmor = Mathf.Min(maxArmor, CurrentArmor + amount);
        ArmorChanged?.Invoke(CurrentArmor);
    }
}
