using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerDodge : MonoBehaviour
{
    [Header("Dodge")]
    [SerializeField] private float dodgeSpeed = 28f;
    [SerializeField] private float dodgeDuration = 0.18f;
    [SerializeField] private float dodgeCooldown = 0.7f;
    [SerializeField] private float defaultDirection = 1f;
    [SerializeField] private int enhancementPointsPerDodge = 1;

    private Rigidbody2D playerRigidbody;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction dodgeAction;
    private Camera playerCamera;
    private PlayerEnhancementPoints enhancementPoints;
    private PlayerCombatFeedbackVFX combatFeedbackVFX;
    private float nextDodgeTime;
    private float dodgeUntil;
    private int dodgeDirection;
    private bool successRegistered;

    public bool IsDodging => Time.time < dodgeUntil;
    public bool CanDodge => !IsDodging && Time.time >= nextDodgeTime;
    public float DodgeCooldownRemaining => Mathf.Max(0f, nextDodgeTime - Time.time);
    public event Action DodgeSucceeded;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
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

        InputActionMap playerActions = playerInput.actions.FindActionMap("Player", true);
        moveAction = playerActions.FindAction("Move", true);
        dodgeAction = playerActions.FindAction("Sprint", true);
        playerCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        dodgeAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        dodgeAction?.Disable();
        dodgeUntil = 0f;
        successRegistered = false;
    }

    private void Update()
    {
        if (dodgeAction != null && dodgeAction.WasPressedThisFrame())
        {
            TryDodge();
        }
    }

    private void FixedUpdate()
    {
        if (!IsDodging)
        {
            return;
        }

        playerRigidbody.linearVelocity = new Vector2(dodgeDirection * dodgeSpeed, 0f);
    }

    private void TryDodge()
    {
        if (!CanDodge)
        {
            return;
        }

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            dodgeDirection = moveInput.x > 0f ? 1 : -1;
        }
        else
        {
            dodgeDirection = GetCursorDirection();
        }

        nextDodgeTime = Time.time + dodgeCooldown;
        dodgeUntil = Time.time + dodgeDuration;
        successRegistered = false;
        playerRigidbody.linearVelocity = new Vector2(dodgeDirection * dodgeSpeed, 0f);
        Debug.Log("[PlayerDodge] Started direction=" + dodgeDirection);
    }

    public bool TryRegisterSuccess()
    {
        if (!IsDodging || successRegistered)
        {
            return false;
        }

        successRegistered = true;
        if (enhancementPoints != null)
        {
            enhancementPoints.AddPoints(enhancementPointsPerDodge);
        }

        DodgeSucceeded?.Invoke();
        combatFeedbackVFX.PlayDodgeSuccess(dodgeDirection);
        return true;
    }

    private int GetCursorDirection()
    {
        if (Mouse.current != null)
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            }

            if (playerCamera != null)
            {
                Vector3 cursorWorldPosition = playerCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                float horizontalOffset = cursorWorldPosition.x - transform.position.x;
                if (Mathf.Abs(horizontalOffset) > 0.01f)
                {
                    return horizontalOffset > 0f ? 1 : -1;
                }
            }
        }

        return defaultDirection >= 0f ? 1 : -1;
    }
}
