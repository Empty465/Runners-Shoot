using System;
using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackRange = 3.5f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackWindup = 0.5f;
    [SerializeField] private float attackActiveTime = 0.2f;

    [Header("Attack References")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private GameObject windupIndicator;
    [SerializeField] private float hitboxHorizontalOffset = 1.3f;

    private BasicEnemyController enemyController;
    private EnemyStun enemyStun;
    private Collider2D attackHitboxCollider;
    private Coroutine attackRoutine;
    private float nextAttackTime;
    private bool parryProcessed;

    public event Action<Collider2D> AttackHit;
    public float AttackDamage => attackDamage;
    public bool IsWindupActive { get; private set; }
    public bool IsAttackActive => attackHitbox != null && attackHitbox.activeSelf;

    private void Awake()
    {
        enemyController = GetComponent<BasicEnemyController>();
        enemyStun = GetComponent<EnemyStun>();

        if (playerTarget == null && enemyController != null)
        {
            playerTarget = enemyController.PlayerTarget;
        }

        if (playerTarget == null)
        {
            GameObject player = GameObject.Find("Player");
            playerTarget = player != null ? player.transform : null;
        }

        if (attackHitbox == null)
        {
            Transform hitboxTransform = transform.Find("AttackHitbox");
            attackHitbox = hitboxTransform != null ? hitboxTransform.gameObject : null;
        }

        if (windupIndicator == null)
        {
            Transform indicatorTransform = transform.Find("AttackWindupIndicator");
            windupIndicator = indicatorTransform != null ? indicatorTransform.gameObject : null;
        }

        if (attackHitbox != null)
        {
            attackHitboxCollider = attackHitbox.GetComponent<Collider2D>();
            attackHitbox.SetActive(false);
        }

        if (windupIndicator != null)
        {
            windupIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.Find("Player");
            playerTarget = player != null ? player.transform : null;
        }

        if (playerTarget == null || attackRoutine != null || Time.time < nextAttackTime || (enemyStun != null && enemyStun.IsStunned))
        {
            return;
        }

        if (Vector2.Distance(transform.position, playerTarget.position) <= attackRange)
        {
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        nextAttackTime = Time.time + attackCooldown;
        parryProcessed = false;
        IsWindupActive = true;
        if (windupIndicator != null)
        {
            windupIndicator.SetActive(true);
        }

        yield return new WaitForSeconds(attackWindup);

        IsWindupActive = false;
        if (windupIndicator != null)
        {
            windupIndicator.SetActive(false);
        }

        PositionAttackHitbox();
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }

        yield return new WaitForSeconds(attackActiveTime);

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        attackRoutine = null;
    }

    public bool TryRegisterParry()
    {
        if (!IsAttackActive || parryProcessed)
        {
            return false;
        }

        parryProcessed = true;
        return true;
    }

    public void CancelCurrentAttack()
    {
        if (attackRoutine == null)
        {
            return;
        }

        StopCoroutine(attackRoutine);
        attackRoutine = null;
        IsWindupActive = false;

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        if (windupIndicator != null)
        {
            windupIndicator.SetActive(false);
        }

        nextAttackTime = Time.time + attackCooldown;
        Debug.Log("[Enemy] Attack Cancelled");
    }

    private void PositionAttackHitbox()
    {
        if (attackHitbox == null)
        {
            return;
        }

        int facingDirection = enemyController != null ? enemyController.FacingDirection : -1;
        Vector3 localPosition = attackHitbox.transform.localPosition;
        localPosition.x = Mathf.Abs(hitboxHorizontalOffset) * facingDirection;
        attackHitbox.transform.localPosition = localPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAttackActive || playerTarget == null || other.transform.root != playerTarget.root)
        {
            return;
        }

        PlayerParry playerParry = other.GetComponentInParent<PlayerParry>();
        if (playerParry != null && playerParry.IsParryActive)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        AttackHit?.Invoke(other);
    }

    private void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        IsWindupActive = false;
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        if (windupIndicator != null)
        {
            windupIndicator.SetActive(false);
        }
    }
}