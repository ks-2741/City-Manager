using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private int _startingMoney = 1000;

    public int Money { get; private set; }
    public event Action<int> OnMoneyChanged;

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
        Money = _startingMoney;
        OnMoneyChanged?.Invoke(Money);

        GameManager.Instance.ReportManagerInitialized(nameof(EconomyManager));
    }

    private void OnEnable()
    {
        BuildingManager.OnBuildingPlaced += HandleBuildingPlaced;
    }

    private void OnDisable()
    {
        BuildingManager.OnBuildingPlaced -= HandleBuildingPlaced;
    }

    private void HandleBuildingPlaced(int cost)
    {
        Money -= cost;
        OnMoneyChanged?.Invoke(Money);
    }

    // BuildingManager calls this before placing, to check affordability
    public bool CanAfford(int cost)
    {
        return Money >= cost;
    }

    public void ApplyWeeklyChange(int amount)
    {
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }

    private void HandleBuildingPlaced(BuildingData data)
    {
        Money -= data.cost;
        OnMoneyChanged?.Invoke(Money);
    }
}