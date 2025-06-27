using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Script_UIManager : NetworkBehaviour
{

    [Header("UI Elements")]
    [Header("Gameplay Elements")]
    [SerializeField] public TMP_Text pointsText;
    [SerializeField] public TMP_Text scrapText;
    [SerializeField] public Slider healthBar;
    [SerializeField] public Slider shieldBar;
    [SerializeField] public TMP_Text gunInfoText;
    [SerializeField] GameObject modIconHolder;
    [SerializeField] GameObject spectatorUI;

    [Header("Class Selection")]
    [SerializeField] private TMP_Dropdown classDropdown;
    private string selectedClass = "Pistol"; // Default to Pistol

    [Header("Leveling UI")]
    [SerializeField] private TMP_Text accountLevelText; // New: Assign in Inspector to display "Account Level: X"
    [SerializeField] private TMP_Text classLevelText;    // New: Assign to display "Class Level: X"

    [Header("Network UI Elements")]
    [SerializeField] GameObject networkUI;
    [SerializeField] GameObject lobbyUI;
    [SerializeField] Button startButton;
    [SerializeField] TMP_Text lobbyText;
    [SerializeField] TMP_InputField lobbyName;
    [SerializeField] Script_LobbyPlayerList lobbyPlayerList;
    [SerializeField] Button leaveLobbyButton;
    [SerializeField] public Button joinLobbyButton;

    [Header("Currency UI")]
    [SerializeField] private TMP_Text currencyText; // New: Assign in Inspector to display currency
    [SerializeField] private Button shopButton; // New: Button to open weapon skin shop

    [SerializeField] NetworkObject playerPrefab;

    private Script_BaseStats currentSpectator = null;
    private int currentSpectatorIndex = 0;
    private string localPlayerName = "";

    public static Script_UIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Script_SteamGameNetworkManager.instance.currentLobby != null)
        {
            if (SteamClient.SteamId.Value == Script_SteamGameNetworkManager.instance.currentLobby.Value.Owner.Id)
                SwitchToLobbyUI(true);
            else
                SwitchToLobbyUI(false);
        }

        if (classDropdown != null)
        {
            classDropdown.ClearOptions();
            List<string> options = new List<string> { "Pistol" };

            // New: Conditionally add Automatic Rifle based on account level
            if (LevelManager.Instance != null && LevelManager.Instance.IsClassUnlocked("Automatic Rifle"))
            {
                options.Add("Automatic Rifle");
            }
            else
            {
                Debug.Log("Automatic Rifle is locked until Account Level 5.");
            }

            classDropdown.AddOptions(options);
            classDropdown.onValueChanged.AddListener(OnClassSelected);
        }

        // New: Setup shop button
        if (shopButton != null)
        {
            shopButton.onClick.AddListener(() => Script_WeaponSkinShop.Instance.OpenShop());
        }

        // New: Update UI with current levels and currency
        UpdateLevelUI();
    }

    public void SwitchToLobbyUI(bool host)
    {
        networkUI.GetComponent<RectTransform>().localScale = Vector3.zero;
        if (!host)
        {
            startButton.interactable = false;
        }
    }

    public void JoinLobby()
    {
        leaveLobbyButton.interactable = true;
    }

    public void CreateLobby()
    {
        Script_SteamGameNetworkManager.instance.StartHost(4);
    }

    public void LeaveLobby()
    {
        networkUI.GetComponent<RectTransform>().localScale = new Vector3(1.3f, 1.3f, 1.3f);
        leaveLobbyButton.interactable = false;
        ResetLobbyPlayerList();
    }

    public void LeaveLobbyButton()
    {
        Script_SteamGameNetworkManager.instance.Disconnected();
    }

    private void OnClassSelected(int index)
    {
        selectedClass = classDropdown.options[index].text;
        // New: Ensure Automatic Rifle can't be selected if locked (fallback)
        if (selectedClass == "Automatic Rifle" && (LevelManager.Instance == null || !LevelManager.Instance.IsClassUnlocked("Automatic Rifle")))
        {
            selectedClass = "Pistol";
            classDropdown.value = 0; // Reset to Pistol
        }
        UpdateLevelUI(); // Refresh class level display
    }

    public string GetSelectedClass()
    {
        return selectedClass;
    }

    // You'll need to call this when spawning the player to equip the correct weapon
    public void EquipSelectedWeapon(GameObject player)
    {
        // Remove any existing weapon
        Weapon existingWeapon = player.GetComponentInChildren<Weapon>();
        if (existingWeapon != null)
        {
            Destroy(existingWeapon.gameObject);
        }

        // Load and attach the selected weapon prefab
        string weaponPrefabPath = selectedClass == "Pistol" ?
            "Alt_Revolver_Polished_Isolated" : "Automatic Rifle";

        GameObject weaponPrefab = Resources.Load<GameObject>(weaponPrefabPath);
        if (weaponPrefab != null)
        {
            // Existing weapon instantiation
            GameObject weaponInstance = Instantiate(weaponPrefab,
                player.transform.Find("Player Camera/FPS_Arms/Armature/Root/R.UpperArm/R.Forearm/R.Hand/WeaponPosition"));
            weaponInstance.GetComponent<Weapon>().SetFPSArms(player.transform.Find("Player Camera/FPS_Arms").gameObject);

            // New: Apply equipped skin instead of level-based skin
            if (LevelManager.Instance != null)
            {
                string equippedSkin = LevelManager.Instance.GetEquippedSkin(selectedClass);
                if (equippedSkin != "Default" && Script_WeaponSkinShop.Instance != null)
                {
                    Material skinMaterial = Script_WeaponSkinShop.Instance.GetSkinMaterial(equippedSkin);
                    if (skinMaterial != null)
                    {
                        Renderer[] weaponRenderers = weaponInstance.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in weaponRenderers)
                        {
                            renderer.material = skinMaterial;
                        }
                        Debug.Log($"Applied skin {equippedSkin} to {selectedClass}");
                    }
                }
                // Fallback to level-based skin if no purchased skin equipped
                else if (LevelManager.Instance.IsClassSkinUnlocked(selectedClass))
                {
                    Renderer[] weaponRenderer = weaponInstance.GetComponentsInChildren<Renderer>();
                    if (weaponRenderer != null)
                    {
                        foreach (Renderer renderer in weaponRenderer)
                        {
                            renderer.material.color = UnityEngine.Color.yellow; // Simple example: Change to yellow for "unlocked" skin
                        }
                        Debug.Log($"{selectedClass} level-based skin applied!");
                    }
                }
            }
        }
    }

    // New: Method to update level and currency display
    private void UpdateLevelUI()
    {
        if (LevelManager.Instance != null)
        {
            if (accountLevelText != null)
                accountLevelText.text = $"Account Level: {LevelManager.Instance.AccountLevel}";
            if (classLevelText != null)
                classLevelText.text = $"{selectedClass} Level: {GetClassLevel(selectedClass)}";
            if (currencyText != null)
                currencyText.text = $"Coins: {LevelManager.Instance.AccountCurrency}";
        }
    }

    // New: Helper to get class level for UI
    private int GetClassLevel(string className)
    {
        if (LevelManager.Instance == null) return 0;
        return className == "Pistol" ? LevelManager.Instance.PistolLevel : LevelManager.Instance.AutomaticRifleLevel;
    }

    public void ToggleNetworkUI(bool toggle)
    {
        networkUI.SetActive(toggle);
        lobbyUI.SetActive(false);
    }

    public void ToggleGameplayUI(bool toggle)
    {
        pointsText.enabled = toggle;
        scrapText.enabled = toggle;
        healthBar.gameObject.SetActive(toggle);
        gunInfoText.enabled = toggle;
        modIconHolder.SetActive(toggle);
    }

    public void ToggleSpectatorUI(bool toggle)
    {
        spectatorUI.SetActive(toggle);
    }

    public string GetUsername()
    {
        return localPlayerName;
    }

    public void SetLocalUsername(string name)
    {
        localPlayerName = name;
    }

    public void SetSpectatorInfo(string name)
    {
        spectatorUI.GetComponentInChildren<TMP_Text>().text = "Currently Spectating: " + name;
    }

    public void SpectatorCamera(int index)
    {
        if (GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().GetPlayers().Count <= 0)
        {
            return;
        }

        currentSpectator = GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().GetPlayers()[index].GetComponent<Script_BaseStats>();
        currentSpectatorIndex = index;
        SetSpectatorInfo(GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().GetPlayers()[index].GetComponent<Script_BaseStats>().GetPlayerName());
        currentSpectator.GetComponentInChildren<CinemachineCamera>().enabled = true;
    }

    public void CheckSpectatorCamera()
    {
        if (currentSpectator != null && currentSpectator.GetDeathStatus())
        {
            SpectatorCamera(0);
        }
    }

    public void DisableSpectatorCamera()
    {
        if (currentSpectator != null)
        {
            currentSpectator.GetComponentInChildren<CinemachineCamera>().enabled = false;
            currentSpectator = null;
        }
    }

    public void SetLobbyInfo(Lobby _lobby)
    {
        lobbyText.text = _lobby.GetData("LobbyName");
    }

    public string GetLobbyName()
    {
        return lobbyName.text;
    }

    public void UpdateLobbyPlayerList(Lobby _lobby)
    {
        lobbyPlayerList.UpdatePlayerList(_lobby);
    }

    public void ResetLobbyPlayerList()
    {
        lobbyPlayerList.ResetLobbyPlayerList();
    }
}
