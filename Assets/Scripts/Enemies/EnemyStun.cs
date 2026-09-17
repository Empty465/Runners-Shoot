using System.Collections;
using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    [SerializeField] private float stunDuration = 1.5f;
    [SerializeField] private GameObject stunDebugVisual;

    private Coroutine stunRoutine;

    public bool IsStunned { get; private set; }

    private void Awake()
    {
        if (stunDebugVisual == null)
        {
            Transform visualTransform = transform.Find("StunDebugVisual");
            stunDebugVisual = visualTransform != null ? visualTransform.gameObject : null;
        }

        SetStunVisual(false);
    }

    public void Stun()
    {
        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
        }

        stunRoutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        IsStunned = true;
        SetStunVisual(true);
        Debug.Log("[Enemy] Stunned");

        yield return new WaitForSeconds(stunDuration);

        IsStunned = false;
        SetStunVisual(false);
        stunRoutine = null;
        Debug.Log("[Enemy] Stun Ended");
    }

    private void SetStunVisual(bool active)
    {
        if (stunDebugVisual != null)
        {
            stunDebugVisual.SetActive(active);
        }
    }

    private void OnDisable()
    {
        if (stunRoutine != null)
        {
            StopCoroutine(stunRoutine);
            stunRoutine = null;
        }

        IsStunned = false;
        SetStunVisual(false);
    }
}