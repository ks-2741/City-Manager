using UnityEngine;

public enum BuildingSector { Education, Economy, Housing }

[CreateAssetMenu(fileName = "New Building", menuName = "City Manager/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public int cost;
    public BuildingSector sector;
    public int sectorValue;       // how much this building raises its sector
    public int weeklyMoneyChange; // negative for schools (cost), positive for business (income), 0 for homes for now
}