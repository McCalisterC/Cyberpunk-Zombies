// Assets/Scripts/Managers/LevelManager.cs
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // Account properties
    public int AccountLevel { get; private set; }
    public int AccountXP { get; private set; }
    public int AccountCurrency { get; private set; } // New: Account currency

    // Class-specific properties (expandable for more classes)
    public int PistolLevel { get; private set; }
    public int PistolXP { get; private set; }
    public int AutomaticRifleLevel { get; private set; }
    public int AutomaticRifleXP { get; private set; }

    // New: Skin ownership tracking
    private HashSet<string> ownedSkins = new HashSet<string>();
    private Dictionary<string, string> equippedSkins = new Dictionary<string, string>(); // className -> skinId

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
        LoadProgress(); // Load on startup
    }

    // Calculate level based on total XP (e.g., Level 1 = 1000 XP, Level 2 = 2000 XP cumulative)
    private int CalculateLevel(int totalXP)
    {
        int level = 0;
        int xpRequired = 0;
        while (totalXP >= xpRequired)
        {
            level++;
            xpRequired += level * 1000; // Cumulative: Adjust multiplier as needed
        }
        return level - 1; // Return the highest achieved level
    }

    // Add XP to account and optionally a class
    public void AddXP(int xpAmount, string className = null)
    {
        // Always add to account
        AccountXP += xpAmount;
        int newAccountLevel = CalculateLevel(AccountXP);
        if (newAccountLevel > AccountLevel)
        {
            AccountLevel = newAccountLevel;
            Script_ChatInput.Instance.SystemMessage($"You have obtained account level {newAccountLevel}");
        }

        // Add to specific class if provided
        if (!string.IsNullOrEmpty(className))
        {
            if (className == "Pistol")
            {
                PistolXP += xpAmount;
                int newPistolLevel = CalculateLevel(PistolXP);
                if (newPistolLevel > PistolLevel)
                {
                    PistolLevel = newPistolLevel;
                    Script_ChatInput.Instance.SystemMessage($"You have obtained {className} level {newPistolLevel}");
                }
            }
            else if (className == "Automatic Rifle")
            {
                AutomaticRifleXP += xpAmount;
                int newRifleLevel = CalculateLevel(AutomaticRifleXP);
                if (newRifleLevel > AutomaticRifleLevel)
                {
                    AutomaticRifleLevel = newRifleLevel;
                    Script_ChatInput.Instance.SystemMessage($"You have obtained {className} level {newRifleLevel}");
                }
            }
        }

        SaveProgress(); // Save after changes
    }

    // Check if a class is unlocked based on account level
    public bool IsClassUnlocked(string className)
    {
        if (className == "Automatic Rifle")
        {
            return AccountLevel >= 5;
        }
        return true; // Pistol is always unlocked
    }

    // Check if a class skin is unlocked
    public bool IsClassSkinUnlocked(string className)
    {
        if (className == "Pistol") return PistolLevel >= 5;
        if (className == "Automatic Rifle") return AutomaticRifleLevel >= 5;
        return false;
    }

    // Add currency to account
    public void AddCurrency(int amount)
    {
        AccountCurrency += amount;
        SaveProgress();
        Script_ChatInput.Instance.SystemMessage($"You earned {amount} coins! Total: {AccountCurrency}");
    }

    // Spend currency (returns true if successful)
    public bool SpendCurrency(int amount)
    {
        if (AccountCurrency >= amount)
        {
            AccountCurrency -= amount;
            SaveProgress();
            return true;
        }
        return false;
    }

    // New: Skin management methods
    public bool PurchaseSkin(string skinId, int cost)
    {
        if (ownedSkins.Contains(skinId))
        {
            Debug.Log($"Skin {skinId} already owned!");
            return false;
        }

        if (SpendCurrency(cost))
        {
            ownedSkins.Add(skinId);
            SaveSkinData();
            Script_ChatInput.Instance.SystemMessage($"Purchased skin: {skinId}");
            return true;
        }
        else
        {
            Script_ChatInput.Instance.SystemMessage("Not enough currency!");
            return false;
        }
    }

    public bool IsSkinOwned(string skinId)
    {
        return ownedSkins.Contains(skinId);
    }

    public void EquipSkin(string className, string skinId)
    {
        if (IsSkinOwned(skinId) || skinId == "Default")
        {
            equippedSkins[className] = skinId;
            SaveSkinData();
            Script_ChatInput.Instance.SystemMessage($"Equipped {skinId} for {className}");
        }
    }

    public string GetEquippedSkin(string className)
    {
        return equippedSkins.ContainsKey(className) ? equippedSkins[className] : "Default";
    }

    public HashSet<string> GetOwnedSkins()
    {
        return new HashSet<string>(ownedSkins);
    }

    private void LoadProgress()
    {
        AccountXP = PlayerPrefs.GetInt("Account_XP", 0);
        AccountLevel = CalculateLevel(AccountXP);
        AccountCurrency = PlayerPrefs.GetInt("Account_Currency", 1000); // Start with 1000 coins
        PistolXP = PlayerPrefs.GetInt("Pistol_XP", 0);
        PistolLevel = CalculateLevel(PistolXP);
        AutomaticRifleXP = PlayerPrefs.GetInt("AutomaticRifle_XP", 0);
        AutomaticRifleLevel = CalculateLevel(AutomaticRifleXP);
        LoadSkinData();
        Debug.Log("Loaded player progress.");
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("Account_XP", AccountXP);
        PlayerPrefs.SetInt("Account_Currency", AccountCurrency);
        PlayerPrefs.SetInt("Pistol_XP", PistolXP);
        PlayerPrefs.SetInt("AutomaticRifle_XP", AutomaticRifleXP);
        PlayerPrefs.Save();
        Debug.Log("Saved player progress.");
    }

    private void LoadSkinData()
    {
        // Load owned skins
        string ownedSkinsData = PlayerPrefs.GetString("Owned_Skins", "");
        if (!string.IsNullOrEmpty(ownedSkinsData))
        {
            string[] skinArray = ownedSkinsData.Split(',');
            foreach (string skin in skinArray)
            {
                if (!string.IsNullOrEmpty(skin))
                    ownedSkins.Add(skin);
            }
        }

        // Load equipped skins
        string equippedSkinsData = PlayerPrefs.GetString("Equipped_Skins", "");
        if (!string.IsNullOrEmpty(equippedSkinsData))
        {
            string[] pairs = equippedSkinsData.Split('|');
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split(':');
                if (keyValue.Length == 2)
                {
                    equippedSkins[keyValue[0]] = keyValue[1];
                }
            }
        }
    }

    private void SaveSkinData()
    {
        // Save owned skins
        string ownedSkinsData = string.Join(",", ownedSkins);
        PlayerPrefs.SetString("Owned_Skins", ownedSkinsData);

        // Save equipped skins
        List<string> equippedPairs = new List<string>();
        foreach (var kvp in equippedSkins)
        {
            equippedPairs.Add($"{kvp.Key}:{kvp.Value}");
        }
        string equippedSkinsData = string.Join("|", equippedPairs);
        PlayerPrefs.SetString("Equipped_Skins", equippedSkinsData);

        PlayerPrefs.Save();
    }

    // Optional: Debug method to reset progress (call from a menu)
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        LoadProgress();
    }
}