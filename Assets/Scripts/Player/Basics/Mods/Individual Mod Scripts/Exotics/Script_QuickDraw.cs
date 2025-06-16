using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_QuickDraw : MonoBehaviour, I_Mods
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
        method = delegate { QuickDraw(); };

        rarity = I_Mods.Rarity.EXOTIC;

        modName = "Quick Draw";
        modDescription = "The first bullet of every clip does 300% more damage!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().AddShootMethod(method);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().RemoveShootMethod(method);
    }

    public void QuickDraw()
    {
        Pistol pistol = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>();

        if (pistol.currentAmmoAmount == pistol.clipSize)
        {
            pistol.BoostDamage(pistol.GetCurrentNextShotDamage() * 3);
        }
    }
}
