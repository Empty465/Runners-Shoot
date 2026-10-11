using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitFeedback : MonoBehaviour
{
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private float flashDuration = 0.16f;

    private sealed class VisualState
    {
        public Renderer Renderer;
        public int ColorProperty;
        public Color OriginalColor;
    }

    private EnemyHealth enemyHealth;
    private readonly List<VisualState> visualStates = new();
    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashRoutine;
    private float previousHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        Transform enemyVisual = transform.Find("EnemyDebugVisual");
        if (enemyVisual == null)
        {
            return;
        }

        propertyBlock = new MaterialPropertyBlock();
        Renderer[] renderers = enemyVisual.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer targetRenderer in renderers)
        {
            Material material = targetRenderer.sharedMaterial;
            if (material == null)
            {
                continue;
            }

            int colorProperty = material.HasProperty("_White")
                ? Shader.PropertyToID("_White")
                : material.HasProperty("_Color")
                    ? Shader.PropertyToID("_Color")
                    : -1;
            if (colorProperty < 0)
            {
                continue;
            }

            visualStates.Add(new VisualState
            {
                Renderer = targetRenderer,
                ColorProperty = colorProperty,
                OriginalColor = material.GetColor(colorProperty)
            });
        }
    }

    private void OnEnable()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            previousHealth = enemyHealth.CurrentHealth;
            enemyHealth.HealthChanged += OnHealthChanged;
        }
    }

    private void OnHealthChanged(float currentHealth)
    {
        bool tookDamage = currentHealth < previousHealth;
        previousHealth = currentHealth;
        if (tookDamage && visualStates.Count > 0)
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        float duration = Mathf.Max(0.01f, flashDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            Color currentColor = Color.Lerp(hitColor, Color.white, progress);
            for (int i = 0; i < visualStates.Count; i++)
            {
                VisualState visual = visualStates[i];
                visual.Renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(visual.ColorProperty, Color.Lerp(currentColor, visual.OriginalColor, progress));
                visual.Renderer.SetPropertyBlock(propertyBlock);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        ResetVisuals();
        flashRoutine = null;
    }

    private void ResetVisuals()
    {
        for (int i = 0; i < visualStates.Count; i++)
        {
            VisualState visual = visualStates[i];
            visual.Renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(visual.ColorProperty, visual.OriginalColor);
            visual.Renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged -= OnHealthChanged;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (propertyBlock != null)
        {
            ResetVisuals();
        }
    }
}
