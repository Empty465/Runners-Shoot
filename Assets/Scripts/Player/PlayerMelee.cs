using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// E tap: execute a stunned low-health enemy, otherwise a normal melee hit.
// E hold with full melee points: heavy melee with an area stun.
public class PlayerMelee : MonoBehaviour
{
    [Header("Melee Points")]
    [SerializeField] private float maxPoints = 100f;
    [SerializeField] private float passiveChargePerSecond = 4f;
    [SerializeField] private float parryCharge = 30f;
    [SerializeField] private float dodgeCharge = 20f;

    [Header("Normal Melee")]
    [SerializeField] private float normalPointCost = 25f;
    [SerializeField] private float normalRange = 1.8f;
    [SerializeField] private float normalHeight = 1.6f;
    [SerializeField] private float normalDamage = 30f;
    [SerializeField] private float normalCooldown = 0.4f;
    [SerializeField] private int normalAmmoReward = 2;

    [Header("Heavy Melee")]
    [SerializeField] private float holdTime = 0.4f;
    [SerializeField] private float heavyRange = 2.6f;
    [SerializeField] private float heavyHeight = 2f;
    [SerializeField] private float heavyDamage = 80f;
    [SerializeField] private float heavyStunRadius = 5f;
    [SerializeField] private float heavyCooldown = 0.8f;

    private static Sprite slashSprite;
    private PlayerExecution playerExecution;
    private PlayerWeapon playerWeapon;
    private PlayerParry playerParry;
    private PlayerDodge playerDodge;
    private Camera aimCamera;
    private float points;
    private float nextAttackTime;
    private float holdStartedAt;
    private bool holding;
    private bool heavyFiredThisHold;
    private int facingDirection = 1;

    public event Action<float> PointsChanged;
    public float Points => points;
    public float MaxPoints => maxPoints;
    public bool IsFull => points >= maxPoints;
    public bool IsHolding => holding;
    public float HoldProgress => holding ? Mathf.Clamp01((Time.time - holdStartedAt) / Mathf.Max(0.01f, holdTime)) : 0f;

    private void Awake()
    {
        playerExecution = GetComponent<PlayerExecution>();
        playerWeapon = GetComponent<PlayerWeapon>();
        playerParry = GetComponent<PlayerParry>();
        playerDodge = GetComponent<PlayerDodge>();
    }

    private void OnEnable()
    {
        if (playerParry != null)
        {
            playerParry.ParrySucceeded += OnParrySucceeded;
        }

        if (playerDodge != null)
        {
            playerDodge.DodgeSucceeded += OnDodgeSucceeded;
        }
    }

    private void OnDisable()
    {
        if (playerParry != null)
        {
            playerParry.ParrySucceeded -= OnParrySucceeded;
        }

        if (playerDodge != null)
        {
            playerDodge.DodgeSucceeded -= OnDodgeSucceeded;
        }

        holding = false;
    }

    private void OnParrySucceeded()
    {
        AddPoints(parryCharge);
    }

    private void OnDodgeSucceeded()
    {
        AddPoints(dodgeCharge);
    }

    public void AddPoints(float amount)
    {
        float updated = Mathf.Clamp(points + amount, 0f, maxPoints);
        if (Mathf.Approximately(updated, points))
        {
            return;
        }

        points = updated;
        PointsChanged?.Invoke(points);
    }

    private void Update()
    {
        AddPoints(passiveChargePerSecond * Time.deltaTime);
        UpdateFacing();

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.eKey.wasPressedThisFrame)
        {
            holding = true;
            heavyFiredThisHold = false;
            holdStartedAt = Time.time;
        }

        if (!holding)
        {
            return;
        }

        if (!heavyFiredThisHold && IsFull && Time.time - holdStartedAt >= holdTime)
        {
            heavyFiredThisHold = true;
            TryHeavyAttack();
        }

