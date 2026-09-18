using UnityEngine;

public class PlayerHealthDebugDisplay : MonoBehaviour
{
    [SerializeField] private Vector2 screenOffset = new(20f, 20f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;

    private PlayerHealth playerHealth;
    private GUIStyle labelStyle;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }
    }

    private void OnGUI()
    {
        if (playerHealth == null)
        {
            return;
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
            new Rect(screenOffset.x, screenOffset.y, 220f, 70f),
            $"HP: {playerHealth.CurrentHealth:0}/{playerHealth.MaxHealth:0}\n" +
            $"ARMOR: {playerHealth.CurrentArmor:0}/{playerHealth.MaxArmor:0}",
            labelStyle);
    }
}
