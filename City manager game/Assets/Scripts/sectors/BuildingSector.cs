using UnityEngine;

public enum BuildingSector { Education, Economy, Housing }

[CreateAssetMenu(fileName = "New Building", menuName = "City Manager/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public int cost;
    public BuildingSector sector;
    public float sectorValue;      // was int — now float, e.g. School = 0.1, Home = 20
    public int weeklyMoneyChange;

    [Header("School-specific (unused by other sectors)")]
    public int maxCapacity;
    public int quality;
}