using UnityEngine;

public class BuildingButtonUI : MonoBehaviour
{
    [SerializeField] private BuildingData _data;

    // Hooked up to this button's OnClick()
    public void OnClickSelect()
    {
        BuildingPlacementManager.Instance.BeginPlacement(_data);
    }
}