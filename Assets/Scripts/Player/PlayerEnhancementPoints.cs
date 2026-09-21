using System;
using UnityEngine;

public class PlayerEnhancementPoints : MonoBehaviour
{
    [Header("Enhancement Points")]
    [SerializeField] private int startingPoints;
    [SerializeField] private int maxPoints = 999;

    public event Action<int> PointsChanged;
    public int CurrentPoints { get; private set; }
    public int MaxPoints => maxPoints;

    private void Awake()
    {
        maxPoints = Mathf.Max(0, maxPoints);
        CurrentPoints = Mathf.Clamp(startingPoints, 0, maxPoints);
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0 || CurrentPoints >= maxPoints)
        {
            return;
        }

        int updatedPoints = Mathf.Min(maxPoints, CurrentPoints + amount);
        if (updatedPoints == CurrentPoints)
        {
            return;
        }

        CurrentPoints = updatedPoints;
        PointsChanged?.Invoke(CurrentPoints);
        Debug.Log($"[Player] Enhancement Points: {CurrentPoints}/{MaxPoints}");
    }

    public bool TrySpendPoints(int amount)
    {
        if (amount <= 0 || CurrentPoints < amount)
        {
            return false;
        }

        CurrentPoints -= amount;
        PointsChanged?.Invoke(CurrentPoints);
        Debug.Log($"[Player] Enhancement Points Spent: {CurrentPoints}/{MaxPoints}");
        return true;
    }
}
