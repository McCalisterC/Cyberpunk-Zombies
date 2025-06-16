using Steamworks;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
}