        if (keyboard.eKey.wasReleasedThisFrame)
        {
            holding = false;
            if (!heavyFiredThisHold)
            {
                OnTap();
            }
        }
    }

    private void OnTap()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        if (playerExecution != null && playerExecution.TryExecute())
        {
            nextAttackTime = Time.time + normalCooldown;
            ShowSlash(normalRange, normalHeight, new Color(1f, 0.85f, 0.3f, 0.7f));
            return;
        }

        TryNormalAttack();
    }

    private void TryNormalAttack()
    {
        if (points < normalPointCost)
        {
            Debug.Log("[PlayerMelee] Not enough melee points");
            return;
        }

        nextAttackTime = Time.time + normalCooldown;
        ShowSlash(normalRange, normalHeight, new Color(0.5f, 0.9f, 1f, 0.7f));

        List<EnemyHealth> hits = FindTargets(normalRange, normalHeight);
        if (hits.Count == 0)
        {
            Debug.Log("[PlayerMelee] Normal miss");
            return;
        }

        points = Mathf.Max(0f, points - normalPointCost);
        PointsChanged?.Invoke(points);
        foreach (EnemyHealth target in hits)
        {
            target.TakeDamage(normalDamage);
        }

        playerWeapon?.RestoreAmmo(normalAmmoReward);
        Debug.Log($"[PlayerMelee] Normal hit x{hits.Count}");
    }

    private void TryHeavyAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + heavyCooldown;
        ShowSlash(heavyRange, heavyHeight, new Color(1f, 0.45f, 0.2f, 0.8f));

        List<EnemyHealth> hits = FindTargets(heavyRange, heavyHeight);
        if (hits.Count == 0)
        {
            Debug.Log("[PlayerMelee] Heavy miss");
            return;
        }

        points = 0f;
        PointsChanged?.Invoke(points);
        foreach (EnemyHealth target in hits)
        {
            target.TakeDamage(heavyDamage);
        }

        int stunned = 0;
        foreach (Collider2D nearby in Physics2D.OverlapCircleAll(transform.position, heavyStunRadius))
        {
            EnemyStun stun = nearby.GetComponentInParent<EnemyStun>();
            EnemyHealth health = stun != null ? stun.GetComponent<EnemyHealth>() : null;
            if (stun == null || health == null || health.IsDead || stun.IsStunned)
            {
                continue;
            }

            stun.Stun();
            stunned++;
        }

        Debug.Log($"[PlayerMelee] Heavy hit x{hits.Count}, area stun x{stunned}");
    }

    private List<EnemyHealth> FindTargets(float range, float height)
    {
        Vector2 center = (Vector2)transform.position + new Vector2(facingDirection * range * 0.5f, 0f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, new Vector2(range, height), 0f);
        List<EnemyHealth> results = new List<EnemyHealth>();
        foreach (Collider2D candidate in colliders)
        {
            EnemyHealth health = candidate.GetComponentInParent<EnemyHealth>();
            if (health != null && !health.IsDead && !results.Contains(health))
            {
                results.Add(health);
            }
        }

        return results;
    }

    private void UpdateFacing()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (aimCamera == null)
        {
            aimCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        }

        if (aimCamera == null)
        {
            return;
        }

        float offset = aimCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()).x - transform.position.x;
        if (Mathf.Abs(offset) > 0.05f)
        {
            facingDirection = offset > 0f ? 1 : -1;
        }
    }

    private void ShowSlash(float range, float height, Color color)
    {
        if (slashSprite == null)
        {
            slashSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        GameObject slash = new GameObject("MeleeSlashVFX");
        slash.transform.position = transform.position + new Vector3(facingDirection * range * 0.5f, 0f, -0.2f);
        slash.transform.localScale = new Vector3(range, height, 1f);
        SpriteRenderer slashRenderer = slash.AddComponent<SpriteRenderer>();
        slashRenderer.sprite = slashSprite;
        slashRenderer.color = color;
        slashRenderer.sortingOrder = 25;
        Destroy(slash, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + new Vector3(facingDirection * normalRange * 0.5f, 0f, 0f),
            new Vector3(normalRange, normalHeight, 0f));
        Gizmos.color = new Color(1f, 0.5f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, heavyStunRadius);
    }
}
