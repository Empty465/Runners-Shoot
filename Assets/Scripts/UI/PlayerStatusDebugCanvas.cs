using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusDebugCanvas : MonoBehaviour
{
    [SerializeField] private Vector2 screenOffset = new(20f, 20f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;

    private PlayerHealth playerHealth;
    private PlayerWeapon playerWeapon;
    private PlayerEnhancementPoints enhancementPoints;
    private PlayerParry playerParry;
    private PlayerDodge playerDodge;
    private Text statusText;
    private Text feedbackText;
    private float feedbackUntil;

    private void Awake()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        playerWeapon = FindAnyObjectByType<PlayerWeapon>();
        enhancementPoints = FindAnyObjectByType<PlayerEnhancementPoints>();
        playerParry = FindAnyObjectByType<PlayerParry>();
        playerDodge = FindAnyObjectByType<PlayerDodge>();
        CreateStatusText();
        SubscribeToHealthChanges();
        SubscribeToAmmoChanges();
        SubscribeToCombatFeedback();
        UpdateStatusText();
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            SubscribeToHealthChanges();
        }

        if (playerWeapon == null)
        {
            playerWeapon = FindAnyObjectByType<PlayerWeapon>();
            SubscribeToAmmoChanges();
        }

        if (enhancementPoints == null)
        {
            enhancementPoints = FindAnyObjectByType<PlayerEnhancementPoints>();
            SubscribeToEnhancementPointChanges();
        }

        if (playerParry == null)
        {
            playerParry = FindAnyObjectByType<PlayerParry>();
            SubscribeToCombatFeedback();
        }

        if (playerDodge == null)
        {
            playerDodge = FindAnyObjectByType<PlayerDodge>();
            SubscribeToCombatFeedback();
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

        if (playerWeapon == null)
        {
            playerWeapon = FindAnyObjectByType<PlayerWeapon>();
            SubscribeToAmmoChanges();
        }

        if (enhancementPoints == null)
        {
            enhancementPoints = FindAnyObjectByType<PlayerEnhancementPoints>();
            SubscribeToEnhancementPointChanges();
        }

        if (playerParry == null)
        {
            playerParry = FindAnyObjectByType<PlayerParry>();
            SubscribeToCombatFeedback();
        }

        if (playerDodge == null)
        {
            playerDodge = FindAnyObjectByType<PlayerDodge>();
            SubscribeToCombatFeedback();
        }

        UpdateStatusText();
        if (feedbackText != null && Time.time >= feedbackUntil)
        {
            feedbackText.text = string.Empty;
        }
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

    private void SubscribeToAmmoChanges()
    {
        if (playerWeapon != null)
        {
            playerWeapon.AmmoChanged += OnAmmoChanged;
        }
    }

    private void OnAmmoChanged(int currentAmmo)
    {
        UpdateStatusText();
    }

    private void SubscribeToEnhancementPointChanges()
    {
        if (enhancementPoints != null)
        {
            enhancementPoints.PointsChanged -= OnEnhancementPointsChanged;
            enhancementPoints.PointsChanged += OnEnhancementPointsChanged;
        }
    }

    private void OnEnhancementPointsChanged(int currentPoints)
    {
        UpdateStatusText();
    }

    private void SubscribeToCombatFeedback()
    {
        if (playerParry != null)
        {
            playerParry.ParrySucceeded -= OnParrySucceeded;
            playerParry.ParrySucceeded += OnParrySucceeded;
        }

        if (playerDodge != null)
        {
            playerDodge.DodgeSucceeded -= OnDodgeSucceeded;
            playerDodge.DodgeSucceeded += OnDodgeSucceeded;
        }
    }

    private void OnParrySucceeded()
    {
        ShowCombatFeedback("PARRY SUCCESS!", new Color(0.3f, 0.9f, 1f));
    }

    private void OnDodgeSucceeded()
    {
        ShowCombatFeedback("DODGE SUCCESS!", new Color(1f, 0.85f, 0.2f));
    }

    private void ShowCombatFeedback(string message, Color color)
    {
        if (feedbackText == null)
        {
            return;
        }

        feedbackText.text = message;
        feedbackText.color = color;
        feedbackUntil = Time.time + 0.8f;
    }

    private void UpdateStatusText()
    {
        if (statusText == null || playerHealth == null)
        {
            return;
        }

        statusText.text = $"HP: {playerHealth.CurrentHealth:0}/{playerHealth.MaxHealth:0} | " +
            $"ARMOR: {playerHealth.CurrentArmor:0}/{playerHealth.MaxArmor:0}";

        if (playerWeapon != null)
        {
            statusText.text += $" | AMMO: {playerWeapon.CurrentAmmo}/{playerWeapon.MaxAmmo}";
        }

        if (enhancementPoints != null)
        {
            statusText.text += $" | POINTS: {enhancementPoints.CurrentPoints}/{enhancementPoints.MaxPoints}";
        }
    }

    private void OnDisable()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.HealthChanged -= OnHealthChanged;
        playerHealth.ArmorChanged -= OnArmorChanged;

        if (playerWeapon != null)
        {
            playerWeapon.AmmoChanged -= OnAmmoChanged;
        }

        if (enhancementPoints != null)
        {
            enhancementPoints.PointsChanged -= OnEnhancementPointsChanged;
        }

        if (playerParry != null)
        {
            playerParry.ParrySucceeded -= OnParrySucceeded;
        }

        if (playerDodge != null)
        {
            playerDodge.DodgeSucceeded -= OnDodgeSucceeded;
        }
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
        textTransform.sizeDelta = new Vector2(420f, 80f);

        GameObject feedbackObject = new GameObject("PlayerCombatFeedbackText");
        feedbackObject.transform.SetParent(canvasObject.transform, false);

        feedbackText = feedbackObject.AddComponent<Text>();
        feedbackText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        feedbackText.fontSize = 32;
        feedbackText.fontStyle = FontStyle.Bold;
        feedbackText.alignment = TextAnchor.UpperCenter;
        feedbackText.horizontalOverflow = HorizontalWrapMode.Overflow;
        feedbackText.verticalOverflow = VerticalWrapMode.Overflow;
        feedbackText.text = string.Empty;

        RectTransform feedbackTransform = feedbackText.rectTransform;
        feedbackTransform.anchorMin = new Vector2(0.5f, 1f);
        feedbackTransform.anchorMax = new Vector2(0.5f, 1f);
        feedbackTransform.pivot = new Vector2(0.5f, 1f);
        feedbackTransform.anchoredPosition = new Vector2(0f, -screenOffset.y - 60f);
        feedbackTransform.sizeDelta = new Vector2(600f, 60f);
    }
}
