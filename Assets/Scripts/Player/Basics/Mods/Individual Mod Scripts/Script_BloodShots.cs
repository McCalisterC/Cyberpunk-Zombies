// Assets\Scripts\Player\Basics\Mods\Individual Mod Scripts\Script_BloodShots.cs
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
    private float maxShieldAmountPercentage = 0;

    private void Start()
    {
        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                percentage = 0.05f;
                maxShieldAmountPercentage = 0.2f;
                break;
            case I_Mods.Rarity.Rare:
                percentage = 0.15f;
                maxShieldAmountPercentage = 0.4f;
                break;
            case I_Mods.Rarity.Epic:
                percentage = 0.25f;
                maxShieldAmountPercentage = 0.7f;
                break;
            case I_Mods.Rarity.Legendary:
                percentage = 0.50f;
                maxShieldAmountPercentage = 1f;
                break;
        }

        modName = "Blood Shots";
        modDescription = "Dealing damage will heal you " + (percentage * 100) + "% of the damage dealt! " +
                        "Excess healing creates a temporary shield (max " + (maxShieldAmountPercentage * 100) + "% of your health)!";
    }

    public void Activate()
    {
        Script_UIManager.Instance.shieldBar.gameObject.SetActive(true);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().SetBloodShots(percentage);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().SetBloodShotsShield(maxShieldAmountPercentage);
    }

    public void Deactivate()
    {
        Script_UIManager.Instance.shieldBar.gameObject.SetActive(false);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>().SetBloodShots(0);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().SetBloodShotsShield(0);
    }
}