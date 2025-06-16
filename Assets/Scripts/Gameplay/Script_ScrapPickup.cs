using Unity.VisualScripting;
using UnityEngine;

public class Script_ScrapPickup : MonoBehaviour
{
    [SerializeField] int scrapValue = 10;
    private bool hasGivenScrap = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "LocalPlayer" && !hasGivenScrap){
            hasGivenScrap = true;
            other.gameObject.GetComponent<Script_PlayerUpgrades>().AddScrap(scrapValue);
            Destroy(gameObject);
        }
    }
}
