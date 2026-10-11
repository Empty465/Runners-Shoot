using UnityEngine;

public enum EnemyArchetype
{
    Slag,
    Pawn,
    Knight,
    Archer,
    Baron
}

public class BasicEnemyController : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private EnemyArchetype archetype = EnemyArchetype.Knight;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform visual;

    [Header("Movement")]
    [SerializeField] private float slagMoveSpeed = 1.6f;
    [SerializeField] private float pawnMoveSpeed = 2.2f;
    [SerializeField] private float knightMoveSpeed = 3.2f;
    [SerializeField] private float archerMoveSpeed = 2.4f;
    [SerializeField] private float baronMoveSpeed = 3.5f;
    [SerializeField] private float movementDeadZone = 0.08f;

    private Rigidbody2D enemyRigidbody;
    private EnemyHealth enemyHealth;
    private EnemyAttack enemyAttack;
    private EnemyStun enemyStun;
    private EnemyStagger enemyStagger;
    private Renderer[] visualRenderers;
    private MaterialPropertyBlock visualMaterialBlock;
    private Vector3 initialVisualScale;
    private float initialVisualWidth;
    private float initialVisualHeight;
    private int parriesReceived;
    private bool baronUsingRangedAttack = true;

    public bool IsPlayerDetected { get; private set; }
    public int FacingDirection { get; private set; } = -1;
    public Transform PlayerTarget => playerTarget;
    public EnemyArchetype Archetype => archetype;
    public bool IsRangedAttacker => archetype == EnemyArchetype.Pawn || archetype == EnemyArchetype.Archer;

    private void Awake()
    {
        enemyRigidbody = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemyStun = GetComponent<EnemyStun>();
        enemyStagger = GetComponent<EnemyStagger>();
        if (playerTarget == null)
        {
            GameObject player = GameObject.Find("Player");
            playerTarget = player != null ? player.transform : null;
        }

        if (visual == null)
        {
            Transform visualTransform = transform.Find("EnemyDebugVisual");
            visual = visualTransform != null ? visualTransform : transform;
        }

        initialVisualScale = visual.localScale;
        initialVisualWidth = Mathf.Abs(initialVisualScale.x);
        initialVisualHeight = Mathf.Abs(initialVisualScale.y);
        visualRenderers = visual.GetComponentsInChildren<Renderer>();
        visualMaterialBlock = new MaterialPropertyBlock();
        ApplyVisualProfile();
    }

    private void Start()
    {
        ApplyCombatProfile();
        if (gameObject.scene.name == "TestArena" && IsOnlyEnemyInGroup())
        {
            detectionRange = 4.5f;
            SpawnTestArenaArchetypes();
        }
    }

    // Hand-placed enemies anywhere in the scene (more than one controller) keep their own inspector values.
    private bool IsOnlyEnemyInGroup()
    {
        return archetype == EnemyArchetype.Knight && transform.parent != null &&
               FindObjectsByType<BasicEnemyController>(FindObjectsInactive.Include).Length == 1;
    }

    private void SpawnTestArenaArchetypes()
    {
        if (!IsOnlyEnemyInGroup())
        {
            return;
        }

        // Knight stays at its scene position (x=7); spacing keeps labels from overlapping.
        SpawnTestEnemy(EnemyArchetype.Slag, -14f);
        SpawnTestEnemy(EnemyArchetype.Pawn, -8.5f);
        SpawnTestEnemy(EnemyArchetype.Archer, 11f);
        SpawnTestEnemy(EnemyArchetype.Baron, 14.5f);
    }

    private void SpawnTestEnemy(EnemyArchetype testArchetype, float xPosition)
    {
        GameObject testEnemy = Instantiate(gameObject, transform.parent);
        testEnemy.name = $"Test_{testArchetype}";
        testEnemy.transform.localPosition = new Vector3(xPosition, 1.5f, 0f);

        BasicEnemyController controller = testEnemy.GetComponent<BasicEnemyController>();
        controller.ConfigureArchetype(testArchetype, 4.5f);
        testEnemy.GetComponent<EnemyStatusDebugDisplay>()?.RefreshArchetypeLabel();
    }

    public void ConfigureArchetype(EnemyArchetype newArchetype, float newDetectionRange = 12f)
    {
        archetype = newArchetype;
        detectionRange = Mathf.Max(0f, newDetectionRange);
        enemyHealth = GetComponent<EnemyHealth>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemyStun = GetComponent<EnemyStun>();
        enemyStagger = GetComponent<EnemyStagger>();
        ApplyVisualProfile();
        ApplyCombatProfile();
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.Find("Player");
            playerTarget = player != null ? player.transform : null;
        }

        if (playerTarget == null || (enemyHealth != null && enemyHealth.IsDead))
        {
            IsPlayerDetected = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTarget.position);
        IsPlayerDetected = distance <= detectionRange;
        if (!IsPlayerDetected)
        {
            return;
        }

        float horizontalOffset = playerTarget.position.x - transform.position.x;
        if (Mathf.Abs(horizontalOffset) > movementDeadZone)
        {
            FacingDirection = horizontalOffset > 0f ? 1 : -1;
            visual.localScale = new Vector3(initialVisualWidth * FacingDirection,
                visual.localScale.y, visual.localScale.z);
        }

        if (archetype == EnemyArchetype.Baron && enemyAttack != null && !enemyAttack.IsBusy)
        {
            bool useRangedAttack = distance > 3.4f;
            ConfigureAttackMode(useRangedAttack);
        }
    }

    private void FixedUpdate()
    {
        if (enemyRigidbody == null || playerTarget == null || !IsPlayerDetected ||
            (enemyHealth != null && enemyHealth.IsDead))
        {
            return;
        }

        if ((enemyStun != null && enemyStun.IsStunned) ||
            (enemyStagger != null && enemyStagger.IsStaggered) ||
            (enemyAttack != null && enemyAttack.IsWindupActive))
        {
            return;
        }

        float distance = Mathf.Abs(playerTarget.position.x - transform.position.x);
        float preferredDistance = GetPreferredDistance();
        float retreatDistance = GetRetreatDistance();
        float movementDirection = 0f;
        if (distance > preferredDistance + movementDeadZone)
        {
            movementDirection = FacingDirection;
        }
        else if (retreatDistance > 0f && distance < retreatDistance - movementDeadZone)
        {
            movementDirection = -FacingDirection;
        }

        if (movementDirection == 0f)
        {
            return;
        }

        Vector2 nextPosition = enemyRigidbody.position +
            Vector2.right * (movementDirection * GetMoveSpeed() * Time.fixedDeltaTime);
        enemyRigidbody.MovePosition(nextPosition);
    }

    public bool RegisterParryStagger()
    {
        parriesReceived++;
        int threshold = archetype == EnemyArchetype.Baron ? 3 : 1;
        if (parriesReceived < threshold || parriesReceived % threshold != 0)
        {
            return false;
        }

        if (enemyStagger != null)
        {
            enemyStagger.Stagger();
        }
        else if (enemyStun != null)
        {
            enemyStun.Stun();
        }

        return true;
    }

    private void ApplyCombatProfile()
    {
        float maxHealth;
        float attackRange;
        float damage;
        float cooldown;
        float windup;
        bool rangedAttack;
        bool parryable;
        float projectileSpeed = 0f;
        Color projectileColor = Color.white;

        switch (archetype)
        {
            case EnemyArchetype.Slag:
                maxHealth = 60f;
                attackRange = 1.45f;
                damage = 20f;
                cooldown = 2.8f;
                windup = 0.9f;
                rangedAttack = false;
                parryable = false;
                break;
            case EnemyArchetype.Pawn:
                maxHealth = 80f;
                attackRange = 7f;
                damage = 15f;
                cooldown = 2.5f;
                windup = 0.85f;
                rangedAttack = true;
                parryable = false;
                projectileSpeed = 5f;
                projectileColor = new Color(0.35f, 0.78f, 1f);
                break;
            case EnemyArchetype.Knight:
                maxHealth = 100f;
                attackRange = 1.7f;
                damage = 25f;
                cooldown = 1.8f;
                windup = 0.55f;
                rangedAttack = false;
                parryable = true;
                break;
            case EnemyArchetype.Archer:
                maxHealth = 90f;
                attackRange = 9f;
                damage = 20f;
                cooldown = 1.7f;
                windup = 0.65f;
                rangedAttack = true;
                parryable = true;
                projectileSpeed = 11f;
                projectileColor = new Color(0.25f, 1f, 0.85f);
                break;
            case EnemyArchetype.Baron:
                maxHealth = 240f;
                attackRange = 10f;
                damage = 32f;
                cooldown = 1.6f;
                windup = 0.45f;
                rangedAttack = true;
                parryable = true;
                projectileSpeed = 8f;
                projectileColor = new Color(1f, 0.24f, 0.18f);
                break;
            default:
                return;
        }

        enemyHealth?.ConfigureMaxHealth(maxHealth);
        enemyStagger?.ConfigureDamageHitsPerStagger(
            archetype == EnemyArchetype.Baron ? 3 : 1);
        if (enemyAttack != null)
        {
            enemyAttack.ConfigureAttack(attackRange, damage, cooldown, windup,
                rangedAttack, parryable, projectileSpeed, projectileColor);
        }
    }

    private void ConfigureAttackMode(bool useRangedAttack)
    {
        if (archetype == EnemyArchetype.Baron && useRangedAttack != baronUsingRangedAttack)
        {
            baronUsingRangedAttack = useRangedAttack;
            enemyAttack.ConfigureAttack(
                useRangedAttack ? 10f : 1.9f,
                1.6f,
                1.6f,
                0.45f,
                useRangedAttack,
                true,
                8f,
                new Color(1f, 0.24f, 0.18f));
        }
    }

    private float GetMoveSpeed()
    {
        return archetype switch
        {
            EnemyArchetype.Slag => slagMoveSpeed,
            EnemyArchetype.Pawn => pawnMoveSpeed,
            EnemyArchetype.Knight => knightMoveSpeed,
            EnemyArchetype.Archer => archerMoveSpeed,
            EnemyArchetype.Baron => baronMoveSpeed,
            _ => 0f
        };
    }

    private float GetPreferredDistance()
    {
        return archetype switch
        {
            EnemyArchetype.Slag => 1.2f,
            EnemyArchetype.Pawn => 5.4f,
            EnemyArchetype.Knight => 1.4f,
            EnemyArchetype.Archer => 6.3f,
            EnemyArchetype.Baron => 1.8f,
            _ => 1.2f
        };
    }

    private float GetRetreatDistance()
    {
        return archetype switch
        {
            EnemyArchetype.Pawn => 3.8f,
            EnemyArchetype.Archer => 5.2f,
            _ => 0f
        };
    }

    private void ApplyVisualProfile()
    {
        Color color;
        float heightScale;
        switch (archetype)
        {
            case EnemyArchetype.Slag:
                color = new Color(0.62f, 0.48f, 0.38f);
                heightScale = 0.88f;
                break;
            case EnemyArchetype.Pawn:
                color = new Color(0.3f, 0.66f, 0.92f);
                heightScale = 1f;
                break;
            case EnemyArchetype.Knight:
                color = new Color(0.9f, 0.57f, 0.24f);
                heightScale = 1.12f;
                break;
            case EnemyArchetype.Archer:
                color = new Color(0.25f, 0.84f, 0.72f);
                heightScale = 0.98f;
                break;
            case EnemyArchetype.Baron:
                color = new Color(0.85f, 0.2f, 0.24f);
                heightScale = 1.3f;
                break;
            default:
                color = Color.white;
                heightScale = 1f;
                break;
        }

        float facing = FacingDirection;
        visual.localScale = new Vector3(initialVisualWidth * facing,
            initialVisualHeight * heightScale, initialVisualScale.z);
        foreach (Renderer visualRenderer in visualRenderers)
        {
            visualRenderer.GetPropertyBlock(visualMaterialBlock);
            visualMaterialBlock.SetColor("_BaseColor", color);
            visualMaterialBlock.SetColor("_Color", color);
            visualRenderer.SetPropertyBlock(visualMaterialBlock);
        }
    }
}
