using UnityEngine;
using UnityEngine.UI;

public class Script_BloodShots : MonoBehaviour, I_Mods
{
    public I_Mods.Rarity rarity { get => _rarity; set => _rarity = value; }
    public string modName { get => _modName; set => _modName = value; }
    public string modDescription { get => _modDescription; set => _modDescription = value; }
    public Sprite modIcon { get => _modIcon; set => _modIcon = value; }

    [SerializeField] Sprite _modIcon;
    string _modName;
    string _modDescription;
    [SerializeField] I_Mods.Rarity _rarity;

    private float percentage = 0;

    private void Start()
    {

        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                percentage = 0.05f;
                break;
            case I_Mods.Rarity.Rare:
                percentage = 0.15f;
                break;
            case I_Mods.Rarity.Epic:
                percentage = 0.25f;
                break;
            case I_Mods.Rarity.Legendary:
                percentage = 0.50f;
                break;
        }

        modName = "Blood Shots";
        modDescription = "Dealing damage will heal you " + (percentage * 100) + "% of the damage dealt!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().SetBloodShots(percentage);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>().SetBloodShots(0);
    }
}
