using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusDebugCanvas : MonoBehaviour
{
    [SerializeField] private Vector2 screenOffset = new(20f, 20f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;

    private PlayerHealth playerHealth;
    private Text statusText;

    private void Awake()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        CreateStatusText();
        SubscribeToHealthChanges();
        UpdateStatusText();
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            SubscribeToHealthChanges();
        }

        if (playerHealth != null)
        {
            UpdateStatusText();
        }
    }

    private void OnGUI()
    {
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            SubscribeToHealthChanges();
        }

        UpdateStatusText();
    }

    private void SubscribeToHealthChanges()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.HealthChanged += OnHealthChanged;
        playerHealth.ArmorChanged += OnArmorChanged;
    }

    private void OnHealthChanged(float currentHealth)
    {
        UpdateStatusText();
    }

    private void OnArmorChanged(float currentArmor)
    {
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (statusText == null || playerHealth == null)
        {
            return;
        }

        statusText.text = $"HP: {playerHealth.CurrentHealth:0}/{playerHealth.MaxHealth:0} | " +
            $"ARMOR: {playerHealth.CurrentArmor:0}/{playerHealth.MaxArmor:0}";
    }

    private void OnDisable()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.HealthChanged -= OnHealthChanged;
        playerHealth.ArmorChanged -= OnArmorChanged;
    }

    private void CreateStatusText()
    {
        GameObject canvasObject = new GameObject("PlayerStatusDebugCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("PlayerStatusDebugText");
        textObject.transform.SetParent(canvasObject.transform, false);

        statusText = textObject.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = fontSize;
        statusText.color = textColor;
        statusText.alignment = TextAnchor.UpperLeft;
        statusText.horizontalOverflow = HorizontalWrapMode.Overflow;
        statusText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textTransform = statusText.rectTransform;
        textTransform.anchorMin = new Vector2(0f, 1f);
        textTransform.anchorMax = new Vector2(0f, 1f);
        textTransform.pivot = new Vector2(0f, 1f);
        textTransform.anchoredPosition = new Vector2(screenOffset.x, -screenOffset.y);
        textTransform.sizeDelta = new Vector2(260f, 80f);
    }
}
