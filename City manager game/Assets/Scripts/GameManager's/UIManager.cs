using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TMP_Text _moneyText;

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
        GameManager.Instance.ReportManagerInitialized(nameof(UIManager));
    }

    private void OnEnable()
    {
        EconomyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
    }

    private void UpdateMoneyDisplay(int newAmount)
    {
        _moneyText.text = $"${newAmount:N0}";
    }
}