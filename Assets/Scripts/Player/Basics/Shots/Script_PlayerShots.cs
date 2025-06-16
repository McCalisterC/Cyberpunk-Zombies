using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;

public class Script_PlayerShots : MonoBehaviour
{
    [SerializeField] ScriptableObject_Shot[] shots;

    public ScriptableObject_Shot[] GetShots(){
        return shots;
    }

    public void Whiskey(float increaseValue, GameObject entry){
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().UpgradeDamage(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void Broth(float increaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().UpgradeHealth(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void Tap(float increaseValue, GameObject entry) 
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().UpgradeReloadSpeed(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void Hops(float increaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<FirstPersonController>().UpgradeSpeed(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void Vodka(float decreaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().UpgradeRegenTime(decreaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void IPA(float increaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().UpgradeFireRate(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }
}
