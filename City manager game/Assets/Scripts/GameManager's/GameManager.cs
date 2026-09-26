using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Tracks which managers have confirmed they're up and running
    private readonly HashSet<string> _initializedManagers = new HashSet<string>();
    private readonly string[] _expectedManagers = { "EconomyManager", "BuildingManager", "UIManager" };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Give managers a frame to Awake/Start and report in, then check
        Invoke(nameof(CheckAllManagersInitialized), 0.1f);
    }

    // Managers call this once they're ready
    public void ReportManagerInitialized(string managerName)
    {
        _initializedManagers.Add(managerName);
        Debug.Log($"[GameManager] {managerName} reported initialized.");
    }

    private void CheckAllManagersInitialized()
    {
        foreach (var expected in _expectedManagers)
        {
            if (!_initializedManagers.Contains(expected))
            {
                Debug.LogError($"[GameManager] {expected} failed to initialize! Safety net triggered.");
                // Later: pause game, show error screen, etc.
            }
        }
    }
}