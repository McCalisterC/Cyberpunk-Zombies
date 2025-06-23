// Assets/Scripts/Player/Basics/Shots/Script_PlayerShots.cs
using StarterAssets;
using UnityEngine;

public class Script_PlayerShots : MonoBehaviour
{
    [SerializeField] ScriptableObject_Shot[] shots;

    public ScriptableObject_Shot[] GetShots()
    {
        return shots;
    }

    public void Whiskey(float increaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>()
            .UpgradeDamage(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void Broth(float increaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>()
            .UpgradeHealth(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void Tap(float increaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>()
            .UpgradeReloadSpeed(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void Hops(float increaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<FirstPersonController>()
            .UpgradeSpeed(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void Vodka(float decreaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>()
            .UpgradeRegenTime(decreaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }

    public void IPA(float increaseValue, GameObject entry)
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>()
            .UpgradeFireRate(increaseValue);
        entry.GetComponent<Script_ShotInformation>().ShotBought();
    }
}