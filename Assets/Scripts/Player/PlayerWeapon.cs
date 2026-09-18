using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerWeapon : MonoBehaviour
{
    [Header("Basic Weapon")]
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileDamage = 1f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private float fireCooldown = 0.2f;
    [SerializeField] private float muzzleOffset = 0.6f;
    [SerializeField] private float projectileSize = 0.16f;
    [SerializeField] private Color projectileColor = Color.yellow;

    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int startingAmmo = 30;

    private PlayerInput playerInput;
    private InputAction attackAction;
    private Camera playerCamera;
    private float nextFireTime;

    public event Action<int> AmmoChanged;
    public int MaxAmmo => maxAmmo;
    public int CurrentAmmo { get; private set; }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        InputActionMap playerActions = playerInput.actions.FindActionMap("Player", true);
        attackAction = playerActions.FindAction("Attack", true);
        playerCamera = FindPlayerCamera();
        maxAmmo = Mathf.Max(0, maxAmmo);
        CurrentAmmo = Mathf.Clamp(startingAmmo, 0, maxAmmo);
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
        if (attackAction != null && attackAction.WasPressedThisFrame() && Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (CurrentAmmo <= 0)
        {
            Debug.Log("[PlayerWeapon] Out of Ammo");
            return;
        }

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
        Vector2 direction = (mouseWorldPosition - transform.position).normalized;
        if (direction == Vector2.zero)
        {
            return;
        }

        nextFireTime = Time.time + fireCooldown;
        CurrentAmmo--;
        AmmoChanged?.Invoke(CurrentAmmo);
        GameObject projectileObject = CreateProjectileObject();
        projectileObject.transform.position = transform.position + (Vector3)(direction * muzzleOffset);
        projectileObject.transform.right = direction;

        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.Initialize(direction, projectileSpeed, projectileDamage, projectileLifetime, transform);
        Debug.Log("[PlayerWeapon] Fired");
    }

    public void RestoreAmmo(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentAmmo = Mathf.Min(maxAmmo, CurrentAmmo + amount);
        AmmoChanged?.Invoke(CurrentAmmo);
    }

    private GameObject CreateProjectileObject()
    {
        GameObject projectileObject = new GameObject("PlayerProjectile");
        projectileObject.layer = gameObject.layer;

        SpriteRenderer spriteRenderer = projectileObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);
        spriteRenderer.color = projectileColor;
        projectileObject.transform.localScale = Vector3.one * projectileSize;

        Rigidbody2D projectileRigidbody = projectileObject.AddComponent<Rigidbody2D>();
        projectileRigidbody.bodyType = RigidbodyType2D.Kinematic;
        projectileRigidbody.simulated = true;

        CircleCollider2D projectileCollider = projectileObject.AddComponent<CircleCollider2D>();
        projectileCollider.isTrigger = true;

        return projectileObject;
    }

    private Camera FindPlayerCamera()
    {
        return Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }
}
