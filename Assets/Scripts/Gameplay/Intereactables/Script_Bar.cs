using TMPro;
using UnityEngine;

public class Script_Bar : MonoBehaviour, I_Interactable
{
    [Header("UI")]
    [SerializeField] TMP_Text prompt;
    [SerializeField] GameObject scrollViewUI;

    private bool playerIsAtBar = false;
    public void SetPlayerIsAtBar(bool value) { playerIsAtBar = value; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "LocalPlayer")
        {
            playerIsAtBar = true;
            other.GetComponent<Script_OtherControls>().currentInteractable = this;
            prompt.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "LocalPlayer")
        {
            playerIsAtBar = false;
            other.GetComponent<Script_OtherControls>().currentInteractable = null;
            prompt.enabled = false;
        }
    }

    public void OnInteract()
    {
        if (playerIsAtBar)
        {
            GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");
            Debug.Log("Open bar menu");
            scrollViewUI.SetActive(true);
            prompt.enabled = false;
            player.GetComponent<Script_OtherControls>().ToggleCursor();
            player.GetComponent<Script_OtherControls>().ToggleInput(false);
            scrollViewUI.GetComponentInChildren<Script_BarMenu>().inMenu = true;
            scrollViewUI.GetComponentInChildren<Script_BarMenu>().CheckPrices();
        }
    }

    public void ReactivatePrompt()
    {
        if (playerIsAtBar)
        {
            prompt.enabled = true;
        }
    }

    public void CloseMenu(){
        scrollViewUI.SetActive(false);
    }

    public void DisablePrompt()
    {
        prompt.enabled = false;
    }
}
