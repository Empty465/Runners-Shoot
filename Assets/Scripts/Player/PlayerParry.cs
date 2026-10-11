using System.Collections;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerParry : MonoBehaviour
{
    [Header("Parry")]
    [SerializeField] private float parryDuration = 0.32f;
    [SerializeField] private float parryCooldown = 1f;
    [SerializeField] private int enhancementPointsPerParry = 1;

    [Header("Parry References")]
    [SerializeField] private GameObject parryHitbox;
    [SerializeField] private GameObject parryDebugVisual;
    [SerializeField] private GameObject parrySuccessDebugVisual;

    private Coroutine parryRoutine;
    private Coroutine successVisualRoutine;
    private PlayerEnhancementPoints enhancementPoints;
    private PlayerCombatFeedbackVFX combatFeedbackVFX;
    private float nextParryTime;

    public bool IsParryActive { get; private set; }
    public bool CanParry => parryRoutine == null && Time.time >= nextParryTime;
    public float ParryCooldownRemaining => Mathf.Max(0f, nextParryTime - Time.time);
    public event Action ParrySucceeded;

    private void Awake()
    {
        enhancementPoints = GetComponent<PlayerEnhancementPoints>();
        if (enhancementPoints == null)
        {
            enhancementPoints = gameObject.AddComponent<PlayerEnhancementPoints>();
        }

        combatFeedbackVFX = GetComponent<PlayerCombatFeedbackVFX>();
        if (combatFeedbackVFX == null)
        {
            combatFeedbackVFX = gameObject.AddComponent<PlayerCombatFeedbackVFX>();
        }

        if (parryHitbox == null)
        {
            Transform hitboxTransform = transform.Find("ParryHitbox");
            parryHitbox = hitboxTransform != null ? hitboxTransform.gameObject : null;
        }

        if (parryDebugVisual == null)
        {
            Transform visualTransform = transform.Find("ParryDebugVisual");
            parryDebugVisual = visualTransform != null ? visualTransform.gameObject : null;
        }

        if (parrySuccessDebugVisual == null)
        {
            Transform visualTransform = transform.Find("ParrySuccessDebugVisual");
            parrySuccessDebugVisual = visualTransform != null ? visualTransform.gameObject : null;
        }

        SetParryVisuals(false);
        if (parrySuccessDebugVisual != null)
        {
            parrySuccessDebugVisual.SetActive(false);
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryStartParry();
        }
    }

    private void TryStartParry()
    {
        if (!CanParry)
        {
            return;
        }

        parryRoutine = StartCoroutine(ParryRoutine());
    }

    private IEnumerator ParryRoutine()
    {
        nextParryTime = Time.time + parryCooldown;
        IsParryActive = true;
        SetParryVisuals(true);
        Debug.Log("[Parry] Started");

        yield return new WaitForSeconds(parryDuration);

        IsParryActive = false;
        SetParryVisuals(false);
        parryRoutine = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsParryActive)
        {
            return;
        }

        EnemyAttack enemyAttack = other.GetComponentInParent<EnemyAttack>();
        if (enemyAttack == null || !enemyAttack.IsAttackActive || !enemyAttack.TryRegisterParry())
        {
            return;
        }

        EnemyStun enemyStun = enemyAttack.GetComponent<EnemyStun>();
        BasicEnemyController enemyController = enemyAttack.GetComponent<BasicEnemyController>();
        if (enemyController != null)
        {
            enemyController.RegisterParryStagger();
        }
        else if (enemyStun != null)
        {
            enemyStun.Stun();
        }

        enemyAttack.CancelCurrentAttack();
        RegisterParrySuccess();
    }

    public bool TryParryProjectile(EnemyProjectile projectile)
    {
        if (!IsParryActive || projectile == null || !projectile.TryRegisterParry())
        {
            return false;
        }

        RegisterParrySuccess();
        return true;
    }

    private void RegisterParrySuccess()
    {
        enhancementPoints.AddPoints(enhancementPointsPerParry);
        ParrySucceeded?.Invoke();
        ShowParrySuccess();
        combatFeedbackVFX.PlayParrySuccess();
        Debug.Log("[Parry] SUCCESS");
    }

    private void SetParryVisuals(bool active)
    {
        if (parryHitbox != null)
        {
            parryHitbox.SetActive(active);
        }

        if (parryDebugVisual != null)
        {
            parryDebugVisual.SetActive(active);
        }
    }

    private void ShowParrySuccess()
    {
        if (parrySuccessDebugVisual == null)
        {
            return;
        }

        if (successVisualRoutine != null)
        {
            StopCoroutine(successVisualRoutine);
        }

        successVisualRoutine = StartCoroutine(ParrySuccessVisualRoutine());
    }

    private IEnumerator ParrySuccessVisualRoutine()
    {
        parrySuccessDebugVisual.SetActive(true);
        yield return new WaitForSeconds(0.35f);
        parrySuccessDebugVisual.SetActive(false);
        successVisualRoutine = null;
    }

    private void OnDisable()
    {
        if (parryRoutine != null)
        {
            StopCoroutine(parryRoutine);
            parryRoutine = null;
        }

        if (successVisualRoutine != null)
        {
            StopCoroutine(successVisualRoutine);
            successVisualRoutine = null;
        }

        IsParryActive = false;
        SetParryVisuals(false);
        if (parrySuccessDebugVisual != null)
        {
            parrySuccessDebugVisual.SetActive(false);
        }
    }
}

public class EnemyProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private float remainingLifetime;
    private Transform owner;
    private bool parryable;
    private bool parryProcessed;

    public void Initialize(
        Vector2 projectileDirection,
        float projectileSpeed,
        float projectileDamage,
        bool canBeParried,
        Transform projectileOwner,
        float lifetime = 4f)
    {
        direction = projectileDirection.normalized;
        speed = Mathf.Max(0f, projectileSpeed);
        damage = Mathf.Max(0f, projectileDamage);
        parryable = canBeParried;
        owner = projectileOwner;
        remainingLifetime = Mathf.Max(0.1f, lifetime);
    }

    public bool TryRegisterParry()
    {
        if (!parryable || parryProcessed)
        {
            return false;
        }

        parryProcessed = true;
        BasicEnemyController enemyController = owner != null
            ? owner.GetComponent<BasicEnemyController>()
            : null;
        enemyController?.RegisterParryStagger();
        Destroy(gameObject);
        return true;
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
        if (owner != null && (other.transform == owner || other.transform.IsChildOf(owner)))
        {
            return;
        }

        PlayerParry playerParry = other.GetComponentInParent<PlayerParry>();
        if (playerParry != null && playerParry.TryParryProjectile(this))
        {
            return;
        }

        PlayerDodge playerDodge = other.GetComponentInParent<PlayerDodge>();
        if (playerDodge != null && playerDodge.TryRegisterSuccess())
        {
            Destroy(gameObject);
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}