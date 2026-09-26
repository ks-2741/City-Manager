using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    public static event Action<int> OnBuildingPlaced; // cost, existing event

    private readonly List<BuildingData> _placedBuildings = new List<BuildingData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.ReportManagerInitialized(nameof(BuildingManager));
    }

    private void OnEnable() => TimeManager.OnWeekPassed += HandleWeekPassed;
    private void OnDisable() => TimeManager.OnWeekPassed -= HandleWeekPassed;

    // Call this from your existing confirm-tick placement code
    public void PlaceBuilding(BuildingData data)
    {
        _placedBuildings.Add(data);
        OnBuildingPlaced?.Invoke(data.cost);
    }

    private void HandleWeekPassed()
    {
        int weeklyTotal = 0;
        foreach (var building in _placedBuildings)
        {
            weeklyTotal += building.weeklyMoneyChange;
        }

        EconomyManager.Instance.ApplyWeeklyChange(weeklyTotal);
    }

    public void RegisterExistingBuilding(BuildingData data)
    {
        _placedBuildings.Add(data);
    }
}