using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDeathHandler : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Rigidbody2D playerRigidbody;
    private bool deathHandled;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        playerHealth.Died += HandleDeath;
        if (playerHealth.IsDead)
        {
            HandleDeath();
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandleDeath;
        }
    }

    private void Update()
    {
        if (deathHandled && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RetryCurrentScene();
        }
    }

    private void RetryCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex < 0)
        {
            Debug.LogError($"[PlayerDeathHandler] Cannot retry scene '{activeScene.name}' because it is not in Build Settings");
            return;
        }

        Debug.Log($"[PlayerDeathHandler] Reloading scene '{activeScene.name}'");
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void HandleDeath()
    {
        if (deathHandled)
        {
            return;
        }

        deathHandled = true;
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour != this && behaviour != playerHealth)
            {
                behaviour.enabled = false;
            }
        }

        Debug.Log("[PlayerDeathHandler] Player controls and combat disabled");
    }
}
