using Steamworks;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Script_ChatInput : NetworkBehaviour
{
    public bool deselecting = false;
    private Script_UIManager uIManager;
    [SerializeField] Animator animator;

    private void Start()
    {
        uIManager = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<Script_UIManager>();
    }
    public void Selected()
    {
        GetComponent<TMP_InputField>().onEndEdit.AddListener(OnEndEdit);
        animator.SetBool("InputEntered", true);
    }

    public void Deselected()
    {
        if (!deselecting)
        {
            deselecting = true;
            GetComponent<TMP_InputField>().onEndEdit.RemoveListener(OnEndEdit);

            if (!GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().GetDeathStatus())
                GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");

            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Input_Controller>().SetCursorState(true);
            animator.SetBool("InputEntered", false);
            deselecting = false;

            gameObject.SetActive(false);
        }
    }

    private void OnEndEdit(string inputString)
    {
        // Optional check if don't want users submitting an empty string.
        if (string.IsNullOrEmpty(inputString))
        {
            return;
        }

        // Checks that OnEndEdit was triggered by a Return/Enter key press this frame,
        // rather than just unfocusing (clicking off) the input field.
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (inputString.StartsWith("/addpoints"))
            {
                AddPointsCommand(inputString);
                return;
            }

            if (inputString.StartsWith("/addscrap"))
            {
                AddScrapCommand(inputString);
                return;
            }

            if (inputString.StartsWith("/nextround"))
            {
                SetNextRoundCommand(inputString);
                return;
            }

            if (inputString.StartsWith("/godmode"))
            {
                GodModeCommand(inputString);
                return;
            }

            // NEW: Added check for the /give_mod command to integrate the new functionality.
            // This follows the pattern of existing command checks.
            if (inputString.StartsWith("/give_mod"))
            {
                GiveModCommand(inputString);
                return;
            }

            UpdateChatRpc(SteamClient.Name, inputString);
            GetComponent<TMP_InputField>().text = "";

            EventSystem.current.SetSelectedGameObject(null);
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateChatRpc(string name, string inputString)
    {
        GameObject.FindGameObjectWithTag("Chat Text").GetComponent<TMP_Text>().text += name + ": " + inputString + "\n";
        animator.SetTrigger("ChatRecieved");
    }

    public void AddPointsCommand(string inputString)
    {
        // Regular expression to match any character that is not a digit (0-9)
        string pattern = "[^0-9]";
        // Replace all occurrences of the matched characters with an empty string.
        string points = Regex.Replace(inputString, pattern, "");
        int pointsToAdd = int.Parse(points);

        GetComponent<TMP_InputField>().text = "";

        GameObject.FindGameObjectWithTag("Chat Text").GetComponent<TMP_Text>().text += "*ADDED " + pointsToAdd + " POINTS!*" + "\n";
        animator.SetTrigger("ChatRecieved");

        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_PlayerUpgrades>().AddBonusPoints(pointsToAdd);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void AddScrapCommand(string inputString)
    {
        // Regular expression to match any character that is not a digit (0-9)
        string pattern = "[^0-9]";
        // Replace all occurrences of the matched characters with an empty string.
        string scrap = Regex.Replace(inputString, pattern, "");
        int scrapToAdd = int.Parse(scrap);

        GetComponent<TMP_InputField>().text = "";

        GameObject.FindGameObjectWithTag("Chat Text").GetComponent<TMP_Text>().text += "*ADDED " + scrapToAdd + " SCRAP!*" + "\n";
        animator.SetTrigger("ChatRecieved");

        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_PlayerUpgrades>().AddBonusScrap(scrapToAdd);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetNextRoundCommand(string inputString)
    {
        // Regular expression to match any character that is not a digit (0-9)
        string pattern = "[^0-9]";
        // Replace all occurrences of the matched characters with an empty string.
        string nextRound = Regex.Replace(inputString, pattern, "");
        int nextRoundInt = int.Parse(nextRound);

        GetComponent<TMP_InputField>().text = "";

        if (nextRoundInt > 0 && NetworkManager.Singleton.IsServer)
        {
            GameObject.FindGameObjectWithTag("Chat Text").GetComponent<TMP_Text>().text += "*NEXT WAVE WILL START AT " + nextRoundInt + "*" + "\n";
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().DebugNextRoundRpc(nextRoundInt);
            animator.SetTrigger("ChatRecieved");
        }
        else
        {
            if (nextRoundInt <= 0)
            {
                GameObject.FindGameObjectWithTag("Chat Text").GetComponent<TMP_Text>().text += "/nextround requires a positive integer greater than 0!" + "\n";
            }
            else
            {
                GameObject.FindGameObjectWithTag("Chat Text").GetComponent<TMP_Text>().text += "/nextround can only be performed by the host!" + "\n";
            }
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void GodModeCommand(string inputString)
    {
        GetComponent<TMP_InputField>().text = "";

        bool godModeValue = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().ToggleGodMode();

        string godModeString = godModeValue ? "ENABLED" : "DISABLED";

        GameObject.FindGameObjectWithTag("Chat Text").GetComponent<TMP_Text>().text += "*GOD MODE " + godModeString + "*" + "\n";
        animator.SetTrigger("ChatRecieved");
    }

    // NEW: Method to handle the /give_mod command. This parses the input, validates the mod and rarity,
    // finds the mod from Script_ScrapMenu, and adds/replaces it while respecting the 5-mod limit.
    // It uses existing Script_ScrapMenu public methods and properties for integration.
    // Errors or success messages are sent to the chat using existing logic.
    private void GiveModCommand(string inputString)
    {
        // Parse the command (e.g., "/give_mod LessIsMore Rare").
        string[] parts = inputString.Split(' ');
        if (parts.Length < 3)
        {
            // Display error in chat if input is malformed.
            UpdateChatRpc("", "*ERROR: Invalid format. Use /give_mod <modName> <modRarity>*");
            return;
        }

        string modName = parts[1].Trim();
        string rarityString = parts[2].Trim();

        // Validate and parse rarity to I_Mods.Rarity enum (case-insensitive).
        if (!System.Enum.TryParse<I_Mods.Rarity>(rarityString, true, out I_Mods.Rarity parsedRarity))
        {
            UpdateChatRpc("", $"*ERROR: Invalid rarity '{rarityString}'. Valid: Common, Rare, Epic, Legendary, Exotic.*");
            return;
        }

        // Get Script_ScrapMenu instance using Script_Mechanic's public method.
        Script_Mechanic mechanic = GameObject.FindAnyObjectByType<Script_Mechanic>();
        if (mechanic == null)
        {
            UpdateChatRpc("", "*ERROR: Could not access mod system.*");
            return;
        }
        Script_ScrapMenu scrapMenu = mechanic.GetScrapHandler();
        if (scrapMenu == null)
        {
            UpdateChatRpc("", "*ERROR: Could not access mod system.*");
            return;
        }

        // Find the mod in the available mods list that matches both name and rarity.
        // This enforces rules: Normal mods match Common-Legendary; Exotic mods only match Exotic.
        I_Mods foundMod = scrapMenu.GetMods().FirstOrDefault(mod => mod.modName.Replace(" ", string.Empty) == modName && mod.rarity == parsedRarity);
        if (foundMod == null)
        {
            UpdateChatRpc("", $"*ERROR: Mod '{modName}' with rarity '{rarityString}' not found or invalid.*");
            return;
        }

        // Determine color based on rarity using Script_ScrapMenu's serialized colors.
        Color modColor = Color.black;
        switch (parsedRarity)
        {
            case I_Mods.Rarity.Common:
                modColor = scrapMenu.GetCommonColor();
                break;
            case I_Mods.Rarity.Rare:
                modColor = scrapMenu.GetRareColor();
                break;
            case I_Mods.Rarity.Epic:
                modColor = scrapMenu.GetEpicColor();
                break;
            case I_Mods.Rarity.Legendary:
                modColor = scrapMenu.GetLegendaryColor();
                break;
            case I_Mods.Rarity.EXOTIC:
                modColor = scrapMenu.GetExoticColor();
                break;
        }

        // Get active mods and icons for modification.
        var activeMods = scrapMenu.GetActiveMods();
        var modIcons = scrapMenu.modIcons;

        // Handle adding or replacing the mod (mirroring AddMod/ReplaceMod logic from Script_ScrapMenu).
        if (activeMods.Count < 5)
        {
            // Add the mod if under limit.
            activeMods.Add(foundMod);
            GameObject modIcon = Object.Instantiate(scrapMenu.GetModIconContentPrefab(), scrapMenu.GetModIconContentHolder().transform);
            modIcon.GetComponentInChildren<Outline>().gameObject.GetComponent<Image>().sprite = foundMod.modIcon;
            modIcon.GetComponent<Image>().color = modColor;
            modIcons.Add(modIcon);
            foundMod.Activate();
        }
        else
        {
            // Replace the last mod if at limit.
            int lastIndex = activeMods.Count - 1;
            I_Mods modToRemove = activeMods[lastIndex];
            GameObject iconToRemove = modIcons[lastIndex];

            modToRemove.Deactivate();
            activeMods.RemoveAt(lastIndex);
            modIcons.RemoveAt(lastIndex);
            Object.Destroy(iconToRemove);

            // Now add the new mod (same as above).
            activeMods.Add(foundMod);
            GameObject modIcon = Object.Instantiate(scrapMenu.GetModIconContentPrefab(), scrapMenu.GetModIconContentHolder().transform);
            modIcon.GetComponentInChildren<Outline>().gameObject.GetComponent<Image>().sprite = foundMod.modIcon;
            modIcon.GetComponent<Image>().color = modColor;
            modIcons.Add(modIcon);
            foundMod.Activate();
        }

        // Play SFX, clear input, and show success message in chat (consistent with other commands).
        mechanic.buySFX.Play();
        GetComponent<TMP_InputField>().text = "";
        UpdateChatRpc("", $"*ADDED MOD: {modName} ({rarityString})*");
        animator.SetTrigger("ChatRecieved");
        EventSystem.current.SetSelectedGameObject(null);
    }
}
