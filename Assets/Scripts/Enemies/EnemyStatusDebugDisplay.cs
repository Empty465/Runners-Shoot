using UnityEngine;

public class EnemyStatusDebugDisplay : MonoBehaviour
{
    [SerializeField] private Vector3 localOffset = new(0f, 2.2f, -0.3f);
    [SerializeField] private int fontSize = 32;
    [SerializeField] private float characterSize = 0.1f;
    [SerializeField] private Color staggerColor = Color.yellow;
    [SerializeField] private Color stunColor = Color.red;
    [SerializeField] private Vector2 healthBarOffset = new(0f, 1.55f);
    [SerializeField] private Vector2 healthBarSize = new(1.4f, 0.12f);
    [SerializeField] private Color healthColor = new(0.15f, 0.9f, 0.72f);
    [SerializeField] private Color lowHealthColor = new(1f, 0.24f, 0.2f);
    [SerializeField] private Color healthBarFrameColor = new(1f, 0.63f, 0.2f);

    private static Sprite healthBarSprite;
    private EnemyHealth enemyHealth;
    private BasicEnemyController enemyController;
    private EnemyStun enemyStun;
    private EnemyStagger enemyStagger;
    private TextMesh statusText;
    private TextMesh archetypeText;
    private MeshRenderer statusRenderer;
    private SpriteRenderer healthBarFrame;
    private SpriteRenderer healthBarBackground;
    private SpriteRenderer healthBarFill;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyController = GetComponent<BasicEnemyController>();
        enemyStun = GetComponent<EnemyStun>();
        enemyStagger = GetComponent<EnemyStagger>();
        BindOrCreateStatusText();
        BindOrCreateArchetypeText();
        BindOrCreateHealthBar();
    }

    private void OnEnable()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged += OnHealthChanged;
            UpdateHealthBar(enemyHealth.CurrentHealth);
        }
    }

    private void OnHealthChanged(float currentHealth)
    {
        UpdateHealthBar(currentHealth);
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
        statusRenderer.sortingOrder = 21;
        statusRenderer.enabled = false;
    }

    private void BindOrCreateStatusText()
    {
        Transform statusObject = transform.Find("EnemyStatusDebugText");
        if (statusObject == null)
        {
            CreateStatusText();
            return;
        }

        statusText = statusObject.GetComponent<TextMesh>();
        statusRenderer = statusObject.GetComponent<MeshRenderer>();
    }

    private void CreateArchetypeText()
    {
        if (enemyController == null)
        {
            return;
        }

        GameObject archetypeObject = new GameObject("EnemyArchetypeDebugText");
        archetypeObject.transform.SetParent(transform, false);
        archetypeObject.transform.localPosition = localOffset + new Vector3(0f, 0.42f, -0.05f);

        archetypeText = archetypeObject.AddComponent<TextMesh>();
        archetypeText.text = $"TEST: {enemyController.Archetype.ToString().ToUpperInvariant()}";
        archetypeText.fontSize = 32;
        archetypeText.characterSize = 0.1f;
        archetypeText.anchor = TextAnchor.MiddleCenter;
        archetypeText.alignment = TextAlignment.Center;
        archetypeText.fontStyle = FontStyle.Bold;
        archetypeObject.GetComponent<MeshRenderer>().sortingOrder = 22;
        RefreshArchetypeLabel();
    }

    private void BindOrCreateArchetypeText()
    {
        Transform archetypeObject = transform.Find("EnemyArchetypeDebugText");
        if (archetypeObject == null)
        {
            CreateArchetypeText();
            return;
        }

        archetypeText = archetypeObject.GetComponent<TextMesh>();
    }

    public void RefreshArchetypeLabel()
    {
        if (archetypeText == null || enemyController == null)
        {
            return;
        }

        EnemyAttack attack = enemyController.GetComponent<EnemyAttack>();
        string parryTag = attack == null ? string.Empty : attack.IsParryable ? "\n(PARRY OK)" : "\n(NO PARRY)";
        archetypeText.text = $"TEST: {enemyController.Archetype.ToString().ToUpperInvariant()}{parryTag}";
        archetypeText.color = enemyController.Archetype switch
        {
            EnemyArchetype.Slag => new Color(0.82f, 0.67f, 0.53f),
            EnemyArchetype.Pawn => new Color(0.52f, 0.8f, 1f),
            EnemyArchetype.Knight => new Color(1f, 0.72f, 0.39f),
            EnemyArchetype.Archer => new Color(0.4f, 1f, 0.82f),
            EnemyArchetype.Baron => new Color(1f, 0.4f, 0.38f),
            _ => Color.white
        };
    }

    private void CreateHealthBar()
    {
        if (healthBarSprite == null)
        {
            healthBarSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            healthBarSprite.name = "EnemyHealthBarSprite";
        }

        Transform barRoot = new GameObject("EnemyHealthBar").transform;
        barRoot.SetParent(transform, false);
        barRoot.localPosition = new Vector3(healthBarOffset.x, healthBarOffset.y, -0.35f);
        barRoot.localScale = new Vector3(healthBarSize.x, healthBarSize.y, 1f);

        healthBarFrame = CreateBarLayer(barRoot, "Frame", healthBarFrameColor, 18, 1.12f);
        healthBarBackground = CreateBarLayer(barRoot, "Background", new Color(0.035f, 0.055f, 0.09f), 19, 1f);
        healthBarFill = CreateBarLayer(barRoot, "Fill", healthColor, 20, 0.68f);
    }

    private void BindOrCreateHealthBar()
    {
        Transform barRoot = transform.Find("EnemyHealthBar");
        if (barRoot == null)
        {
            CreateHealthBar();
            return;
        }

        Transform frame = barRoot.Find("Frame");
        Transform background = barRoot.Find("Background");
        Transform fill = barRoot.Find("Fill");
        if (frame == null || background == null || fill == null)
        {
            CreateHealthBar();
            return;
        }

        healthBarFrame = frame.GetComponent<SpriteRenderer>();
        healthBarBackground = background.GetComponent<SpriteRenderer>();
        healthBarFill = fill.GetComponent<SpriteRenderer>();
    }

    private static SpriteRenderer CreateBarLayer(
        Transform parent,
        string objectName,
        Color color,
        int sortingOrder,
        float inset)
    {
        GameObject layerObject = new GameObject(objectName);
        layerObject.transform.SetParent(parent, false);
        layerObject.transform.localScale = new Vector3(inset, inset, 1f);
        SpriteRenderer spriteRenderer = layerObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = healthBarSprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
        return spriteRenderer;
    }

    private void UpdateHealthBar(float currentHealth)
    {
        if (healthBarFill == null || enemyHealth == null)
        {
            return;
        }

        float healthRatio = enemyHealth.MaxHealth > 0f
            ? Mathf.Clamp01(currentHealth / enemyHealth.MaxHealth)
            : 0f;
        healthBarFill.transform.localScale = new Vector3(0.68f * healthRatio, 0.68f, 1f);
        healthBarFill.transform.localPosition = new Vector3(-0.34f * (1f - healthRatio), 0f, 0f);
        healthBarFill.color = Color.Lerp(lowHealthColor, healthColor, healthRatio);
        bool isAlive = currentHealth > 0f;
        healthBarFrame.enabled = isAlive;
        healthBarBackground.enabled = isAlive;
        healthBarFill.enabled = isAlive;
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged -= OnHealthChanged;
        }

        if (statusRenderer != null)
        {
            statusRenderer.enabled = false;
        }

        if (healthBarFrame != null)
        {
            healthBarFrame.enabled = false;
            healthBarBackground.enabled = false;
            healthBarFill.enabled = false;
        }
    }
}
