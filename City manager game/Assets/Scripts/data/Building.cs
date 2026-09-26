using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingData _data;

    // Set true when this building is spawned at runtime through
    // BuildingPlacementManager, so it doesn't also register itself
    // for free via Start().
    public bool skipAutoRegister;

    public BuildingData Data => _data;

    private void Start()
    {
        if (!skipAutoRegister)
            BuildingManager.Instance.RegisterExistingBuilding(_data);
    }

    // Used when this component is added at runtime (AddComponent),
    // since the Inspector-only _data field won't be set in that case.
    public void SetData(BuildingData data)
    {
        _data = data;
    }
}