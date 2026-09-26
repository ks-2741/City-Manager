using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _educationText;
    [SerializeField] private TMP_Text _populationText;

    [Header("Approval Panel")]
    [SerializeField] private GameObject _approvalPanel;
    [SerializeField] private TMP_Text _approvalPanelText;

    [Header("Build Panel")]
    [SerializeField] private GameObject _buildPanel;

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
        EconomyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
        UpdateMoneyDisplay(EconomyManager.Instance.Money);

        CityStatsManager.Instance.OnEducationChanged += UpdateEducationDisplay;
        CityStatsManager.Instance.OnPopulationChanged += UpdatePopulationDisplay;
        UpdateEducationDisplay(CityStatsManager.Instance.EducationScore);
        UpdatePopulationDisplay(CityStatsManager.Instance.Population);

        _approvalPanel.SetActive(false);
        _buildPanel.SetActive(false);

        GameManager.Instance.ReportManagerInitialized(nameof(UIManager));
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;

        if (CityStatsManager.Instance != null)
        {
            CityStatsManager.Instance.OnEducationChanged -= UpdateEducationDisplay;
            CityStatsManager.Instance.OnPopulationChanged -= UpdatePopulationDisplay;
        }
    }

    private void UpdateMoneyDisplay(int newAmount)
    {
        _moneyText.text = $"£{newAmount:N0}";
    }

    private void UpdateEducationDisplay(float score)
    {
        _educationText.text = $"Education: {score:0.0}/10";
    }

    private void UpdatePopulationDisplay(int pop)
    {
        _populationText.text = $"Population: {pop}";
    }

    // Hooked up to the approval button's OnClick()
    public void ToggleApprovalPanel()
    {
        bool willBeActive = !_approvalPanel.activeSelf;
        _approvalPanel.SetActive(willBeActive);

        if (willBeActive)
            RefreshApprovalPanelDetails();
    }

    private void RefreshApprovalPanelDetails()
    {
        int schoolCount = BuildingManager.Instance.GetBuildingCount(BuildingSector.Education);
        int totalBuildings = BuildingManager.Instance.TotalBuildingCount;
        float education = CityStatsManager.Instance.EducationScore;
        float approval = PublicImageManager.Instance.Approval;

        _approvalPanelText.text =
            $"Public Approval: {approval:0}%\n\n" +
            $"Schools: {schoolCount}\n" +
            $"Education Rating: {education:0.0}/10\n" +
            $"Total Buildings: {totalBuildings}";
    }

    // Hooked up to the "open build menu" button's OnClick()
    public void OpenBuildPanel()
    {
        _buildPanel.SetActive(true);
    }

    // Called by BuildingPlacementManager once a building is selected
    public void CloseBuildPanel()
    {
        _buildPanel.SetActive(false);
    }
}