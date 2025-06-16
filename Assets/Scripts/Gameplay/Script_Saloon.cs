using Unity.Netcode;
using UnityEngine;

public class Script_Saloon : NetworkBehaviour
{
    [SerializeField] GameObject saloonDoor;
    [SerializeField] GameObject kickoutPoint;
    private bool playerInSaloon = false;

    [SerializeField] Script_BarMenu barMenu;
    [SerializeField] Script_ScrapMenu scrapMenu;
    private Script_Bar bar;
    private Script_Mechanic mechanic;

    void Start()
    {
        bar = GetComponentInChildren<Script_Bar>();
        mechanic = GetComponentInChildren<Script_Mechanic>();
    }

    public void DoorToggle(bool open){
        if (open){
            saloonDoor.GetComponent<Animator>().SetTrigger("Open");
        }
        else
        {
            saloonDoor.GetComponent<Animator>().SetTrigger("Close");
        }
    }

    public void KickOutPlayer(){
        if (playerInSaloon){
            Debug.Log("Kicking player out");
            GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");
            player.GetComponent<CharacterController>().enabled = false;

            //Calculate kickout point depending on player clientid
            Vector3 newKickout = new Vector3(kickoutPoint.transform.position.x, kickoutPoint.transform.position.y, 
                kickoutPoint.transform.position.z + NetworkManager.LocalClientId);

            player.transform.position = newKickout;
            player.GetComponent<CharacterController>().enabled = true;
            playerInSaloon = false;
            bar.SetPlayerIsAtBar(false);
            bar.DisablePrompt();
            mechanic.SetPlayerIsAtMech(false);
            mechanic.DisablePrompt();
            if (barMenu.isActiveAndEnabled)
                barMenu.Close();
            if (scrapMenu.isActiveAndEnabled)
                scrapMenu.Close();
            scrapMenu.CloseReplacementMenu();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LocalPlayer"){
            playerInSaloon = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "LocalPlayer"){
            playerInSaloon = false;
        }
    }

    public void ResetScrapCost()
    {
        mechanic.ResetScrapCost();
    }
}
