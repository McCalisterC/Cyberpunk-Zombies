using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_BloodMoney : MonoBehaviour, I_Mods
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

    private int pointGain = 0;
    private int maxPoint = 0;

    private int currentRound = -1;
    private int pointGainedInRound = 0;

    private void Start()
    {
        method = delegate { BloodMoney(); };

        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                pointGain = 10;
                maxPoint = 200;
                break;
            case I_Mods.Rarity.Rare:
                pointGain = 25;
                maxPoint = 500;
                break;
            case I_Mods.Rarity.Epic:
                pointGain = 50;
                maxPoint = 1000;
                break;
            case I_Mods.Rarity.Legendary:
                pointGain = 200;
                maxPoint = 5000;
                break;
        }

        modName = "Blood Money";
        modDescription = "When taking damage, gain " + pointGain + " points! Can gain a total of " + maxPoint + " per round!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_BaseStats>().AddTakeDamageMethod(method);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_BaseStats>().RemoveTakeDamageMethod(method);
    }

    public void BloodMoney()
    {
        if (currentRound != GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().GetRound())
        {
            currentRound = GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().GetRound();
            pointGainedInRound = 0;
        }

        if (pointGainedInRound > maxPoint)
        {
            return;
        }

        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_PlayerUpgrades>().AddBonusPoints(pointGain);
        pointGainedInRound += pointGain;
    }
}
