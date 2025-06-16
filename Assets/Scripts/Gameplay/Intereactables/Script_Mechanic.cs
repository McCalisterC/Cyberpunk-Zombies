using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Script_Mechanic : MonoBehaviour, I_Interactable
{
    [Header("Requirements")]
    [SerializeField] int initScrapCost = 10;
    [SerializeField] int currentScrapCost = 10;

    [Header("UI")]
    [SerializeField] TMP_Text prompt;
    [SerializeField] GameObject contentUI;

    [Header("Audio")]
    [SerializeField] public AudioSource buySFX;

    private BoxCollider menuTrigger;

    private bool playerIsAtMech = false;
    public void SetPlayerIsAtMech(bool value) { playerIsAtMech = value; }

    private void Start()
    {
        menuTrigger = gameObject.AddComponent<BoxCollider>();
        prompt.text = "Press E to open scrap menu [Cost: " + currentScrapCost + "]";

        menuTrigger.size = new Vector3(1.5f, 1.5f, 1.5f);
        menuTrigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LocalPlayer")
        {
            if (!GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().isTransitioning)
            {
                prompt.text = "Kill all the zombies first!";
                prompt.enabled = true;
                return;
            }
            prompt.text = "Press E to open scrap menu [Cost: " + currentScrapCost + "]";
            playerIsAtMech = true;
            other.GetComponent<Script_OtherControls>().currentInteractable = this;
            prompt.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "LocalPlayer")
        {
            playerIsAtMech = false;
            other.GetComponent<Script_OtherControls>().currentInteractable = null;
            prompt.enabled = false;
        }
    }

    public void OnInteract()
    {
        GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");

        if (playerIsAtMech && GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().isTransitioning)
        {
            if (player.GetComponent<Script_PlayerUpgrades>().GetScrap() < currentScrapCost)
            {
                Debug.Log("Not enough scrap to open menu");
                return;
            }

            player.GetComponent<Script_PlayerUpgrades>().RemoveScrap(currentScrapCost);
            currentScrapCost *= 2;
            prompt.text = "Press E to open scrap menu [Cost: " + currentScrapCost + "]";
            Debug.Log("Open scrap menu");
            contentUI.SetActive(true);
            prompt.enabled = false;
            player.GetComponent<Script_OtherControls>().ToggleCursor();
            player.GetComponent<Script_OtherControls>().ToggleInput(false);
            contentUI.GetComponentInChildren<Script_ScrapMenu>().InitMenu();
        }
    }

    public void ReactivatePrompt()
    {
        if (playerIsAtMech)
        {
            if (!GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().isTransitioning)
            {
                prompt.text = "Kill all the zombies first!";
                prompt.enabled = true;
                return;
            }

            prompt.text = "Press E to open scrap menu [Cost: " + currentScrapCost + "]";
            prompt.enabled = true;
        }
    }

    public void CloseMenu(){
        contentUI.SetActive(false);
    }

    public void DisablePrompt()
    {
        prompt.enabled = false;
    }

    public Script_ScrapMenu GetScrapHandler()
    {
        return contentUI.GetComponentInChildren<Script_ScrapMenu>();
    }

    public void ResetScrapCost()
    {
        currentScrapCost = initScrapCost;
        if (!GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().isTransitioning)
        {
            prompt.text = "Kill all the zombies first!";
            return;
        }
        prompt.text = "Press E to open scrap menu [Cost: " + currentScrapCost + "]";
    }
}
