using StarterAssets;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Script_KillSpeedBurst : MonoBehaviour, I_Mods
{
    public I_Mods.Rarity rarity { get => _rarity; set => _rarity = value; }
    public string modName { get => _modName; set => _modName = value; }
    public string modDescription { get => _modDescription; set => _modDescription = value; }
    public Sprite modIcon { get => _modIcon; set => _modIcon = value; }

    [SerializeField] Sprite _modIcon;
    string _modName;
    string _modDescription;
    [SerializeField] I_Mods.Rarity _rarity;
    Action method;

    float speedBonus = 0;
    float bonusTime = 0;
    string descriptor = "";
    float currentBonus = 0;

    private void Start()
    {
        method = delegate { KillSpeed(); };

        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                speedBonus = 1;
                bonusTime = 0.2f;
                descriptor = "small";
                break;
            case I_Mods.Rarity.Rare:
                speedBonus = 2;
                bonusTime = 0.4f;
                descriptor = "medium";
                break;
            case I_Mods.Rarity.Epic:
                speedBonus = 3;
                bonusTime = 0.6f;
                descriptor = "large";
                break;
            case I_Mods.Rarity.Legendary:
                speedBonus = 6;
                bonusTime = 1f;
                descriptor = "EXTREME";
                break;
        }

        modName = "Run And Gun";
        modDescription = "On kill, gain a " + descriptor + " burst to speed decaying over " + bonusTime + " seconds!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_PlayerUpgrades>().AddKillMethod(method);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_PlayerUpgrades>().RemoveKillMethod(method);
    }

    public void KillSpeed()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<FirstPersonController>().AddSpeed(speedBonus);
        currentBonus = speedBonus;
        StartCoroutine(SpeedDecay(Time.time));
    }

    IEnumerator SpeedDecay(float time)
    {
        if ((Time.time - time) > bonusTime)
        {
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<FirstPersonController>().RemoveSpeed(currentBonus);
            currentBonus = 0;
            yield break;
        }
        else
        {
            float percentageDecrease = (Time.time - time) / bonusTime;
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<FirstPersonController>().RemoveSpeed(currentBonus);
            currentBonus = speedBonus - (speedBonus * percentageDecrease);
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<FirstPersonController>().AddSpeed(currentBonus);
            yield return new WaitForEndOfFrame();
            StartCoroutine(SpeedDecay(time));
        }
    }
}
