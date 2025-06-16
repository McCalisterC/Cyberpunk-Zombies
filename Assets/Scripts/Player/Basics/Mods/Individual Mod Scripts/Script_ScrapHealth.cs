using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_ScrapHealth : MonoBehaviour, I_Mods
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

    int health = 0;

    private void Start()
    {
        method = delegate { HealthBonus(); };

        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                health = 5;
                break;
            case I_Mods.Rarity.Rare:
                health = 15;
                break;
            case I_Mods.Rarity.Epic:
                health = 25;
                break;
            case I_Mods.Rarity.Legendary:
                health = 50;
                break;
        }

        modName = "Recycled Parts";
        modDescription = "On scrap pickup, recover " + health + " health!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_PlayerUpgrades>().AddScrapMethod(method);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_PlayerUpgrades>().RemoveScrapMethod(method);
    }

    public void HealthBonus()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().AddHealth(health);
    }
}
