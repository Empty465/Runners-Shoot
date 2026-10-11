using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeath : MonoBehaviour
{
    [SerializeField] private float deathPresentationDuration = 0.4f;
    [SerializeField] private float deathFallAngle = 78f;
    [SerializeField] private float deathShrinkAmount = 0.82f;
    [SerializeField] private float deathDropDistance = 0.35f;

    private EnemyHealth enemyHealth;
    private Transform enemyVisual;
    private Coroutine deathPresentationRoutine;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        Transform visual = transform.Find("EnemyDebugVisual");
        enemyVisual = visual != null ? visual : null;
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

        EnemyStatusDebugDisplay statusDisplay = GetComponent<EnemyStatusDebugDisplay>();
        if (statusDisplay != null)
        {
            statusDisplay.enabled = false;
        }

        if (enemyVisual != null)
        {
            deathPresentationRoutine = StartCoroutine(DeathPresentationRoutine());
        }

        Debug.Log("[Enemy] Died");
    }

    private IEnumerator DeathPresentationRoutine()
    {
        Vector3 startPosition = enemyVisual.localPosition;
        Vector3 startScale = enemyVisual.localScale;
        Quaternion startRotation = enemyVisual.localRotation;
        float facingDirection = Mathf.Sign(startScale.x);
        float duration = Mathf.Max(0.01f, deathPresentationDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            enemyVisual.localPosition = startPosition + Vector3.down * (deathDropDistance * progress);
            enemyVisual.localRotation = startRotation * Quaternion.Euler(0f, 0f, -facingDirection * deathFallAngle * progress);
            enemyVisual.localScale = new Vector3(
                startScale.x,
                startScale.y * Mathf.Lerp(1f, 1f - deathShrinkAmount, progress),
                startScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        enemyVisual.gameObject.SetActive(false);
        deathPresentationRoutine = null;
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged -= OnHealthChanged;
        }

        if (deathPresentationRoutine != null)
        {
            StopCoroutine(deathPresentationRoutine);
            deathPresentationRoutine = null;
        }
    }
}
