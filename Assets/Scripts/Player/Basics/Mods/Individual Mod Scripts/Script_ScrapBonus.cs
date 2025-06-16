using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_ScrapBonus : MonoBehaviour, I_Mods
{
    public I_Mods.Rarity rarity { get => _rarity; set => _rarity = value; }
    public string modName { get => _modName; set => _modName = value; }
    public string modDescription { get => _modDescription; set => _modDescription = value; }
    public Sprite modIcon { get => _modIcon; set => _modIcon = value; }

    [SerializeField] Sprite _modIcon;
    string _modName;
    string _modDescription;
    [SerializeField] I_Mods.Rarity _rarity;

    int bonus = 0;
    Action method;

    private void Start()
    {
        method = delegate { BonusScrap(); };

        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                bonus = 2;
                break;
            case I_Mods.Rarity.Rare:
                bonus = 5;
                break;
            case I_Mods.Rarity.Epic:
                bonus = 10;
                break;
            case I_Mods.Rarity.Legendary:
                bonus = 20;
                break;
        }

        modName = "Scrappy";
        modDescription = "On scrap pickup, gain a extra " + bonus + " scrap!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_PlayerUpgrades>().AddScrapMethod(method);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_PlayerUpgrades>().RemoveScrapMethod(method);
    }

    public void BonusScrap()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_PlayerUpgrades>().AddBonusScrap(bonus);
    }
}
