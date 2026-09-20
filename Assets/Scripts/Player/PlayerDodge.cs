using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerDodge : MonoBehaviour
{
    [Header("Dodge")]
    [SerializeField] private float dodgeSpeed = 14f;
    [SerializeField] private float dodgeDuration = 0.16f;
    [SerializeField] private float dodgeCooldown = 0.75f;
    [SerializeField] private float defaultDirection = 1f;

    private Rigidbody2D playerRigidbody;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction dodgeAction;
    private float nextDodgeTime;
    private float dodgeUntil;
    private int dodgeDirection;

    public bool IsDodging => Time.time < dodgeUntil;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        InputActionMap playerActions = playerInput.actions.FindActionMap("Player", true);
        moveAction = playerActions.FindAction("Move", true);
        dodgeAction = playerActions.FindAction("Sprint", true);
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
        if (Time.time < nextDodgeTime || IsDodging)
        {
            return;
        }

        float horizontalInput = moveAction != null ? moveAction.ReadValue<Vector2>().x : 0f;
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            dodgeDirection = horizontalInput > 0f ? 1 : -1;
        }
        else if (dodgeDirection == 0)
        {
            dodgeDirection = defaultDirection >= 0f ? 1 : -1;
        }

        nextDodgeTime = Time.time + dodgeCooldown;
        dodgeUntil = Time.time + dodgeDuration;
        playerRigidbody.linearVelocity = new Vector2(dodgeDirection * dodgeSpeed, 0f);
        Debug.Log("[PlayerDodge] Started direction=" + dodgeDirection);
    }
}
