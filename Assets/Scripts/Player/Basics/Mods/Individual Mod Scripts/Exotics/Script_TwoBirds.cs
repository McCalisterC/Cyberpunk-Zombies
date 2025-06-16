using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_TwoBirds : MonoBehaviour, I_Mods
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

    private void Start()
    {
        method = delegate { TwoBirds(); };

        rarity = I_Mods.Rarity.EXOTIC;

        modName = "Two Birds";
        modDescription = "Each bullet now has a 50% chance to be regained when fired!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().AddShootMethod(method);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().RemoveShootMethod(method);
    }

    public void TwoBirds()
    {
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().currentAmmoAmount++;
        }
    }
}
