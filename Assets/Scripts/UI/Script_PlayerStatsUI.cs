// Assets/Scripts/UI/Script_PlayerStatsUI.cs
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Script_PlayerStatsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject statsPanel;  // The UI panel to toggle (set inactive by default)
    [SerializeField] private Transform statsContainer;  // Vertical Layout Group inside the panel for rows
    [SerializeField] private GameObject playerStatRowPrefab;  // Prefab for each player's stat row

    private Input_Controller inputController;  // Reference to input for toggling
    private Script_GameController gameController;  // To get player lists
    private List<GameObject> statRows = new List<GameObject>();  // Track instantiated rows for cleanup
    private bool isPanelVisible = false;

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>();

        // Ensure panel starts hidden
        if (statsPanel != null) statsPanel.SetActive(false);
    }

    void Update()
    {
        // Check for inputController
        if (inputController == null)
        {
            if (GameObject.FindGameObjectWithTag("LocalPlayer") != null)
            {
                inputController = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Input_Controller>();
            }

            return;
        }

        // Toggle panel when ShowStats input is pressed
        if (inputController != null && inputController.showStats)
        {
            isPanelVisible = !isPanelVisible;
            inputController.showStats = false;
            if (statsPanel != null) statsPanel.SetActive(isPanelVisible);
            UpdateStatsUI();  // Refresh UI when toggled
        }

        // Real-time update if panel is visible (can be optimized with events if needed)
        if (isPanelVisible)
        {
            UpdateStatsUI();
        }
    }

    private void UpdateStatsUI()
    {
        if (gameController == null || statsContainer == null || playerStatRowPrefab == null) return;

        // Clear existing rows
        foreach (var row in statRows)
        {
            Destroy(row);
        }
        statRows.Clear();

        // Get all players (alive + dead for complete stats)
        List<GameObject> allPlayers = new List<GameObject>(gameController.GetPlayers());
        allPlayers.AddRange(gameController.GetComponent<Script_GameController>().deadPlayers);  // Access deadPlayers via reflection or make public if needed

        // Limit to 4 players and create rows dynamically
        for (int i = 0; i < Mathf.Min(allPlayers.Count, 4); i++)
        {
            GameObject player = allPlayers[i];
            if (player == null) continue;

            // Get components (safe checks)
            Script_BaseStats baseStats = player.GetComponent<Script_BaseStats>();
            Script_PlayerUpgrades upgrades = player.GetComponent<Script_PlayerUpgrades>();
            if (baseStats == null || upgrades == null) continue;

            // Instantiate row
            GameObject row = Instantiate(playerStatRowPrefab, statsContainer);
            statRows.Add(row);

            // Get TMP_Text components (assume order: Name, Kills, Deaths, Points, Scrap)
            TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 5)
            {
                texts[0].text = baseStats.GetPlayerName();  // Name
                texts[1].text = upgrades.GetKills().ToString();  // Kills
                texts[2].text = upgrades.GetDeaths().ToString();  // Deaths
                texts[3].text = upgrades.GetPoints().ToString();  // Points
                texts[4].text = upgrades.GetScrap().ToString();   // Scrap
            }
        }
    }
}
