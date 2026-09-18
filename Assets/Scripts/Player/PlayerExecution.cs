using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerExecution : MonoBehaviour
{
    [Header("Execution")]
    [SerializeField] private float executionRange = 2f;
    [SerializeField] private float healthThresholdRatio = 0.3f;
    [SerializeField] private LayerMask targetLayers = ~0;

    [Header("Execution Rewards")]
    [SerializeField] private float healthReward = 2f;
    [SerializeField] private int ammoReward = 5;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private PlayerHealth playerHealth;
    private PlayerWeapon playerWeapon;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        InputActionMap playerActions = playerInput.actions.FindActionMap("Player", true);
        interactAction = playerActions.FindAction("Interact", true);
        playerHealth = GetComponent<PlayerHealth>();
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    private void OnEnable()
    {
        interactAction?.Enable();
    }

    private void OnDisable()
    {
        interactAction?.Disable();
    }

    private void Update()
    {
        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            TryExecute();
        }
    }

    private void TryExecute()
    {
        EnemyHealth target = FindExecutionTarget();
        if (target == null)
        {
            return;
        }

        EnemyStun enemyStun = target.GetComponent<EnemyStun>();
        if (enemyStun == null || !enemyStun.IsStunned)
        {
            return;
        }

        float healthThreshold = target.MaxHealth * Mathf.Clamp01(healthThresholdRatio);
        if (target.CurrentHealth > healthThreshold)
        {
            return;
        }

        target.TakeDamage(target.CurrentHealth);
        playerHealth?.RestoreHealth(healthReward);
        playerWeapon?.RestoreAmmo(ammoReward);
        Debug.Log("[PlayerExecution] EXECUTED");
    }

    private EnemyHealth FindExecutionTarget()
    {
        Collider2D[] nearbyColliders = targetLayers.value == 0
            ? Physics2D.OverlapCircleAll(transform.position, executionRange)
            : Physics2D.OverlapCircleAll(transform.position, executionRange, targetLayers);
        EnemyHealth closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D nearbyCollider in nearbyColliders)
        {
            EnemyHealth enemyHealth = nearbyCollider.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, enemyHealth.transform.position);
            if (distance < closestDistance)
            {
                closestTarget = enemyHealth;
                closestDistance = distance;
            }
        }

        return closestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, executionRange);
    }
}
