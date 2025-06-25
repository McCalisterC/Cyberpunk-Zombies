// Assets/Scripts/UI/Script_WeaponSkinShop.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class WeaponSkin
{
    public string skinId;
    public string displayName;
    public string className; // "Pistol" or "Automatic Rifle"
    public Material skinMaterial;
    public int cost;
    public Sprite previewImage;
}

public class Script_WeaponSkinShop : MonoBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private Transform skinListParent;
    [SerializeField] private GameObject skinItemPrefab;
    [SerializeField] private Button closeShopButton;

    [Header("Skin Preview")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text skinNameText;
    [SerializeField] private TMP_Text skinClassText;
    [SerializeField] private TMP_Text skinCostText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button equipButton;

    [Header("Available Skins")]
    [SerializeField] private List<WeaponSkin> availableSkins = new List<WeaponSkin>();

    private WeaponSkin selectedSkin;
    private List<GameObject> skinItems = new List<GameObject>();

    public static Script_WeaponSkinShop Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        closeShopButton.onClick.AddListener(CloseShop);
        purchaseButton.onClick.AddListener(PurchaseSelectedSkin);
        equipButton.onClick.AddListener(EquipSelectedSkin);

        shopPanel.SetActive(false);
        PopulateSkinList();
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        UpdateCurrencyDisplay();
        RefreshSkinList();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    private void PopulateSkinList()
    {
        foreach (WeaponSkin skin in availableSkins)
        {
            GameObject skinItem = Instantiate(skinItemPrefab, skinListParent);
            skinItems.Add(skinItem);

            // Setup skin item UI
            TMP_Text nameText = skinItem.transform.Find("SkinName").GetComponent<TMP_Text>();
            TMP_Text classText = skinItem.transform.Find("ClassName").GetComponent<TMP_Text>();
            TMP_Text costText = skinItem.transform.Find("Cost").GetComponent<TMP_Text>();
            Image previewImg = skinItem.transform.Find("Preview").GetComponent<Image>();
            Button selectButton = skinItem.GetComponent<Button>();

            nameText.text = skin.displayName;
            classText.text = skin.className;
            costText.text = $"{skin.cost} coins";
            previewImg.sprite = skin.previewImage;

            // Add click listener
            WeaponSkin skinRef = skin; // Capture for closure
            selectButton.onClick.AddListener(() => SelectSkin(skinRef));
        }
    }

    private void RefreshSkinList()
    {
        for (int i = 0; i < skinItems.Count && i < availableSkins.Count; i++)
        {
            WeaponSkin skin = availableSkins[i];
            GameObject skinItem = skinItems[i];

            // Update ownership status
            bool isOwned = LevelManager.Instance.IsSkinOwned(skin.skinId);
            bool isEquipped = LevelManager.Instance.GetEquippedSkin(skin.className) == skin.skinId;

            // Update visual indicators
            Transform statusIndicator = skinItem.transform.Find("StatusIndicator");
            if (statusIndicator != null)
            {
                TMP_Text statusText = statusIndicator.GetComponent<TMP_Text>();
                if (isEquipped)
                {
                    statusText.text = "EQUIPPED";
                    statusText.color = Color.green;
                }
                else if (isOwned)
                {
                    statusText.text = "OWNED";
                    statusText.color = Color.blue;
                }
                else
                {
                    statusText.text = "";
                }
            }
        }
    }

    private void SelectSkin(WeaponSkin skin)
    {
        selectedSkin = skin;

        // Update preview panel
        previewImage.sprite = skin.previewImage;
        skinNameText.text = skin.displayName;
        skinClassText.text = skin.className;
        skinCostText.text = $"{skin.cost} coins";

        // Update button states
        bool isOwned = LevelManager.Instance.IsSkinOwned(skin.skinId);
        bool isEquipped = LevelManager.Instance.GetEquippedSkin(skin.className) == skin.skinId;

        purchaseButton.gameObject.SetActive(!isOwned);
        equipButton.gameObject.SetActive(isOwned && !isEquipped);

        if (isEquipped)
        {
            equipButton.gameObject.SetActive(true);
            equipButton.GetComponentInChildren<TMP_Text>().text = "EQUIPPED";
            equipButton.interactable = false;
        }
        else if (isOwned)
        {
            equipButton.interactable = true;
            equipButton.GetComponentInChildren<TMP_Text>().text = "EQUIP";
        }
    }

    private void PurchaseSelectedSkin()
    {
        if (selectedSkin != null && LevelManager.Instance.PurchaseSkin(selectedSkin.skinId, selectedSkin.cost))
        {
            UpdateCurrencyDisplay();
            RefreshSkinList();
            SelectSkin(selectedSkin); // Refresh selection UI
        }
    }

    private void EquipSelectedSkin()
    {
        if (selectedSkin != null)
        {
            LevelManager.Instance.EquipSkin(selectedSkin.className, selectedSkin.skinId);
            RefreshSkinList();
            SelectSkin(selectedSkin); // Refresh selection UI
        }
    }

    private void UpdateCurrencyDisplay()
    {
        if (LevelManager.Instance != null)
        {
            currencyText.text = $"Coins: {LevelManager.Instance.AccountCurrency}";
        }
    }

    // Method to get skin material by ID
    public Material GetSkinMaterial(string skinId)
    {
        foreach (WeaponSkin skin in availableSkins)
        {
            if (skin.skinId == skinId)
                return skin.skinMaterial;
        }
        return null;
    }
}