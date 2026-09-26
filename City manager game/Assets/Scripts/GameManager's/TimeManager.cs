using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float _secondsPerWeek = 10f;
    private float _timer;

    public static event Action OnWeekPassed;

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
        GameManager.Instance.ReportManagerInitialized(nameof(TimeManager));
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _secondsPerWeek)
        {
            _timer -= _secondsPerWeek;
            OnWeekPassed?.Invoke();
        }
    }
}