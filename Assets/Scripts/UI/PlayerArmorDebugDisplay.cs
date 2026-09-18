using UnityEngine;

public class PlayerArmorDebugDisplay : MonoBehaviour
{
    [SerializeField] private Vector2 screenOffset = new(20f, 48f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;

    private PlayerHealth playerHealth;
    private GUIStyle labelStyle;

    private void Awake()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
    }

    private void OnGUI()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.Find("Player");
            playerHealth = player != null ? player.GetComponent<PlayerHealth>() : FindAnyObjectByType<PlayerHealth>();
            if (playerHealth == null)
            {
                return;
            }
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                alignment = TextAnchor.UpperLeft
            };
            labelStyle.normal.textColor = textColor;
        }

        GUI.Label(
            new Rect(screenOffset.x, screenOffset.y, 260f, 40f),
            $"ARMOR: {playerHealth.CurrentArmor:0}/{playerHealth.MaxArmor:0}",
            labelStyle);
    }
}
