using System.Collections;
using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    [SerializeField] private float stunDuration = 1.5f;
    [SerializeField] private float lowHealthStunDuration = 3f;
    [SerializeField] private float healthThresholdRatio = 0.3f;
    [SerializeField] private GameObject stunDebugVisual;

    private EnemyHealth enemyHealth;
    private Coroutine stunRoutine;

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
        if (enemyHealth == null || enemyHealth.IsDead || IsStunned)
        {
            return;
        }

        if (currentHealth <= HealthThreshold)
        {
            EnemyStagger enemyStagger = GetComponent<EnemyStagger>();
            if (enemyStagger != null)
            {
                enemyStagger.CancelStagger();
            }

            Stun(lowHealthStunDuration);
            Debug.Log("[Enemy] Health Threshold Stun");
        }
    }

    public void Stun()
    {
        Stun(stunDuration);
    }

    public void Stun(float duration)
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            return;
        }

        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
        }

        stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;
        SetStunVisual(true);
        Debug.Log("[Enemy] Stunned");

        yield return new WaitForSeconds(duration);

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
        SetStunVisual(false);
    }
}