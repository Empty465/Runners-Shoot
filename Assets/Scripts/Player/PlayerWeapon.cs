using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerWeaponType
{
    SMG,
    Shotgun,
    Railgun
}

[RequireComponent(typeof(PlayerInput))]
public class PlayerWeapon : MonoBehaviour
{
    [Header("SMG")]
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileDamage = 20f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private float fireCooldown = 0.2f;
    [SerializeField] private float muzzleOffset = 0.6f;
    [SerializeField] private float projectileSize = 0.16f;
    [SerializeField] private Color projectileColor = Color.yellow;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int startingAmmo = 30;

    [Header("Shotgun")]
    [SerializeField] private float shotgunProjectileSpeed = 16f;
    [SerializeField] private float shotgunProjectileLifetime = 3f;
    [SerializeField] private float shotgunProjectileDamage = 11f;
    [SerializeField] private float shotgunFireCooldown = 0.8f;
    [SerializeField] private int shotgunPelletCount = 7;
    [SerializeField] private float shotgunSpreadDegrees = 26f;
    [SerializeField] private int shotgunMaxAmmo = 8;
    [SerializeField] private int shotgunStartingAmmo = 8;

    [Header("Railgun")]
    [SerializeField] private float railgunDamage = 80f;
    [SerializeField] private float railgunFireCooldown = 1.4f;
    [SerializeField] private float railgunRange = 20f;
    [SerializeField] private float railgunExplosionRadius = 1.5f;
    [SerializeField] private int railgunMaxAmmo = 4;
    [SerializeField] private int railgunStartingAmmo = 4;

    [Header("Upgrade")]
    [SerializeField] private int upgradeCost = 1;
    [SerializeField] private float damageUpgradeAmount = 10f;
    [SerializeField] private float speedUpgradeAmount = 1.5f;
    [SerializeField] private float upgradeDuration = 10f;
    [SerializeField] private float burnDamagePerTick = 5f;
    [SerializeField] private float burnTickInterval = 0.5f;
    [SerializeField] private int burnTickCount = 4;
    [SerializeField] private float armorOnBurnHit = 2f;
    [SerializeField] private float shotgunFollowupDelay = 0.12f;
    [SerializeField] private float railgunUpgradeDamageAmount = 10f;
    [SerializeField] private float railgunUpgradeRadiusAmount = 0.25f;

    private PlayerInput playerInput;
    private InputAction attackAction;
    private Camera playerCamera;
    private PlayerEnhancementPoints enhancementPoints;
    private PlayerHealth playerHealth;
    private float smgNextFireTime;
    private float shotgunNextFireTime;
    private float railgunNextFireTime;
    private int smgAmmo;
    private int shotgunAmmo;
    private int railgunAmmo;
    private int smgUpgradeLevel;
    private int shotgunUpgradeLevel;
    private int railgunUpgradeLevel;
    private float smgUpgradeExpiresAt;
    private float shotgunUpgradeExpiresAt;
    private float railgunUpgradeExpiresAt;
    private readonly HashSet<PlayerWeaponType> emptyAmmoWarnings = new();
    private bool pendingShotgunFollowup;
    private float shotgunFollowupTime;
    private Vector2 shotgunFollowupDirection;

    public event Action<int> AmmoChanged;
    public event Action<PlayerWeaponType> WeaponChanged;
    public event Action<PlayerWeaponType, int> WeaponUpgradeChanged;
    public PlayerWeaponType CurrentWeapon { get; private set; }
    public int CurrentAmmo => GetAmmo(CurrentWeapon);
    public int MaxAmmo => GetMaxAmmo(CurrentWeapon);
    public int CurrentUpgradeLevel => GetUpgradeLevel(CurrentWeapon);
    public int UpgradeCost => upgradeCost;
    public float UpgradeDuration => upgradeDuration;
    public float CurrentUpgradeRemaining => Mathf.Max(0f, GetUpgradeExpiresAt(CurrentWeapon) - Time.time);
    public string CurrentWeaponName => CurrentWeapon.ToString().ToUpperInvariant();

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        enhancementPoints = GetComponent<PlayerEnhancementPoints>();
        if (enhancementPoints == null)
        {
            enhancementPoints = gameObject.AddComponent<PlayerEnhancementPoints>();
        }

