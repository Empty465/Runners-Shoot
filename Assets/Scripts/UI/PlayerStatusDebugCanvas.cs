using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusDebugCanvas : MonoBehaviour
{
    [SerializeField] private Vector2 screenOffset = new(20f, 20f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float damageShakeDuration = 0.16f;
    [SerializeField] private float damageShakeStrength = 0.08f;

    private PlayerHealth playerHealth;
    private PlayerWeapon playerWeapon;
    private PlayerEnhancementPoints enhancementPoints;
    private PlayerParry playerParry;
    private PlayerDodge playerDodge;
    private Image healthFill;
    private Image armorFill;
    private Text healthValueText;
    private Text armorValueText;
    private Text ammoValueText;
    private Text pointsValueText;
    private Text dodgeStatusText;
    private Text parryStatusText;
    private Text feedbackText;
    private float feedbackUntil;
    private Image damageFlashOverlay;
    private float damageFlashRemaining;
    private Transform damageShakeCamera;
    private Vector3 damageShakeOrigin;
    private float damageShakeRemaining;
    private float activeDamageShakeDuration;

    private const float DamageFlashDuration = 0.38f;

    private void Awake()
    {
        playerHealth = FindPlayerComponent<PlayerHealth>();
        playerWeapon = FindPlayerComponent<PlayerWeapon>();
        enhancementPoints = FindPlayerComponent<PlayerEnhancementPoints>();
        playerParry = FindPlayerComponent<PlayerParry>();
        playerDodge = FindPlayerComponent<PlayerDodge>();
        Camera mainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        damageShakeCamera = mainCamera != null ? mainCamera.transform : null;
        if (damageShakeCamera != null)
        {
            damageShakeOrigin = damageShakeCamera.localPosition;
        }

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
            playerHealth = FindPlayerComponent<PlayerHealth>();
            SubscribeToHealthChanges();
        }

        if (playerWeapon == null)
        {
            playerWeapon = FindPlayerComponent<PlayerWeapon>();
            SubscribeToAmmoChanges();
        }

        if (enhancementPoints == null)
        {
            enhancementPoints = FindPlayerComponent<PlayerEnhancementPoints>();
            SubscribeToEnhancementPointChanges();
        }

        if (playerParry == null)
        {
            playerParry = FindPlayerComponent<PlayerParry>();
            SubscribeToCombatFeedback();
        }

        if (playerDodge == null)
        {
            playerDodge = FindPlayerComponent<PlayerDodge>();
            SubscribeToCombatFeedback();
        }

        if (feedbackText != null && feedbackText.text.Length > 0 && Time.time >= feedbackUntil)
        {
            feedbackText.text = string.Empty;
        }

        if (damageFlashOverlay != null && damageFlashRemaining > 0f)
        {
            damageFlashRemaining = Mathf.Max(0f, damageFlashRemaining - Time.unscaledDeltaTime);
            Color flashColor = damageFlashOverlay.color;
            flashColor.a = 0.24f * (damageFlashRemaining / DamageFlashDuration);
            damageFlashOverlay.color = flashColor;
        }

        UpdateDamageShake();
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
        playerHealth.DamageTaken += OnDamageTaken;
    }

    private void OnHealthChanged(float currentHealth)
    {
        UpdateStatusText();
    }

    private void OnArmorChanged(float currentArmor)
    {
        UpdateStatusText();
    }

    private void OnDamageTaken(bool damagedArmor)
    {
        ShowDamageFlash(damagedArmor
            ? new Color(1f, 0.55f, 0.16f, 1f)
            : new Color(1f, 0.16f, 0.2f, 1f));
    }

    private void ShowDamageFlash(Color color)
    {
        if (damageFlashOverlay != null)
        {
            color.a = 0.24f;
            damageFlashOverlay.color = color;
            damageFlashRemaining = DamageFlashDuration;
        }

        if (damageShakeCamera == null)
        {
            Camera mainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            damageShakeCamera = mainCamera != null ? mainCamera.transform : null;
        }

        if (damageShakeCamera != null)
        {
            if (damageShakeRemaining <= 0f)
            {
                damageShakeOrigin = damageShakeCamera.localPosition;
            }

            activeDamageShakeDuration = Mathf.Max(0.01f, damageShakeDuration);
            damageShakeRemaining = activeDamageShakeDuration;
        }
    }

    private void UpdateDamageShake()
    {
        if (damageShakeCamera == null || damageShakeRemaining <= 0f)
        {
            return;
        }

        damageShakeRemaining = Mathf.Max(0f, damageShakeRemaining - Time.unscaledDeltaTime);
        if (damageShakeRemaining <= 0f)
        {
            damageShakeCamera.localPosition = damageShakeOrigin;
            return;
        }

        float intensity = damageShakeRemaining / activeDamageShakeDuration;
        Vector2 offset = new Vector2(
            Mathf.Sin(Time.unscaledTime * 61f),
            Mathf.Cos(Time.unscaledTime * 47f)).normalized * (damageShakeStrength * intensity);
        damageShakeCamera.localPosition = damageShakeOrigin + new Vector3(offset.x, offset.y, 0f);
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
        if (playerHealth != null)
        {
            float healthRatio = playerHealth.MaxHealth > 0f
                ? Mathf.Clamp01(playerHealth.CurrentHealth / playerHealth.MaxHealth)
                : 0f;
            float armorRatio = playerHealth.MaxArmor > 0f
                ? Mathf.Clamp01(playerHealth.CurrentArmor / playerHealth.MaxArmor)
                : 0f;

            if (healthFill != null)
            {
                SetFillAmount(healthFill, healthRatio);
            }

            if (armorFill != null)
            {
                SetFillAmount(armorFill, armorRatio);
            }

            if (healthValueText != null)
            {
                healthValueText.text = $"{playerHealth.CurrentHealth:0} / {playerHealth.MaxHealth:0}";
            }

            if (armorValueText != null)
            {
                armorValueText.text = $"{playerHealth.CurrentArmor:0} / {playerHealth.MaxArmor:0}";
            }
        }

        if (ammoValueText != null && playerWeapon != null)
        {
            ammoValueText.text = $"{playerWeapon.CurrentAmmo:00} / {playerWeapon.MaxAmmo:00}";
            ammoValueText.color = playerWeapon.CurrentAmmo <= 5
                ? new Color(1f, 0.35f, 0.32f)
                : textColor;
        }

        if (pointsValueText != null && enhancementPoints != null)
        {
            pointsValueText.text = enhancementPoints.CurrentPoints.ToString();
        }

        UpdateAbilityStatus(dodgeStatusText, playerDodge != null && playerDodge.CanDodge,
            playerDodge != null ? playerDodge.DodgeCooldownRemaining : 0f);
        UpdateAbilityStatus(parryStatusText, playerParry != null && playerParry.CanParry,
            playerParry != null ? playerParry.ParryCooldownRemaining : 0f);
    }

    private static void UpdateAbilityStatus(Text statusText, bool isReady, float cooldownRemaining)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = isReady ? "1 / 1  READY" : $"0 / 1  {cooldownRemaining:0.0}s";
        statusText.color = isReady
            ? new Color(0.35f, 0.95f, 0.7f)
            : new Color(1f, 0.65f, 0.3f);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
            playerHealth.ArmorChanged -= OnArmorChanged;
            playerHealth.DamageTaken -= OnDamageTaken;
        }

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

        damageFlashRemaining = 0f;
        if (damageFlashOverlay != null)
        {
            Color flashColor = damageFlashOverlay.color;
            flashColor.a = 0f;
            damageFlashOverlay.color = flashColor;
        }

        if (damageShakeRemaining > 0f && damageShakeCamera != null)
        {
            damageShakeCamera.localPosition = damageShakeOrigin;
        }

        damageShakeRemaining = 0f;
    }

    private static T FindPlayerComponent<T>() where T : Component
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            T component = player.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
        }

        return FindAnyObjectByType<T>();
    }

    private void CreateStatusText()
    {
        GameObject canvasObject = new GameObject("PlayerStatusDebugCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        damageFlashOverlay = CreateImage(
            canvasObject.transform,
            "DamageFlashOverlay",
            new Color(1f, 0.16f, 0.2f, 0f),
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);
        damageFlashOverlay.rectTransform.offsetMin = Vector2.zero;
        damageFlashOverlay.rectTransform.offsetMax = Vector2.zero;
        damageFlashOverlay.raycastTarget = false;

        GameObject vitalsPanel = CreatePanel(
            canvasObject.transform,
            "VitalsPanel",
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(screenOffset.x, screenOffset.y),
            new Vector2(318f, 138f));
        CreateLabel(vitalsPanel.transform, "VitalsTitle", "VITALS", 13, new Color(0.62f, 0.72f, 0.84f),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -11f),
            new Vector2(120f, 20f), TextAnchor.UpperLeft);
        CreateLabel(vitalsPanel.transform, "HealthLabel", "HEALTH", 13, textColor,
            Vector2.zero, Vector2.zero, new Vector2(16f, 91f),
            new Vector2(100f, 19f), TextAnchor.MiddleLeft);
        healthValueText = CreateLabel(vitalsPanel.transform, "HealthValue", string.Empty, 14, textColor,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 91f),
            new Vector2(100f, 19f), TextAnchor.MiddleRight);
        healthFill = CreateBar(vitalsPanel.transform, "HealthBar", new Color(1f, 0.31f, 0.36f),
            new Vector2(16f, 75f), new Vector2(286f, 8f));

        CreateLabel(vitalsPanel.transform, "ArmorLabel", "ARMOR", 13, textColor,
            Vector2.zero, Vector2.zero, new Vector2(16f, 44f),
            new Vector2(100f, 19f), TextAnchor.MiddleLeft);
        armorValueText = CreateLabel(vitalsPanel.transform, "ArmorValue", string.Empty, 14, textColor,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 44f),
            new Vector2(100f, 19f), TextAnchor.MiddleRight);
        armorFill = CreateBar(vitalsPanel.transform, "ArmorBar", new Color(0.16f, 0.73f, 1f),
            new Vector2(16f, 28f), new Vector2(286f, 8f));

        GameObject resourcesPanel = CreatePanel(
            canvasObject.transform,
            "ResourcesPanel",
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-screenOffset.x, screenOffset.y),
            new Vector2(300f, 112f));
        CreateLabel(resourcesPanel.transform, "AmmoTitle", "AMMO", 13, new Color(0.62f, 0.72f, 0.84f),
            Vector2.zero, Vector2.zero, new Vector2(18f, 80f),
            new Vector2(110f, 20f), TextAnchor.MiddleLeft);
        ammoValueText = CreateLabel(resourcesPanel.transform, "AmmoValue", string.Empty, fontSize, textColor,
            Vector2.zero, Vector2.zero, new Vector2(18f, 36f),
            new Vector2(130f, 40f), TextAnchor.MiddleLeft);
        CreateImage(resourcesPanel.transform, "Divider", new Color(0.36f, 0.43f, 0.53f, 0.8f),
            Vector2.zero, Vector2.zero, new Vector2(154f, 17f), new Vector2(1f, 76f));
        CreateLabel(resourcesPanel.transform, "PointsTitle", "POINTS", 13, new Color(0.62f, 0.72f, 0.84f),
            Vector2.zero, Vector2.zero, new Vector2(174f, 80f),
            new Vector2(110f, 20f), TextAnchor.MiddleLeft);
        pointsValueText = CreateLabel(resourcesPanel.transform, "PointsValue", string.Empty, fontSize,
            new Color(1f, 0.78f, 0.28f), Vector2.zero, Vector2.zero, new Vector2(174f, 36f),
            new Vector2(100f, 40f), TextAnchor.MiddleLeft);

        GameObject abilitiesPanel = CreatePanel(
            canvasObject.transform,
            "AbilitiesPanel",
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-screenOffset.x, screenOffset.y + 122f),
            new Vector2(300f, 104f));
        CreateLabel(abilitiesPanel.transform, "AbilitiesTitle", "COMBAT", 13,
            new Color(0.62f, 0.72f, 0.84f), Vector2.zero, Vector2.zero, new Vector2(18f, 76f),
            new Vector2(110f, 20f), TextAnchor.MiddleLeft);
        CreateLabel(abilitiesPanel.transform, "DodgeLabel", "DASH", 14, textColor,
            Vector2.zero, Vector2.zero, new Vector2(18f, 46f),
            new Vector2(100f, 22f), TextAnchor.MiddleLeft);
        dodgeStatusText = CreateLabel(abilitiesPanel.transform, "DodgeStatus", string.Empty, 16, textColor,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 46f),
            new Vector2(150f, 22f), TextAnchor.MiddleRight);
        CreateLabel(abilitiesPanel.transform, "ParryLabel", "PARRY", 14, textColor,
            Vector2.zero, Vector2.zero, new Vector2(18f, 18f),
            new Vector2(100f, 22f), TextAnchor.MiddleLeft);
        parryStatusText = CreateLabel(abilitiesPanel.transform, "ParryStatus", string.Empty, 16, textColor,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f),
            new Vector2(150f, 22f), TextAnchor.MiddleRight);

        feedbackText = CreateLabel(canvasObject.transform, "PlayerCombatFeedbackText", string.Empty,
            32, textColor, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -screenOffset.y - 60f), new Vector2(600f, 60f),
            TextAnchor.UpperCenter);
        feedbackText.fontStyle = FontStyle.Bold;
    }

    private static GameObject CreatePanel(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        panel.GetComponent<Image>().color = new Color(0.035f, 0.055f, 0.09f, 0.9f);
        return panel;
    }

    private static Text CreateLabel(
        Transform parent,
        string objectName,
        string initialText,
        int size,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 dimensions,
        TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        Text label = textObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.text = initialText;

        RectTransform rectTransform = label.rectTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = alignment == TextAnchor.UpperCenter
            ? new Vector2(0.5f, 1f)
            : new Vector2(anchorMin.x, anchorMin.y);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = dimensions;
        return label;
    }

    private static Image CreateImage(
        Transform parent,
        string objectName,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Image CreateBar(Transform parent, string objectName, Color fillColor, Vector2 position, Vector2 size)
    {
        Image background = CreateImage(parent, objectName + "Background",
            new Color(0.12f, 0.16f, 0.22f, 1f), Vector2.zero, Vector2.zero, position, size);
        Image fill = CreateImage(background.transform, objectName + "Fill", fillColor,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return fill;
    }

    private static void SetFillAmount(Image fill, float amount)
    {
        RectTransform rectTransform = fill.rectTransform;
        rectTransform.anchorMax = new Vector2(amount, 1f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
