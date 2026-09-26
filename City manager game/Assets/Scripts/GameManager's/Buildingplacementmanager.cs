using UnityEngine;

public class BuildingPlacementManager : MonoBehaviour
{
    public static BuildingPlacementManager Instance { get; private set; }

    [SerializeField] private LayerMask _groundMask;    // terrain/ground the ghost can sit on
    [SerializeField] private LayerMask _blockedMask;   // roads, other buildings - invalid to place on
    [SerializeField] private Vector3 _placementCheckSize = new Vector3(2f, 2f, 2f);

    private enum PlacementState { Idle, Following, Locked }
    private PlacementState _state = PlacementState.Idle;

    private BuildingData _currentData;
    private GameObject _ghostInstance;
    private bool _currentlyValid;

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
        GameManager.Instance.ReportManagerInitialized(nameof(BuildingPlacementManager));
    }

    // Called by a build-menu button (BuildingButtonUI)
    public void BeginPlacement(BuildingData data)
    {
        if (_state != PlacementState.Idle) return; // already placing something

        _currentData = data;
        _ghostInstance = Instantiate(data.prefab);
        _state = PlacementState.Following;

        UIManager.Instance.CloseBuildPanel();
    }

    private void Update()
    {
        if (_state == PlacementState.Following)
            HandleFollowing();
        else if (_state == PlacementState.Locked)
            HandleLockedInput();
    }

    private void HandleFollowing()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, _groundMask))
        {
            _ghostInstance.transform.position = hit.point;
            _currentlyValid = !Physics.CheckBox(hit.point, _placementCheckSize / 2f, Quaternion.identity, _blockedMask);

            var ghostVisual = _ghostInstance.GetComponent<PlacementGhost>();
            if (ghostVisual != null)
                ghostVisual.SetValid(_currentlyValid);
        }

        if (Input.GetMouseButtonDown(0) && _currentlyValid)
        {
            _state = PlacementState.Locked;
        }

        if (Input.GetMouseButtonDown(1)) // right-click cancels entirely
        {
            CancelPlacement();
        }
    }

    private void HandleLockedInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryConfirmPurchase();
        }

        if (Input.GetMouseButtonDown(1)) // right-click while locked = go back to repositioning
        {
            _state = PlacementState.Following;
        }
    }

    private void TryConfirmPurchase()
    {
        if (!EconomyManager.Instance.CanAfford(_currentData.cost))
        {
            Debug.Log("Not enough money to build this.");
            return; // stay locked - player can reposition, cancel, or wait
        }

        Vector3 spawnPos = _ghostInstance.transform.position;
        Quaternion spawnRot = _ghostInstance.transform.rotation;
        Destroy(_ghostInstance);

        GameObject placed = Instantiate(_currentData.prefab, spawnPos, spawnRot);

        var buildingComponent = placed.GetComponent<Building>();
        if (buildingComponent == null)
            buildingComponent = placed.AddComponent<Building>();

        buildingComponent.SetData(_currentData);
        buildingComponent.skipAutoRegister = true;

        BuildingManager.Instance.PlaceBuilding(_currentData);

        ResetState();
    }

    private void CancelPlacement()
    {
        if (_ghostInstance != null)
            Destroy(_ghostInstance);

        ResetState();
    }

    private void ResetState()
    {
        _currentData = null;
        _ghostInstance = null;
        _state = PlacementState.Idle;
    }
}