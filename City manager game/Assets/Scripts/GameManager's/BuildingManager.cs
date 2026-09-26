using System;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    public static event Action<int> OnBuildingPlaced;

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

    // TODO: hook up existing placement/confirm-tick logic here
    // TODO: raise OnBuildingPlaced(int cost) event once EconomyManager is ready to listen
}