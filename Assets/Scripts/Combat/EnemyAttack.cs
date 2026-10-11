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

    [Header("Attack Visuals")]
    [SerializeField] private Transform enemyVisual;
    [SerializeField] private float windupPulseSpeed = 14f;

    private BasicEnemyController enemyController;
    private EnemyStun enemyStun;
    private EnemyStagger enemyStagger;
    private Collider2D attackHitboxCollider;
    private MeshRenderer windupIndicatorRenderer;
    private MaterialPropertyBlock windupMaterialBlock;
    private Coroutine attackRoutine;
    private float nextAttackTime;
    private float windupStartedAt;
    private Vector3 initialWindupIndicatorScale;
    private Vector3 initialEnemyVisualScale;
    private Quaternion initialEnemyVisualRotation;
    private Color initialWindupColor = Color.white;
    private bool hasWindupColor;
    private bool parryProcessed;

    public event Action<Collider2D> AttackHit;
    public float AttackDamage => attackDamage;
    public bool IsWindupActive { get; private set; }
    public bool IsAttackActive => attackHitbox != null && attackHitbox.activeSelf;

    private void Awake()
    {
        enemyController = GetComponent<BasicEnemyController>();
        enemyStun = GetComponent<EnemyStun>();
        enemyStagger = GetComponent<EnemyStagger>();

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

        if (windupIndicator != null)
        {
            initialWindupIndicatorScale = windupIndicator.transform.localScale;
            windupIndicatorRenderer = windupIndicator.GetComponent<MeshRenderer>();
            if (windupIndicatorRenderer != null && windupIndicatorRenderer.sharedMaterial != null &&
                windupIndicatorRenderer.sharedMaterial.HasProperty("_White"))
            {
                initialWindupColor = windupIndicatorRenderer.sharedMaterial.GetColor("_White");
                windupMaterialBlock = new MaterialPropertyBlock();
                hasWindupColor = true;
            }
        }

        if (enemyVisual == null)
        {
            Transform visualTransform = transform.Find("EnemyDebugVisual");
            enemyVisual = visualTransform != null ? visualTransform : null;
        }

        if (enemyVisual != null)
        {
            initialEnemyVisualScale = enemyVisual.localScale;
            initialEnemyVisualRotation = enemyVisual.localRotation;
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
        UpdateAttackVisuals();

        if (playerTarget == null)
        {
            GameObject player = GameObject.Find("Player");
            playerTarget = player != null ? player.transform : null;
        }

        if (IsAttackActive && playerTarget != null)
        {
            PlayerDodge playerDodge = playerTarget.GetComponent<PlayerDodge>();
            if (playerDodge != null && playerDodge.TryRegisterSuccess())
            {
                CancelCurrentAttack();
                Debug.Log("[Dodge] SUCCESS");
                return;
            }
        }

        if (playerTarget == null || attackRoutine != null || Time.time < nextAttackTime ||
            (enemyStun != null && enemyStun.IsStunned) ||
            (enemyStagger != null && enemyStagger.IsStaggered))
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
        windupStartedAt = Time.time;
        if (windupIndicator != null)
        {
            windupIndicator.SetActive(true);
        }

        yield return new WaitForSeconds(attackWindup);

        IsWindupActive = false;
        if (windupIndicator != null)
        {
            windupIndicator.SetActive(false);
            ResetAttackVisuals();
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

        ResetAttackVisuals();
        nextAttackTime = Time.time + attackCooldown;
        Debug.Log("[Enemy] Attack Cancelled");
    }

    private void UpdateAttackVisuals()
    {
        if (IsWindupActive)
        {
            float pulse = (Mathf.Sin(Time.time * windupPulseSpeed) + 1f) * 0.5f;
            if (windupIndicator != null)
            {
                Transform indicatorTransform = windupIndicator.transform;
                indicatorTransform.localScale = new Vector3(
                    initialWindupIndicatorScale.x * Mathf.Lerp(0.8f, 1.2f, pulse),
                    initialWindupIndicatorScale.y * Mathf.Lerp(0.75f, 1.6f, pulse),
                    initialWindupIndicatorScale.z);
            }

            if (hasWindupColor && windupIndicatorRenderer != null)
            {
                float progress = attackWindup > 0f
                    ? Mathf.Clamp01((Time.time - windupStartedAt) / attackWindup)
                    : 1f;
                Color telegraphColor = Color.Lerp(new Color(1f, 0.58f, 0.14f), new Color(1f, 0.12f, 0.08f), progress);
                telegraphColor = Color.Lerp(telegraphColor, Color.white, pulse * 0.28f);
                windupIndicatorRenderer.GetPropertyBlock(windupMaterialBlock);
                windupMaterialBlock.SetColor("_White", telegraphColor);
                windupIndicatorRenderer.SetPropertyBlock(windupMaterialBlock);
            }

            if (enemyVisual != null)
            {
                enemyVisual.localScale = new Vector3(
                    enemyVisual.localScale.x,
                    initialEnemyVisualScale.y * Mathf.Lerp(1f, 0.9f, pulse),
                    initialEnemyVisualScale.z);
                int facingDirection = enemyController != null ? enemyController.FacingDirection : -1;
                enemyVisual.localRotation = initialEnemyVisualRotation *
                    Quaternion.Euler(0f, 0f, -8f * facingDirection * pulse);
            }
        }
        else if (IsAttackActive && enemyVisual != null)
        {
            int facingDirection = enemyController != null ? enemyController.FacingDirection : -1;
            enemyVisual.localScale = new Vector3(
                enemyVisual.localScale.x,
                initialEnemyVisualScale.y * 1.08f,
                initialEnemyVisualScale.z);
            enemyVisual.localRotation = initialEnemyVisualRotation *
                Quaternion.Euler(0f, 0f, 9f * facingDirection);
        }
        else
        {
            ResetAttackVisuals();
        }
    }

    private void ResetAttackVisuals()
    {
        if (windupIndicator != null)
        {
            windupIndicator.transform.localScale = initialWindupIndicatorScale;
        }

        if (hasWindupColor && windupIndicatorRenderer != null)
        {
            windupIndicatorRenderer.GetPropertyBlock(windupMaterialBlock);
            windupMaterialBlock.SetColor("_White", initialWindupColor);
            windupIndicatorRenderer.SetPropertyBlock(windupMaterialBlock);
        }

        if (enemyVisual != null)
        {
            enemyVisual.localScale = new Vector3(
                enemyVisual.localScale.x,
                initialEnemyVisualScale.y,
                initialEnemyVisualScale.z);
            enemyVisual.localRotation = initialEnemyVisualRotation;
        }
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

        PlayerDodge playerDodge = other.GetComponentInParent<PlayerDodge>();
        if (playerDodge != null && playerDodge.TryRegisterSuccess())
        {
            Debug.Log("[Dodge] SUCCESS");
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

        ResetAttackVisuals();
    }
}