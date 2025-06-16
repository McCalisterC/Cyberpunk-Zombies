using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Script_ModInformation : MonoBehaviour
{
    private string modName;
    private string modDescription;
    private Color color;
    private Sprite modIcon;

    public void SetInformation(string newName, string newDesc, Color newColor, Sprite newModIcon){
        modName = newName;
        modDescription = newDesc;
        color = newColor;
        modIcon = newModIcon;

        GetComponentInChildren<TMP_Text>().text = "<uppercase><b>" + modName + "</b></uppercase>\n" + modDescription;
        GetComponentInChildren<Button>().gameObject.GetComponent<Image>().color = color;
        GetComponentInChildren<Shadow>().gameObject.GetComponent<Image>().color = color;
        GetComponentInChildren<Outline>().gameObject.GetComponent<Image>().sprite = modIcon;
    }
}
