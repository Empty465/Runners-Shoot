using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// TestArena only: F2 restores player HP/Armor, F3 reloads the scene to reset all enemies.
public class TestArenaDebugKeys : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (SceneManager.GetActiveScene().name == "TestArena")
        {
            GameObject host = new GameObject("TestArenaDebugKeys");
            host.AddComponent<TestArenaDebugKeys>();
            DontDestroyOnLoad(host);
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || SceneManager.GetActiveScene().name != "TestArena")
        {
            return;
        }

        if (keyboard.f2Key.wasPressedThisFrame)
        {
            GameObject player = GameObject.Find("Player");
            PlayerHealth health = player != null ? player.GetComponent<PlayerHealth>() : null;
            if (health != null && !health.IsDead)
            {
                health.RestoreHealth(health.MaxHealth);
                health.RestoreArmor(health.MaxArmor);
            }
        }

        if (keyboard.f3Key.wasPressedThisFrame)
        {
            ReloadScene();
        }
    }

    private static void ReloadScene()
    {
        Scene scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        if (scene.buildIndex >= 0)
        {
            SceneManager.LoadScene(scene.buildIndex);
        }
#endif
    }
}
