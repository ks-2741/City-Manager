using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingData _data;
    public BuildingData Data => _data;

    private void Start()
    {
        BuildingManager.Instance.RegisterExistingBuilding(_data);
    }
}