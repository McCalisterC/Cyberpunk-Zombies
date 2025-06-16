using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Script_BarMenu : MonoBehaviour
{
    [SerializeField] GameObject bar;
    [SerializeField] GameObject contentPrefab;
    public bool inMenu = false;
    private Script_PlayerShots shots;

    [Header("Audio")]
    [SerializeField] AudioSource buySFX;

    private List<GameObject> entires = new List<GameObject>();

    private void Start()
    {
        shots = bar.GetComponent<Script_PlayerShots>();
        Populate();
    }

    private void Populate()
    {
        for (int i = 0; i < shots.GetShots().Length; i++)
        {
            GameObject newContent = Instantiate(contentPrefab, transform);
            entires.Add(newContent);

            newContent.GetComponent<Script_ShotInformation>().SetInformation(shots.GetShots()[i].shotPublicName, 
                shots.GetShots()[i].shotDescription, shots.GetShots()[i].shotColor, shots.GetShots()[i].cost, shots.GetShots()[i].scalePercentOrValue);

            switch (shots.GetShots()[i].shotInternalName){
                case "Whiskey":
                    newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { bar.GetComponent<Script_PlayerShots>().Whiskey(newContent.GetComponent<Script_ShotInformation>().GetCurrentScalePercent(), newContent); });
                    break;
                case "Broth":
                    newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { bar.GetComponent<Script_PlayerShots>().Broth(newContent.GetComponent<Script_ShotInformation>().GetScalePercent(), newContent); });
                    break;
                case "Tap":
                    newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { bar.GetComponent<Script_PlayerShots>().Tap(newContent.GetComponent<Script_ShotInformation>().GetScalePercent(), newContent); });
                    break;
                case "Hops":
                    newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { bar.GetComponent<Script_PlayerShots>().Hops(newContent.GetComponent<Script_ShotInformation>().GetCurrentScalePercent(), newContent); });
                    break;
                case "Vodka":
                    newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { bar.GetComponent<Script_PlayerShots>().Vodka(newContent.GetComponent<Script_ShotInformation>().GetScalePercent(), newContent); });
                    break;
                case "IPA":
                    newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { bar.GetComponent<Script_PlayerShots>().IPA(newContent.GetComponent<Script_ShotInformation>().GetCurrentScalePercent(), newContent); });
                    break;
                default:
                    Debug.Log("Cannot set shot function, check the internal naming and if it matches the case");
                    break;
            }

            newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { CheckPrices(); } );
            newContent.GetComponentInChildren<Button>().onClick.AddListener(delegate { PlaySFX(); });
        }

        bar.GetComponent<Script_Bar>().CloseMenu();
    }

    public void CheckPrices(){
        int currentPoints = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_PlayerUpgrades>().GetPoints();
        foreach (GameObject entry in entires){
            entry.GetComponent<Script_ShotInformation>().CheckIfCanBuy(currentPoints);
        }
    }

    public void Close()
    {
        gameObject.GetComponentInParent<ScrollRect>().gameObject.SetActive(false);
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_OtherControls>().ToggleCursor();
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_OtherControls>().ToggleInput(true);
        bar.GetComponent<Script_Bar>().ReactivatePrompt();
        inMenu = false;
    }

    public void PlaySFX()
    {
        buySFX.Play();
    }
}
