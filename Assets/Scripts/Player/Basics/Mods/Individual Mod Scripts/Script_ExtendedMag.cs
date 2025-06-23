using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_ExtendedMag : MonoBehaviour, I_Mods
{
    public I_Mods.Rarity rarity { get => _rarity; set => _rarity = value; }
    public string modName { get => _modName; set => _modName = value; }
    public string modDescription { get => _modDescription; set => _modDescription = value; }
    public Sprite modIcon { get => _modIcon; set => _modIcon = value; }

    [SerializeField] Sprite _modIcon;
    string _modName;
    string _modDescription;
    [SerializeField] I_Mods.Rarity _rarity;

    private int sizeIncrease;

    private void Start()
    {
        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                modDescription = "Increases Clip Size by a small amount!";
                break;
            case I_Mods.Rarity.Rare:
                modDescription = "Increases Clip Size by a medium amount!";
                break;
            case I_Mods.Rarity.Epic:
                modDescription = "Increases Clip Size by a large amount!";
                break;
            case I_Mods.Rarity.Legendary:
                modDescription = "Increases Clip Size by a MASSIVE amount!";
                break;
        }

        modName = "Extended Mag";
    }

    public void Activate()
    {
        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                sizeIncrease = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().clipSize / 3;
                break;
            case I_Mods.Rarity.Rare:
                sizeIncrease = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().clipSize / 2;
                break;
            case I_Mods.Rarity.Epic:
                sizeIncrease = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().clipSize;
                break;
            case I_Mods.Rarity.Legendary:
                sizeIncrease = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().clipSize + GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().clipSize / 3;
                break;
        }

        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().clipSize += sizeIncrease;
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().clipSize -= sizeIncrease;
    }
}
