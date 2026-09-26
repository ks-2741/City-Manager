using System;
using UnityEngine;

public class CityStatsManager : MonoBehaviour
{
    public static CityStatsManager Instance { get; private set; }

    public int TotalEducationCapacity { get; private set; }

    [SerializeField] private float _startingEducation = 1f;



    public float EducationScore { get; private set; }
    public int Population { get; private set; }

    public event Action<float> OnEducationChanged;
    public event Action<int> OnPopulationChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        EducationScore = _startingEducation;
        GameManager.Instance.ReportManagerInitialized(nameof(CityStatsManager));
    }

    private void OnEnable()
    {
        BuildingManager.OnBuildingPlaced += HandleBuildingAdded;
        BuildingManager.OnBuildingRegistered += HandleBuildingAdded;
    }

    private void OnDisable()
    {
        BuildingManager.OnBuildingPlaced -= HandleBuildingAdded;
        BuildingManager.OnBuildingRegistered -= HandleBuildingAdded;
    }

    private void HandleBuildingAdded(BuildingData data)
    {
        switch (data.sector)
        {
            case BuildingSector.Education:
                EducationScore = Mathf.Clamp(EducationScore + data.sectorValue, 1f, 10f);
                TotalEducationCapacity += data.maxCapacity;
                OnEducationChanged?.Invoke(EducationScore);
                break;

            case BuildingSector.Housing:
                Population += Mathf.RoundToInt(data.sectorValue);
                OnPopulationChanged?.Invoke(Population);
                break;
        }
    }
}