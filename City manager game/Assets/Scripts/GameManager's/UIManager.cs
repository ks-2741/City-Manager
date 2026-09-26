using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _educationText;
    [SerializeField] private TMP_Text _populationText;

    [Header("Approval Panel")]
    [SerializeField] private GameObject _approvalPanel;
    [SerializeField] private TMP_Text _approvalPanelText;

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

        GameManager.Instance.ReportManagerInitialized(nameof(UIManager));



        PublicImageManager.Instance.OnApprovalChanged += UpdateApprovalPanel;
        UpdateApprovalPanel(PublicImageManager.Instance.Approval);

        _approvalPanel.SetActive(false); // hidden by default
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

        if (PublicImageManager.Instance != null)
            PublicImageManager.Instance.OnApprovalChanged -= UpdateApprovalPanel;
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

    private void UpdateApprovalPanel(float approval)
    {
        _approvalPanelText.text = $"Public Approval: {approval:0}%";
    }

    public void ToggleApprovalPanel()
    {
        _approvalPanel.SetActive(!_approvalPanel.activeSelf);
    }
}