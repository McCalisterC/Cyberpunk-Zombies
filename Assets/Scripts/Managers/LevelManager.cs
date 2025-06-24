// Assets/Scripts/Managers/LevelManager.cs
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // Account properties
    public int AccountLevel { get; private set; }
    public int AccountXP { get; private set; }

    // Class-specific properties (expandable for more classes)
    public int PistolLevel { get; private set; }
    public int PistolXP { get; private set; }
    public int AutomaticRifleLevel { get; private set; }
    public int AutomaticRifleXP { get; private set; }

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

    private void LoadProgress()
    {
        AccountXP = PlayerPrefs.GetInt("Account_XP", 0);
        AccountLevel = CalculateLevel(AccountXP);
        PistolXP = PlayerPrefs.GetInt("Pistol_XP", 0);
        PistolLevel = CalculateLevel(PistolXP);
        AutomaticRifleXP = PlayerPrefs.GetInt("AutomaticRifle_XP", 0);
        AutomaticRifleLevel = CalculateLevel(AutomaticRifleXP);
        Debug.Log("Loaded player progress.");
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("Account_XP", AccountXP);
        PlayerPrefs.SetInt("Pistol_XP", PistolXP);
        PlayerPrefs.SetInt("AutomaticRifle_XP", AutomaticRifleXP);
        PlayerPrefs.Save();
        Debug.Log("Saved player progress.");
    }

    // Optional: Debug method to reset progress (call from a menu)
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        LoadProgress();
    }
}