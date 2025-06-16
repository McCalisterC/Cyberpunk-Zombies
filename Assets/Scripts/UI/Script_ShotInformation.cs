using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Script_ShotInformation : MonoBehaviour
{
    private string shotName;
    private string shotDescription;
    private Color color;
    private int cost;

    private float scalePercent;
    public float GetScalePercent() { return scalePercent; }
    private int initCost;
    private float currentScalePercent;
    public float GetCurrentScalePercent() { return currentScalePercent;  }
    private float costScale = 1;

    private void Start()
    {
        currentScalePercent = 1 + scalePercent;
    }

    public void SetInformation(string newName, string newDesc, Color newColor, int newCost, float newScalePercent){
        shotName = newName;
        shotDescription = newDesc;
        color = newColor;
        cost = newCost;
        initCost = newCost;
        scalePercent = newScalePercent;

        GetComponentInChildren<TMP_Text>().text = shotName + ": " + shotDescription + "\nCost: " + cost;
        GetComponentInChildren<Button>().gameObject.GetComponent<Image>().color = color;
    }

    public void CheckIfCanBuy(int currentPoints){
        if (currentPoints < cost){
            GetComponentInChildren<Button>().interactable = false;
        }
        else {
            GetComponentInChildren<Button>().interactable = true;
        }
    }

    public void ShotBought(){
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_PlayerUpgrades>().AddPointsRpc(-cost);
        currentScalePercent += scalePercent;
        IncreaseCost();
        UpdateInfo();
    }

    public void IncreaseCost(){
        costScale += 0.25f;
        cost = Convert.ToInt32(Math.Round(initCost * costScale));
    }

    public void UpdateInfo(){
        GetComponentInChildren<TMP_Text>().text = shotName + ": " + shotDescription + "\nCost: " + cost;
        GetComponentInChildren<Button>().gameObject.GetComponent<Image>().color = color;
    }
}
