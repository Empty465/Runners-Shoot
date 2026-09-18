using UnityEngine;

public class EnemyStatusDebugDisplay : MonoBehaviour
{
    [SerializeField] private Vector3 localOffset = new(0f, 2.2f, -0.3f);
    [SerializeField] private int fontSize = 32;
    [SerializeField] private float characterSize = 0.1f;
    [SerializeField] private Color staggerColor = Color.yellow;
    [SerializeField] private Color stunColor = Color.red;

    private EnemyStun enemyStun;
    private EnemyStagger enemyStagger;
    private TextMesh statusText;
    private MeshRenderer statusRenderer;

    private void Awake()
    {
        enemyStun = GetComponent<EnemyStun>();
        enemyStagger = GetComponent<EnemyStagger>();
        CreateStatusText();
    }

    private void Update()
    {
        if (statusText == null || statusRenderer == null)
        {
            return;
        }

        if (enemyStun != null && enemyStun.IsStunned)
        {
            SetStatus("STUN", stunColor);
        }
        else if (enemyStagger != null && enemyStagger.IsStaggered)
        {
            SetStatus("STAGGER", staggerColor);
        }
        else
        {
            SetStatus(string.Empty, Color.clear);
        }
    }

    private void SetStatus(string message, Color color)
    {
        statusText.text = message;
        statusText.color = color;
        statusRenderer.enabled = !string.IsNullOrEmpty(message);
    }

    private void CreateStatusText()
    {
        GameObject statusObject = new GameObject("EnemyStatusDebugText");
        statusObject.transform.SetParent(transform, false);
        statusObject.transform.localPosition = localOffset;

        statusText = statusObject.AddComponent<TextMesh>();
        statusText.fontSize = fontSize;
        statusText.characterSize = characterSize;
        statusText.anchor = TextAnchor.MiddleCenter;
        statusText.alignment = TextAlignment.Center;
        statusText.fontStyle = FontStyle.Bold;
        statusText.text = string.Empty;

        statusRenderer = statusObject.GetComponent<MeshRenderer>();
        statusRenderer.sortingOrder = 20;
        statusRenderer.enabled = false;
    }
}
