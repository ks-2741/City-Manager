using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    private readonly List<BuildingData> _placedBuildings = new List<BuildingData>();

    public static event Action<BuildingData> OnBuildingPlaced;
    public static event Action<BuildingData> OnBuildingRegistered;

    public static event Action<int> OnWeeklyEconomyTick;
    public int LastWeeklyEconomyChange { get; private set; }

    public int TotalBuildingCount => _placedBuildings.Count;

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
        OnBuildingPlaced?.Invoke(data);
    }

    // Call this from buildings already sitting in the scene at load time
    public void RegisterExistingBuilding(BuildingData data)
    {
        _placedBuildings.Add(data);
        OnBuildingRegistered?.Invoke(data);
    }

    public int GetBuildingCount(BuildingSector sector)
    {
        int count = 0;
        foreach (var b in _placedBuildings)
        {
            if (b.sector == sector)
                count++;
        }
        return count;
    }

    private void HandleWeekPassed()
    {
        int weeklyTotal = 0;
        foreach (var building in _placedBuildings)
            weeklyTotal += building.weeklyMoneyChange;

        LastWeeklyEconomyChange = weeklyTotal;
        EconomyManager.Instance.ApplyWeeklyChange(weeklyTotal);
        OnWeeklyEconomyTick?.Invoke(weeklyTotal);
    }
}