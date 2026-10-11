using System.Collections;
using UnityEngine;

public class PlayerCombatFeedbackVFX : MonoBehaviour
{
    private const int RingSegments = 40;
    private const int ArcSegments = 22;

    private static readonly Color ParryCyan = new(0.2f, 0.92f, 1f, 1f);
    private static readonly Color ParryWhite = new(0.9f, 1f, 1f, 1f);
    private static readonly Color DodgeGold = new(1f, 0.72f, 0.18f, 1f);

    private Material effectMaterial;
    private GameObject activeEffect;
    private Coroutine effectRoutine;

    public void PlayParrySuccess()
    {
        PlayEffect(true, 1);
    }

    public void PlayDodgeSuccess(int direction)
    {
        PlayEffect(false, direction);
    }

    private void PlayEffect(bool isParry, int direction)
    {
        ClearEffect();

        if (!EnsureMaterial())
        {
            return;
        }

        activeEffect = new GameObject(isParry ? "ParrySuccessVFX" : "DodgeSuccessVFX");
        activeEffect.transform.position = transform.position + Vector3.back * 0.2f;

        LineRenderer ring = CreateLine(activeEffect.transform, "ImpactRing", 0.12f,
            isParry ? ParryCyan : DodgeGold);

        LineRenderer[] arcs = new LineRenderer[2];
        LineRenderer[] rays = new LineRenderer[8];
        LineRenderer[] trails = new LineRenderer[3];

        if (isParry)
        {
            arcs[0] = CreateLine(activeEffect.transform, "SlashArcA", 0.15f, ParryWhite);
            arcs[1] = CreateLine(activeEffect.transform, "SlashArcB", 0.1f, ParryCyan);
            for (int i = 0; i < rays.Length; i++)
            {
                rays[i] = CreateLine(activeEffect.transform, "Spark_" + i, 0.07f,
                    i % 2 == 0 ? ParryCyan : ParryWhite);
            }
        }
        else
        {
            for (int i = 0; i < trails.Length; i++)
            {
                trails[i] = CreateLine(activeEffect.transform, "DodgeTrail_" + i, 0.09f,
                    i == 1 ? ParryWhite : DodgeGold);
            }
        }

        float duration = isParry ? 0.38f : 0.28f;
        effectRoutine = StartCoroutine(AnimateEffect(
            activeEffect,
            ring,
            arcs,
            rays,
            trails,
            isParry,
            direction >= 0 ? 1 : -1,
            duration));
    }

    private IEnumerator AnimateEffect(
        GameObject effect,
        LineRenderer ring,
        LineRenderer[] arcs,
        LineRenderer[] rays,
        LineRenderer[] trails,
        bool isParry,
        int direction,
        float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            Vector3 center = effect.transform.position;

            SetAlphaAndWidth(ring, 1f - progress);
            DrawCircle(ring, center, Mathf.Lerp(0.45f, isParry ? 1.75f : 1.2f, easedProgress));

            if (isParry)
            {
                SetAlphaAndWidth(arcs[0], 1f - progress);
                SetAlphaAndWidth(arcs[1], 1f - progress);
                DrawArc(arcs[0], center, Mathf.Lerp(0.82f, 1.45f, easedProgress), 205f + progress * 45f, 230f);
                DrawArc(arcs[1], center, Mathf.Lerp(1.05f, 1.68f, easedProgress), 18f - progress * 35f, 155f);
                DrawSparks(rays, center, progress);
            }
            else
            {
                for (int i = 0; i < trails.Length; i++)
                {
                    SetAlphaAndWidth(trails[i], 1f - progress);
                    float yOffset = (i - 1) * 0.2f;
                    Vector3 near = center + new Vector3(-direction * 0.2f, yOffset, 0f);
                    Vector3 far = center + new Vector3(-direction * Mathf.Lerp(1.2f, 0.55f, easedProgress), yOffset, 0f);
                    trails[i].positionCount = 2;
                    trails[i].SetPosition(0, near);
                    trails[i].SetPosition(1, far);
                }
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        effectRoutine = null;
        activeEffect = null;
        Destroy(effect);
    }

    private bool EnsureMaterial()
    {
        if (effectMaterial != null)
        {
            return true;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Debug.LogError("[PlayerCombatFeedbackVFX] Could not find the Sprites/Default shader.");
            return false;
        }

        effectMaterial = new Material(shader)
        {
            name = "PlayerCombatFeedbackVFX"
        };
        return true;
    }

    private LineRenderer CreateLine(Transform parent, string objectName, float width, Color color)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(parent, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = false;
        line.positionCount = 0;
        line.startWidth = width;
        line.endWidth = width * 0.45f;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.alignment = LineAlignment.View;
        line.sharedMaterial = effectMaterial;
        line.sortingOrder = 100;
        line.startColor = color;
        line.endColor = color;
        return line;
    }

    private static void DrawCircle(LineRenderer line, Vector3 center, float radius)
    {
        line.loop = true;
        line.positionCount = RingSegments;
        for (int i = 0; i < RingSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RingSegments;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    private static void DrawArc(LineRenderer line, Vector3 center, float radius, float startDegrees, float sweepDegrees)
    {
        line.loop = false;
        line.positionCount = ArcSegments;
        for (int i = 0; i < ArcSegments; i++)
        {
            float angle = (startDegrees + sweepDegrees * i / (ArcSegments - 1)) * Mathf.Deg2Rad;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    private static void DrawSparks(LineRenderer[] sparks, Vector3 center, float progress)
    {
        for (int i = 0; i < sparks.Length; i++)
        {
            SetAlphaAndWidth(sparks[i], 1f - progress);
            float angle = (22.5f + i * 45f + progress * 30f) * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            float startRadius = Mathf.Lerp(0.7f, 1.45f, progress);
            float endRadius = startRadius + Mathf.Lerp(0.38f, 0.04f, progress);
            sparks[i].positionCount = 2;
            sparks[i].SetPosition(0, center + (Vector3)(direction * startRadius));
            sparks[i].SetPosition(1, center + (Vector3)(direction * endRadius));
        }
    }

    private static void SetAlphaAndWidth(LineRenderer line, float alpha)
    {
        Color startColor = line.startColor;
        Color endColor = line.endColor;
        startColor.a = alpha;
        endColor.a = alpha;
        line.startColor = startColor;
        line.endColor = endColor;
        line.startWidth = Mathf.Lerp(0.02f, 0.15f, alpha);
        line.endWidth = line.startWidth * 0.45f;
    }

    private void ClearEffect()
    {
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);
            effectRoutine = null;
        }

        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
        }
    }

    private void OnDisable()
    {
        ClearEffect();
    }

    private void OnDestroy()
    {
        if (effectMaterial != null)
        {
            Destroy(effectMaterial);
            effectMaterial = null;
        }
    }
}
