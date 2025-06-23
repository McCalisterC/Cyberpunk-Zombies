using System.Collections;
using UnityEngine;

public class Script_BlackHole : MonoBehaviour, I_Mods
{
    // I_Mods interface properties
    public I_Mods.Rarity rarity { get => _rarity; set => _rarity = value; }
    public string modName { get => _modName; set => _modName = value; }
    public string modDescription { get => _modDescription; set => _modDescription = value; }
    public Sprite modIcon { get => _modIcon; set => _modIcon = value; }

    [SerializeField] private Sprite _modIcon;  // Assign in Inspector
    private string _modName;
    private string _modDescription;
    [SerializeField] private I_Mods.Rarity _rarity;

    // Mod-specific variables (tuned by rarity)
    private float radius;
    private float stunDuration;
    private float cooldownTime;
    private bool onCooldown = false;
    private string descriptor;  // For flavor text, like in the example mod

    // Optional VFX/SFX (assign in Inspector for visual feedback)
    [SerializeField] private GameObject blackHoleVFXPrefab;  // E.g., a particle system for the black hole effect
    [SerializeField] private AudioClip blackHoleSFX;  // Optional sound effect

    [SerializeField] private bool debug;  // For testing

    private void Start()
    {
        // Set up mod based on rarity (similar to Script_ReloadKnockback)
        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                radius = 5f;
                stunDuration = 2f;
                cooldownTime = 30f;
                descriptor = "small";
                break;
            case I_Mods.Rarity.Rare:
                radius = 7f;
                stunDuration = 3f;
                cooldownTime = 25f;
                descriptor = "medium";
                break;
            case I_Mods.Rarity.Epic:
                radius = 10f;
                stunDuration = 4f;
                cooldownTime = 20f;
                descriptor = "large";
                break;
            case I_Mods.Rarity.Legendary:
                radius = 15f;
                stunDuration = 5f;
                cooldownTime = 15f;
                descriptor = "massive";
                break;
        }

        // Set mod metadata
        modName = "Black Hole";
        modDescription = $"On zombie kill, pull nearby zombies into a {descriptor} black hole and stun them!";
    }

    // Called when the mod is equipped/activated
    public void Activate()
    {
        // Hook into the player's on-kill event (using the new method in Script_BaseStats)
        GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");
        if (player != null)
        {
            player.GetComponent<Script_BaseStats>().AddOnEnemyKillMethod(TriggerBlackHole);
        }
    }

    // Called when the mod is unequipped/deactivated
    public void Deactivate()
    {
        // Unhook from the on-kill event
        GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");
        if (player != null)
        {
            player.GetComponent<Script_BaseStats>().RemoveOnEnemyKillMethod(TriggerBlackHole);
        }
    }

    // The core method triggered on enemy kill (passed the killed enemy's position)
    private void TriggerBlackHole(Vector3 centerPosition)
    {
        if (onCooldown) return;  // Respect cooldown

        if (debug) Debug.Log("Black Hole triggered at: " + centerPosition);

        // Start cooldown
        StartCoroutine(CooldownTimer());

        // Optional: Spawn VFX/SFX at the center for feedback
        if (blackHoleVFXPrefab != null)
        {
            GameObject vfx = Instantiate(blackHoleVFXPrefab, centerPosition, Quaternion.identity);
            Destroy(vfx, stunDuration);  // Destroy after effect ends
        }
        if (blackHoleSFX != null && GetComponent<AudioSource>() != null)
        {
            GetComponent<AudioSource>().PlayOneShot(blackHoleSFX);
        }

        // Find and affect nearby enemies (similar to OverlapBox in Script_ReloadKnockback)
        Collider[] colliders = Physics.OverlapSphere(centerPosition, radius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Script_BasicEnemy enemy = collider.GetComponent<Script_BasicEnemy>();

                if (enemy != null && !enemy.isStunned)
                {
                    // Stun the enemy (using the new RPC in Script_BasicEnemy)
                    enemy.StunEnemyRpc(stunDuration, centerPosition);
                }
            }
        }
    }

    // Cooldown coroutine to prevent spamming
    private IEnumerator CooldownTimer()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        onCooldown = false;
    }
}