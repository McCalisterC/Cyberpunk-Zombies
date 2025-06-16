using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_LessIsMore : MonoBehaviour, I_Mods, I_Mods_DamageBoost
{
    public I_Mods.Rarity rarity { get => _rarity; set => _rarity = value; }
    public string modName { get => _modName; set => _modName = value; }
    public string modDescription { get => _modDescription; set => _modDescription = value; }
    public float currentBonus { get => _currentBonus; set => _currentBonus = value; }
    public Sprite modIcon { get => _modIcon; set => _modIcon = value; }

    [SerializeField] Sprite _modIcon;
    string _modName;
    string _modDescription;
    [SerializeField] I_Mods.Rarity _rarity;
    Action method;
    float _currentBonus = 0;

    private float percentage = 0;
    private string descriptor = "";

    private void Start()
    {
        method = delegate { LessIsMore(); };

        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                percentage = 0.05f;
                descriptor = "small";
                break;
            case I_Mods.Rarity.Rare:
                percentage = 0.15f;
                descriptor = "medium";
                break;
            case I_Mods.Rarity.Epic:
                percentage = 0.25f;
                descriptor = "large";
                break;
            case I_Mods.Rarity.Legendary:
                percentage = 0.50f;
                descriptor = "EXTREME";
                break;
        }

        modName = "Less Is More";
        modDescription = "Gain a " + descriptor + " damage bonus for each bullet missing in the current magazine!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().AddShootMethod(method);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().RemoveShootMethod(method);
    }

    public void LessIsMore()
    {
        Pistol weapon = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>();

        currentBonus = weapon.GetCurrentNextShotDamage() * percentage * (weapon.clipSize - weapon.currentAmmoAmount);

        weapon.BoostDamage(currentBonus);
    }
}
