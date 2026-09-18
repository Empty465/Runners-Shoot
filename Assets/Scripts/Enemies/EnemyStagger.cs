using System.Collections;
using UnityEngine;

public class EnemyStagger : MonoBehaviour
{
    [SerializeField] private float staggerDuration = 0.25f;

    private EnemyHealth enemyHealth;
    private EnemyStun enemyStun;
    private Coroutine staggerRoutine;

    public bool IsStaggered { get; private set; }

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyStun = GetComponent<EnemyStun>();
    }

    private void OnEnable()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        if (enemyStun == null)
        {
            enemyStun = GetComponent<EnemyStun>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged += OnHealthChanged;
        }
    }

    private void OnHealthChanged(float currentHealth)
    {
        if (enemyHealth == null || enemyHealth.IsDead || (enemyStun != null && currentHealth <= enemyStun.HealthThreshold))
        {
            return;
        }

        Stagger();
    }

    public void Stagger()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            return;
        }

        if (staggerRoutine != null)
        {
            StopCoroutine(staggerRoutine);
        }

        staggerRoutine = StartCoroutine(StaggerRoutine());
    }

    public void CancelStagger()
    {
        if (staggerRoutine != null)
        {
            StopCoroutine(staggerRoutine);
            staggerRoutine = null;
        }

        IsStaggered = false;
    }

    private IEnumerator StaggerRoutine()
    {
        IsStaggered = true;
        Debug.Log("[Enemy] Staggered");

        yield return new WaitForSeconds(staggerDuration);

        IsStaggered = false;
        staggerRoutine = null;
        Debug.Log("[Enemy] Stagger Ended");
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged -= OnHealthChanged;
        }

        if (staggerRoutine != null)
        {
            StopCoroutine(staggerRoutine);
            staggerRoutine = null;
        }

        IsStaggered = false;
    }
}
