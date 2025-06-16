using System.Collections.Generic;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Script_BaseStats;

public class Script_PlayerUpgrades : NetworkBehaviour
{
    [Header("Currencies")]
    // [SerializeField] int points = 500;  // Replaced with NetworkVariable for syncing
    // [SerializeField] int scrap = 0;     // Replaced with NetworkVariable for syncing
    private NetworkVariable<int> points = new NetworkVariable<int>(500);  // Synced points
    public int GetPoints() { return points.Value; }
    private NetworkVariable<int> scrap = new NetworkVariable<int>(0);     // Synced scrap
    public int GetScrap() { return scrap.Value; }

    // New: Synced kills and deaths for stats tracking
    [Header("Player Stats")]
    private NetworkVariable<int> kills = new NetworkVariable<int>(0);
    public int GetKills() { return kills.Value; }
    private NetworkVariable<int> deaths = new NetworkVariable<int>(0);
    public int GetDeaths() { return deaths.Value; }

    // Mod Methods
    private List<Action> scrapMethods = new List<Action>();
    private List<Action> killMethods = new List<Action>();

    void Start()
    {
        if (IsLocalPlayer)
        {
            Script_UIManager.Instance.pointsText.text = "Points: " + points.Value;
            Script_UIManager.Instance.scrapText.text = "Scrap: " + scrap.Value;
        }

        // New: Set up callbacks for real-time UI updates when values change
        points.OnValueChanged += OnPointsChanged;
        scrap.OnValueChanged += OnScrapChanged;
    }

    // New: Callback for points change to update UI
    private void OnPointsChanged(int oldValue, int newValue)
    {
        if (IsLocalPlayer)
        {
            Script_UIManager.Instance.pointsText.text = "Points: " + newValue;
        }
    }

    // New: Callback for scrap change to update UI
    private void OnScrapChanged(int oldValue, int newValue)
    {
        if (IsLocalPlayer)
        {
            Script_UIManager.Instance.scrapText.text = "Scrap: " + newValue;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void AddPointsRpc(int value)
    {
        int newPoints = points.Value + value;
        points.Value = Mathf.Max(newPoints, 0);  // Prevent negative
        if (IsLocalPlayer)
        {
            EnemyKillMethods();
        }
    }

    // Updated: Use NetworkVariable for bonus points
    public void AddBonusPoints(int value)
    {
        points.Value += value;
    }

    // Updated: Use NetworkVariable for scrap
    public void AddScrap(int value)
    {
        foreach (Action action in scrapMethods)
        {
            action();
        }
        int newScrap = scrap.Value + value;
        scrap.Value = Mathf.Max(newScrap, 0);  // Prevent negative
    }

    // Updated: Use NetworkVariable
    public void RemoveScrap(int value)
    {
        scrap.Value -= value;
    }

    // Updated: Use NetworkVariable for bonus scrap
    public void AddBonusScrap(int value)
    {
        scrap.Value += value;
    }

    // New: RPC to increment kills (called from Script_GameController on enemy death)
    [Rpc(SendTo.Server)]
    public void IncrementKillsRpc()
    {
        kills.Value += 1;
    }

    // New: RPC to increment deaths (called from Script_GameController on player death)
    [Rpc(SendTo.Server)]
    public void IncrementDeathsRpc()
    {
        deaths.Value += 1;
    }

    public void AddScrapMethod(Action method)
    {
        scrapMethods.Add(method);
    }

    public void RemoveScrapMethod(Action method)
    {
        scrapMethods.Remove(method);
    }

    public void AddKillMethod(Action method)
    {
        killMethods.Add(method);
    }

    public void RemoveKillMethod(Action method)
    {
        killMethods.Remove(method);
    }

    public void EnemyKillMethods()
    {
        foreach (Action action in killMethods)
        {
            action();
        }
    }
}
