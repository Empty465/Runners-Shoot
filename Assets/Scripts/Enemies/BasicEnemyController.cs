using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform visual;

    private Vector3 initialVisualScale;

    public bool IsPlayerDetected { get; private set; }
    public int FacingDirection { get; private set; } = -1;
    public Transform PlayerTarget => playerTarget;

    private void Awake()
    {
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
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.Find("Player");
            playerTarget = player != null ? player.transform : null;
        }

        if (playerTarget == null)
        {
            IsPlayerDetected = false;
            return;
        }

        IsPlayerDetected = Vector2.Distance(transform.position, playerTarget.position) <= detectionRange;
        if (!IsPlayerDetected)
        {
            return;
        }

        float horizontalDirection = Mathf.Sign(playerTarget.position.x - transform.position.x);
        if (horizontalDirection != 0f)
        {
            FacingDirection = horizontalDirection > 0f ? 1 : -1;
            visual.localScale = new Vector3(Mathf.Abs(initialVisualScale.x) * FacingDirection, initialVisualScale.y, initialVisualScale.z);
        }
    }
}