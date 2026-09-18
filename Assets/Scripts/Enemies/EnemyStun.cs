using System.Collections;
using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    [SerializeField] private float stunDuration = 1.5f;
    [SerializeField] private float healthThresholdRatio = 0.3f;
    [SerializeField] private GameObject stunDebugVisual;

    private EnemyHealth enemyHealth;
    private Coroutine stunRoutine;
    private bool healthThresholdTriggered;

    public bool IsStunned { get; private set; }
    public float HealthThreshold => enemyHealth != null
        ? enemyHealth.MaxHealth * Mathf.Clamp01(healthThresholdRatio)
        : 0f;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();

        if (stunDebugVisual == null)
        {
            Transform visualTransform = transform.Find("StunDebugVisual");
            stunDebugVisual = visualTransform != null ? visualTransform.gameObject : null;
        }

        SetStunVisual(false);
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
        }
    }

    private void OnHealthChanged(float currentHealth)
    {
        if (healthThresholdTriggered || enemyHealth == null || enemyHealth.IsDead)
        {
            return;
        }

        if (currentHealth <= HealthThreshold)
        {
            healthThresholdTriggered = true;
            EnemyStagger enemyStagger = GetComponent<EnemyStagger>();
            if (enemyStagger != null)
            {
                enemyStagger.CancelStagger();
            }

            Stun();
            Debug.Log("[Enemy] Health Threshold Stun");
        }
    }

    public void Stun()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            return;
        }

        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
        }

        stunRoutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        IsStunned = true;
        SetStunVisual(true);
        Debug.Log("[Enemy] Stunned");

        yield return new WaitForSeconds(stunDuration);

        IsStunned = false;
        SetStunVisual(false);
        stunRoutine = null;
        Debug.Log("[Enemy] Stun Ended");
    }

    private void SetStunVisual(bool active)
    {
        if (stunDebugVisual != null)
        {
            stunDebugVisual.SetActive(active);
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged -= OnHealthChanged;
        }

        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
            stunRoutine = null;
        }

        IsStunned = false;
        healthThresholdTriggered = false;
        SetStunVisual(false);
    }
}