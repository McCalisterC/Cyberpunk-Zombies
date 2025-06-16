using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Script_ScrapMenu : MonoBehaviour
{
    [Header("Mods")]
    [SerializeField] GameObject modHolder;
    [SerializeField] GameObject mechanic;
    [SerializeField] GameObject contentPrefab;
    [SerializeField] GameObject modIconContentHolder;
    [SerializeField] GameObject modIconContentPrefab;
    [SerializeField] GameObject replacementContentHolder;
    [SerializeField] Script_ModReplacementMenu modReplacementMenu;

    [Header("Mod Colors")]
    [SerializeField] Color commonColor;
    [SerializeField] Color rareColor;
    [SerializeField] Color epicColor;
    [SerializeField] Color legendaryColor;
    [SerializeField] Color exoticColor;
    public bool inMenu = false;

    private List<I_Mods> mods = new List<I_Mods>();
    private List<GameObject> entries = new List<GameObject>();
    private List<I_Mods> activeMods = new List<I_Mods>();
    public List<GameObject> modIcons = new List<GameObject>();
    private int modLimit = 5;
    public List<I_Mods> GetMods() { return mods; }
    public void SetMods(List<I_Mods> value) { mods = value; }
    public void SetActiveMods(List<I_Mods> value) { activeMods = value; }
    public List<I_Mods> GetActiveMods() { return activeMods; }

    // List for each rarity
    private List<I_Mods> commonMods = new List<I_Mods>();
    private List<I_Mods> rareMods = new List<I_Mods>();
    private List<I_Mods> epicMods = new List<I_Mods>();
    private List<I_Mods> legendaryMods = new List<I_Mods>();
    private List<I_Mods> exoticMods = new List<I_Mods>();

    // 2D Array for storing percent chance combinations
    [System.Serializable] public class MultiDimFloatArr { public float[] floatArr; }
    public MultiDimFloatArr[] modChances;


    private void Start()
    {
        mods = modHolder.GetComponentsInChildren<I_Mods>().ToList();
        mechanic.GetComponent<Script_Mechanic>().CloseMenu();

        foreach (I_Mods mod in mods)
        {
            switch (mod.rarity)
            {
                case I_Mods.Rarity.Common:
                    commonMods.Add(mod);
                    break;
                case I_Mods.Rarity.Rare:
                    rareMods.Add(mod);
                    break;
                case I_Mods.Rarity.Epic:
                    epicMods.Add(mod);
                    break;
                case I_Mods.Rarity.Legendary:
                    legendaryMods.Add(mod);
                    break;
                case I_Mods.Rarity.EXOTIC:
                    exoticMods.Add(mod);
                    break;
            }
        }
    }

    public void InitMenu()
    {
        inMenu = true;
        Populate();
    }

    private void Populate()
    {
        List<I_Mods> modsThatCanBeSelected = mods.Except(activeMods).ToList();

        int roundScale = GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().GetRound() / 5;

        if (roundScale > modChances.Length)
        {
            roundScale = modChances.Length;
        }

        float[] currentModChances = modChances[roundScale].floatArr;

        for (int i = 0; i < 3; i++)
        {
            if (modsThatCanBeSelected.Count <= 0)
            {
                return;
            }

            List<I_Mods> activeSet = null;

            float randTemp = Random.Range(0, 100);

            for (int j = 0; j < currentModChances.Length; j++)
            {
                randTemp -= currentModChances[j];

                if (randTemp < 0)
                {
                    switch (j)
                    {
                        case 0:
                            activeSet = commonMods;
                            break;
                        case 1:
                            activeSet = rareMods;
                            break;
                        case 2:
                            activeSet = epicMods;
                            break;
                        case 3:
                            activeSet = legendaryMods;
                            break;
                        case 4:
                            activeSet = exoticMods;
                            break;
                        default:
                            activeSet = commonMods;
                            break;
                    }
                    break;
                }
            }

            List<I_Mods> modsSelectableThisRound = modsThatCanBeSelected.Intersect(activeSet).ToList();

            if (modsSelectableThisRound.Count == 0)
            {
                modsSelectableThisRound = modsThatCanBeSelected;
            }

            I_Mods modSelected = modsSelectableThisRound[new System.Random().Next(modsSelectableThisRound.Count)];
            modsThatCanBeSelected.Remove(modSelected);

            Color modColor = Color.black;

            switch (modSelected.rarity)
            {
                case I_Mods.Rarity.Common:
                    modColor = commonColor;
                    break;
                case I_Mods.Rarity.Rare:
                    modColor = rareColor;
                    break;
                case I_Mods.Rarity.Epic:
                    modColor = epicColor;
                    break;
                case I_Mods.Rarity.Legendary:
                    modColor = legendaryColor;
                    break;
                case I_Mods.Rarity.EXOTIC:
                    modColor = exoticColor;
                    break;
            }

            GameObject newContent = Instantiate(contentPrefab, transform);

            newContent.GetComponent<Script_ModInformation>().SetInformation(modSelected.modName,
                modSelected.modDescription, modColor, modSelected.modIcon);

            bool limitReached = (activeMods.Count >= modLimit);

            if (limitReached)
            {
                newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { StartReplaceMod(modSelected, modColor); });
            }

            else
            {
                newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { AddMod(modSelected, modColor); });
            }

            entries.Add(newContent);
        }
    }

    private void StartReplaceMod(I_Mods modSelected, Color selectedModColor)
    {
        replacementContentHolder.SetActive(true);

        for (int i = 0; i < 5; i++)
        {
            Color modColor = Color.black;

            switch (activeMods[i].rarity)
            {
                case I_Mods.Rarity.Common:
                    modColor = commonColor;
                    break;
                case I_Mods.Rarity.Rare:
                    modColor = rareColor;
                    break;
                case I_Mods.Rarity.Epic:
                    modColor = epicColor;
                    break;
                case I_Mods.Rarity.Legendary:
                    modColor = legendaryColor;
                    break;
                case I_Mods.Rarity.EXOTIC:
                    modColor = exoticColor;
                    break;
            }

            modReplacementMenu.modContent[i].GetComponentInChildren<Shadow>().gameObject.GetComponent<Image>().color = modColor;
            modReplacementMenu.modContent[i].GetComponentInChildren<Outline>().gameObject.GetComponent<Image>().sprite = activeMods[i].modIcon;
            modReplacementMenu.modContent[i].GetComponentInChildren<Button>().gameObject.GetComponent<Image>().color = modColor;
            modReplacementMenu.modContent[i].GetComponentInChildren<TMP_Text>().text = "<uppercase><b>" + activeMods[i].modName + "</b></uppercase>\n" + activeMods[i].modDescription;

            int modIndex = i;

            modReplacementMenu.modContent[i].GetComponentInChildren<Button>().onClick.RemoveAllListeners();

            modReplacementMenu.modContent[i].GetComponentInChildren<Button>().onClick.AddListener(delegate { ReplaceMod(modIndex, modSelected, selectedModColor); });
        }
    }

    private void ReplaceMod(int activeModIndex, I_Mods selectedMod, Color modColor)
    {
        replacementContentHolder.SetActive(false);

        GameObject modIcon = modIcons[activeModIndex];

        modIcons.Remove(modIcon);
        Destroy(modIcon);
        activeMods[activeModIndex].Deactivate();
        activeMods.RemoveAt(activeModIndex);

        AddMod(selectedMod, modColor);
    }

    private void AddMod(I_Mods modToAdd, Color modColor)
    {
        activeMods.Add(modToAdd);
        GameObject modIcon = Instantiate(modIconContentPrefab, modIconContentHolder.transform);
        modIcon.GetComponentInChildren<Outline>().gameObject.GetComponent<Image>().sprite = modToAdd.modIcon;
        modIcon.GetComponent<Image>().color = modColor;
        modIcons.Add(modIcon);
        modToAdd.Activate();
        mechanic.GetComponent<Script_Mechanic>().buySFX.Play();

        Close();
    }

    public void Close()
    {
        foreach (GameObject entry in entries)
        {
            Destroy(entry);
        }
        entries.Clear();

        gameObject.GetComponentInParent<Image>().gameObject.SetActive(false);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_OtherControls>().ToggleCursor();
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_OtherControls>().ToggleInput(true);
        mechanic.GetComponent<Script_Mechanic>().ReactivatePrompt();
        inMenu = false;
    }

    public void CloseReplacementMenu()
    {
        replacementContentHolder.SetActive(false);
    }
}
