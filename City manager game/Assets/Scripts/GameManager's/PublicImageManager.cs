using System;
using UnityEngine;

public class PublicImageManager : MonoBehaviour
{
    public static PublicImageManager Instance { get; private set; }

    [SerializeField] private float _startingApproval = 50f;
    [SerializeField] private float _economyPenalty = 2f;   // approval lost per week economy is negative
    [SerializeField] private float _economyBonus = 1f;     // approval gained per week economy is positive
    [SerializeField] private float _capacityPenaltyPer10Uncovered = 1f; // approval lost per 10 uncovered residents

    public float Approval { get; private set; }
    public event Action<float> OnApprovalChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        Approval = _startingApproval;
        GameManager.Instance.ReportManagerInitialized(nameof(PublicImageManager));
    }

    private void OnEnable() => TimeManager.OnWeekPassed += HandleWeekPassed;
    private void OnDisable() => TimeManager.OnWeekPassed -= HandleWeekPassed;

    private void HandleWeekPassed()
    {
        // Economy factor
        if (BuildingManager.Instance.LastWeeklyEconomyChange < 0)
            Approval -= _economyPenalty;
        else if (BuildingManager.Instance.LastWeeklyEconomyChange > 0)
            Approval += _economyBonus;

        // Education coverage factor
        int uncovered = CityStatsManager.Instance.Population - CityStatsManager.Instance.TotalEducationCapacity;
        if (uncovered > 0)
            Approval -= (uncovered / 10f) * _capacityPenaltyPer10Uncovered;

        Approval = Mathf.Clamp(Approval, 0f, 100f);
        OnApprovalChanged?.Invoke(Approval);
    }
}