        playerHealth = GetComponent<PlayerHealth>();
        InputActionMap playerActions = playerInput.actions.FindActionMap("Player", true);
        attackAction = playerActions.FindAction("Attack", true);
        playerCamera = FindPlayerCamera();
        maxAmmo = Mathf.Max(0, maxAmmo);
        shotgunMaxAmmo = Mathf.Max(0, shotgunMaxAmmo);
        railgunMaxAmmo = Mathf.Max(0, railgunMaxAmmo);
        startingAmmo = Mathf.Clamp(startingAmmo, 0, maxAmmo);
        shotgunStartingAmmo = Mathf.Clamp(shotgunStartingAmmo, 0, shotgunMaxAmmo);
        railgunStartingAmmo = Mathf.Clamp(railgunStartingAmmo, 0, railgunMaxAmmo);
        smgAmmo = startingAmmo;
        shotgunAmmo = shotgunStartingAmmo;
        railgunAmmo = railgunStartingAmmo;
        shotgunPelletCount = Mathf.Max(1, shotgunPelletCount);
        upgradeDuration = Mathf.Max(0.1f, upgradeDuration);
    }

    private void OnEnable()
    {
        attackAction?.Enable();
    }

    private void OnDisable()
    {
        attackAction?.Disable();
    }

    private void Update()
    {
        ExpireWeaponUpgrades();

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                TryEquipWeapon(PlayerWeaponType.SMG);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                TryEquipWeapon(PlayerWeaponType.Shotgun);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                TryEquipWeapon(PlayerWeaponType.Railgun);
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                TryUpgradeWeapon();
            }
        }

        if (pendingShotgunFollowup && Time.time >= shotgunFollowupTime)
        {
            pendingShotgunFollowup = false;
            FireShotgunVolley(shotgunFollowupDirection);
            shotgunNextFireTime = Time.time + shotgunFireCooldown;
            Debug.Log("[PlayerWeapon] Shotgun double-barrel follow-up");
        }

        bool attackPressed = attackAction != null &&
            (CurrentWeapon == PlayerWeaponType.SMG
                ? attackAction.IsPressed()
                : attackAction.WasPressedThisFrame());
        if (attackPressed && Time.time >= GetNextFireTime(CurrentWeapon))
        {
            FireAtCursor();
        }
    }

    public bool TryEquipWeapon(PlayerWeaponType weaponType)
    {
        if (!Enum.IsDefined(typeof(PlayerWeaponType), weaponType) || CurrentWeapon == weaponType)
        {
            return false;
        }

        CurrentWeapon = weaponType;
        AmmoChanged?.Invoke(CurrentAmmo);
        WeaponChanged?.Invoke(CurrentWeapon);
        Debug.Log($"[PlayerWeapon] Equipped {CurrentWeaponName}");
        return true;
    }

    public bool TryFire(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon || Time.time < GetNextFireTime(CurrentWeapon))
        {
            return false;
        }

        direction.Normalize();
        bool shotgunDoubleShot = CurrentWeapon == PlayerWeaponType.Shotgun &&
            shotgunUpgradeLevel > 0 && CurrentAmmo >= 2;
        int ammoCost = shotgunDoubleShot ? 2 : 1;
        if (CurrentAmmo < ammoCost)
        {
            if (emptyAmmoWarnings.Add(CurrentWeapon))
            {
                Debug.Log($"[PlayerWeapon] {CurrentWeaponName} out of ammo");
            }

            return false;
        }

        emptyAmmoWarnings.Remove(CurrentWeapon);
        SetAmmo(CurrentWeapon, CurrentAmmo - ammoCost);
        AmmoChanged?.Invoke(CurrentAmmo);

        switch (CurrentWeapon)
        {
            case PlayerWeaponType.SMG:
                FireSmg(direction);
                smgNextFireTime = Time.time + GetSmgCooldown();
                break;
            case PlayerWeaponType.Shotgun:
                FireShotgunVolley(direction);
                if (shotgunDoubleShot)
                {
                    pendingShotgunFollowup = true;
                    shotgunFollowupTime = Time.time + shotgunFollowupDelay;
                    shotgunFollowupDirection = direction;
                    shotgunNextFireTime = shotgunFollowupTime + shotgunFireCooldown;
                }
                else
                {
                    shotgunNextFireTime = Time.time + shotgunFireCooldown;
                }
                break;
            case PlayerWeaponType.Railgun:
                FireRailgun(direction);
                railgunNextFireTime = Time.time + railgunFireCooldown;
                break;
            default:
                return false;
        }

        Debug.Log($"[PlayerWeapon] Fired {CurrentWeaponName}");
        return true;
    }

    private void FireAtCursor()
    {
        if (playerCamera == null)
        {
            playerCamera = FindPlayerCamera();
        }

        if (playerCamera == null || Mouse.current == null)
        {
            return;
        }

        Vector3 mouseWorldPosition = playerCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPosition.z = transform.position.z;
        TryFire(mouseWorldPosition - transform.position);
    }

    public bool TryUpgradeWeapon()
    {
        if (enhancementPoints == null || !enhancementPoints.TrySpendPoints(upgradeCost))
        {
            return false;
        }

        SetUpgradeLevel(CurrentWeapon, 1);
        SetUpgradeExpiresAt(CurrentWeapon, Time.time + upgradeDuration);
        WeaponUpgradeChanged?.Invoke(CurrentWeapon, CurrentUpgradeLevel);
        Debug.Log($"[PlayerWeapon] {CurrentWeaponName} enhanced for {upgradeDuration:0.#} seconds");
        return true;
    }

    private void ExpireWeaponUpgrades()
    {
        ExpireWeaponUpgrade(PlayerWeaponType.SMG, smgUpgradeLevel, smgUpgradeExpiresAt);
        ExpireWeaponUpgrade(PlayerWeaponType.Shotgun, shotgunUpgradeLevel, shotgunUpgradeExpiresAt);
        ExpireWeaponUpgrade(PlayerWeaponType.Railgun, railgunUpgradeLevel, railgunUpgradeExpiresAt);
    }

    private void ExpireWeaponUpgrade(PlayerWeaponType weaponType, int level, float expiresAt)
    {
        if (level == 0 || Time.time < expiresAt)
        {
            return;
        }

        SetUpgradeLevel(weaponType, 0);
        SetUpgradeExpiresAt(weaponType, 0f);
        WeaponUpgradeChanged?.Invoke(weaponType, 0);
        Debug.Log($"[PlayerWeapon] {weaponType.ToString().ToUpperInvariant()} enhancement expired");
    }

    public void RestoreAmmo(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int previousAmmo = CurrentAmmo;
        SetAmmo(CurrentWeapon, Mathf.Min(MaxAmmo, previousAmmo + amount));
        if (CurrentAmmo != previousAmmo)
        {
            if (CurrentAmmo > 0)
            {
                emptyAmmoWarnings.Remove(CurrentWeapon);
            }

            AmmoChanged?.Invoke(CurrentAmmo);
        }
    }

    private void FireSmg(Vector2 direction)
    {
        float speed = projectileSpeed + speedUpgradeAmount * smgUpgradeLevel;
        float damage = projectileDamage + damageUpgradeAmount * smgUpgradeLevel;
        SpawnProjectile(direction, speed, damage, projectileLifetime, projectileSize, projectileColor,
            smgUpgradeLevel > 0);
    }

    private void FireShotgunVolley(Vector2 direction)
    {
        float damage = shotgunProjectileDamage + damageUpgradeAmount * 0.2f * shotgunUpgradeLevel;
        float halfSpread = shotgunSpreadDegrees * 0.5f;
        for (int i = 0; i < shotgunPelletCount; i++)
        {
            float spread = shotgunPelletCount == 1
                ? 0f
                : Mathf.Lerp(-halfSpread, halfSpread, i / (float)(shotgunPelletCount - 1));
            Vector2 pelletDirection = Quaternion.Euler(0f, 0f, spread) * direction;
            SpawnProjectile(pelletDirection, shotgunProjectileSpeed, damage, shotgunProjectileLifetime,
                projectileSize * 0.8f, new Color(1f, 0.68f, 0.24f), false);
        }
    }

    private void FireRailgun(Vector2 direction)
    {
        float damage = railgunDamage + railgunUpgradeDamageAmount * railgunUpgradeLevel;
        float radius = railgunUpgradeLevel > 0
            ? railgunExplosionRadius + railgunUpgradeRadiusAmount * (railgunUpgradeLevel - 1)
            : 0f;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, railgunRange);
        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        Vector2 endPoint = (Vector2)transform.position + direction * railgunRange;
        bool hitSomething = false;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || IsOwnedCollider(hit.collider))
            {
                continue;
            }

            endPoint = hit.point;
            hitSomething = true;
            break;
        }

        CreateRailBeam(transform.position, endPoint);
        if (railgunUpgradeLevel > 0 && hitSomething)
        {
            ApplyRailgunExplosion(endPoint, damage, radius);
            CreateExplosionVisual(endPoint, radius);
        }
        else if (hitSomething)
        {
            EnemyHealth enemyHealth = FindEnemyHealthAt(endPoint, 0.15f);
            enemyHealth?.TakeDamage(damage);
        }
    }

    private void ApplyRailgunExplosion(Vector2 point, float damage, float radius)
    {
        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(point, radius))
        {
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null && damagedEnemies.Add(enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    private static EnemyHealth FindEnemyHealthAt(Vector2 point, float radius)
    {
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(point, radius))
        {
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                return enemyHealth;
            }
        }

        return null;
    }

    private bool IsOwnedCollider(Collider2D collider)
    {
        return collider.transform == transform || collider.transform.IsChildOf(transform);
    }

    private void SpawnProjectile(
        Vector2 direction,
        float speed,
        float damage,
        float lifetime,
        float size,
        Color color,
        bool overheat)
    {
        GameObject projectileObject = CreateProjectileObject(size, color);
        projectileObject.transform.position = transform.position + (Vector3)(direction * muzzleOffset);
        projectileObject.transform.right = direction;

        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.Initialize(direction, speed, damage, lifetime, transform, overheat,
            burnDamagePerTick, burnTickCount, burnTickInterval, armorOnBurnHit, playerHealth);
    }

    private GameObject CreateProjectileObject(float size, Color color)
    {
        GameObject projectileObject = new GameObject("PlayerProjectile");
        projectileObject.layer = gameObject.layer;

        SpriteRenderer spriteRenderer = projectileObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);
        spriteRenderer.color = color;
        projectileObject.transform.localScale = Vector3.one * size;

        Rigidbody2D projectileRigidbody = projectileObject.AddComponent<Rigidbody2D>();
        projectileRigidbody.bodyType = RigidbodyType2D.Kinematic;
        projectileRigidbody.simulated = true;

        CircleCollider2D projectileCollider = projectileObject.AddComponent<CircleCollider2D>();
        projectileCollider.isTrigger = true;

        return projectileObject;
    }

    private float GetSmgCooldown()
    {
        return Mathf.Max(0.04f, fireCooldown * (1f - 0.12f * smgUpgradeLevel));
    }

    private float GetNextFireTime(PlayerWeaponType weaponType)
    {
        return weaponType switch
        {
            PlayerWeaponType.SMG => smgNextFireTime,
            PlayerWeaponType.Shotgun => shotgunNextFireTime,
            PlayerWeaponType.Railgun => railgunNextFireTime,
            _ => 0f
        };
    }

    private int GetAmmo(PlayerWeaponType weaponType)
    {
        return weaponType switch
        {
            PlayerWeaponType.SMG => smgAmmo,
            PlayerWeaponType.Shotgun => shotgunAmmo,
            PlayerWeaponType.Railgun => railgunAmmo,
            _ => 0
        };
    }

    private int GetMaxAmmo(PlayerWeaponType weaponType)
    {
        return weaponType switch
        {
            PlayerWeaponType.SMG => maxAmmo,
            PlayerWeaponType.Shotgun => shotgunMaxAmmo,
            PlayerWeaponType.Railgun => railgunMaxAmmo,
            _ => 0
        };
    }

    private int GetUpgradeLevel(PlayerWeaponType weaponType)
    {
        return weaponType switch
        {
            PlayerWeaponType.SMG => smgUpgradeLevel,
            PlayerWeaponType.Shotgun => shotgunUpgradeLevel,
            PlayerWeaponType.Railgun => railgunUpgradeLevel,
            _ => 0
        };
    }

    private float GetUpgradeExpiresAt(PlayerWeaponType weaponType)
    {
        return weaponType switch
        {
            PlayerWeaponType.SMG => smgUpgradeExpiresAt,
            PlayerWeaponType.Shotgun => shotgunUpgradeExpiresAt,
            PlayerWeaponType.Railgun => railgunUpgradeExpiresAt,
            _ => 0f
        };
    }

    private void SetUpgradeExpiresAt(PlayerWeaponType weaponType, float expiresAt)
    {
        switch (weaponType)
        {
            case PlayerWeaponType.SMG:
                smgUpgradeExpiresAt = expiresAt;
                break;
            case PlayerWeaponType.Shotgun:
                shotgunUpgradeExpiresAt = expiresAt;
                break;
            case PlayerWeaponType.Railgun:
                railgunUpgradeExpiresAt = expiresAt;
                break;
        }
    }

    private void SetAmmo(PlayerWeaponType weaponType, int amount)
    {
        switch (weaponType)
        {
            case PlayerWeaponType.SMG:
                smgAmmo = amount;
                break;
            case PlayerWeaponType.Shotgun:
                shotgunAmmo = amount;
                break;
            case PlayerWeaponType.Railgun:
                railgunAmmo = amount;
                break;
        }
    }

    private void SetUpgradeLevel(PlayerWeaponType weaponType, int level)
    {
        switch (weaponType)
        {
            case PlayerWeaponType.SMG:
                smgUpgradeLevel = level;
                break;
            case PlayerWeaponType.Shotgun:
                shotgunUpgradeLevel = level;
                break;
            case PlayerWeaponType.Railgun:
                railgunUpgradeLevel = level;
                break;
        }
    }

    private void CreateRailBeam(Vector2 startPoint, Vector2 endPoint)
    {
        GameObject beamObject = new GameObject("RailgunBeam");
        LineRenderer line = beamObject.AddComponent<LineRenderer>();
        if (railBeamMaterial == null)
        {
            railBeamMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        line.positionCount = 2;
        line.SetPosition(0, startPoint);
        line.SetPosition(1, endPoint);
        line.startWidth = 0.09f;
        line.endWidth = 0.02f;
        line.useWorldSpace = true;
        line.sharedMaterial = railBeamMaterial;
        line.startColor = new Color(0.35f, 0.9f, 1f);
        line.endColor = new Color(0.7f, 0.96f, 1f, 0.15f);
        line.sortingOrder = 20;
        Destroy(beamObject, 0.12f);
    }

    private static Material railBeamMaterial;
    private static Sprite explosionSprite;

    private static void CreateExplosionVisual(Vector2 point, float radius)
    {
        GameObject explosion = new GameObject("RailgunExplosion");
        SpriteRenderer spriteRenderer = explosion.AddComponent<SpriteRenderer>();
        if (explosionSprite == null)
        {
            const int textureSize = 32;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[textureSize * textureSize];
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f)) /
                        (textureSize * 0.5f);
                    float alpha = Mathf.Clamp01((1f - distance) * 8f);
                    pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            explosionSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f), textureSize);
        }

        spriteRenderer.sprite = explosionSprite;
        spriteRenderer.color = new Color(0.25f, 0.8f, 1f, 0.65f);
        spriteRenderer.sortingOrder = 19;
        explosion.transform.position = point;
        explosion.transform.localScale = Vector3.one * radius * 2f;
        Destroy(explosion, 0.2f);
    }

    private Camera FindPlayerCamera()
    {
        return Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }
}
