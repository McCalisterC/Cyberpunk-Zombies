using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_DeathTax : MonoBehaviour, I_Mods
{
    public I_Mods.Rarity rarity { get => _rarity; set => _rarity = value; }
    public string modName { get => _modName; set => _modName = value; }
    public string modDescription { get => _modDescription; set => _modDescription = value; }
    public Sprite modIcon { get => _modIcon; set => _modIcon = value; }

    [SerializeField] Sprite _modIcon;
    string _modName;
    string _modDescription;
    [SerializeField] I_Mods.Rarity _rarity;

    private void Start()
    {
        rarity = I_Mods.Rarity.EXOTIC;

        modName = "Death Tax";
        modDescription = "When you would die, instead destroy this mod and recover to 10 health and become invulernable for 2 seconds!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().DeathTax = true;
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().DeathTax = false;
    }
}
