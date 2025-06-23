// Assets\Scripts\Player\Basics\Mods\Individual Mod Scripts\Exotics\Script_AutoTurret.cs
using System.Collections;
using UnityEngine;

public class Script_AutoTurret : MonoBehaviour, I_Mods
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

    // Turret-specific variables
    [Header("Turret Settings")]
    [SerializeField] private GameObject turretPrefab;  // Prefab for the floating turret (assign a simple model in Inspector)
    [SerializeField] private float turretRadius = 3f;  // Distance from player to turret
    [SerializeField] private float orbitSpeed = 50f;   // How fast the turret orbits the player
    [SerializeField] private float detectionRadius = 15f;  // Radius to detect enemies
    [SerializeField] private GameObject muzzleFlashVFX;  // Optional VFX for shooting (assign in Inspector)
    [SerializeField] private AudioClip shootSFX;  // Optional sound effect for shooting

    private GameObject activeTurret;  // Reference to the spawned turret
    private Weapon playerWeapon; // Changed from Pistol to Weapon
    private Transform playerTransform;  // Reference to the player's transform for positioning
    private bool isActive = false;
    private float nextShotTime = 0f;

    private void Start()
    {
        // Set up mod metadata
        rarity = I_Mods.Rarity.EXOTIC;
        modName = "Auto-Turret";
        modDescription = "Spawns a floating turret that orbits you and automatically shoots nearby enemies. Damage and fire rate scale with your upgrades!";
    }

    // Called when the mod is equipped/activated
    public void Activate()
    {
        if (isActive) return;

        // Get references to player components
        GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");
        if (player == null) return;

        playerTransform = player.transform;
        playerWeapon = player.GetComponentInChildren<Weapon>(); // Changed from Pistol
        if (playerWeapon == null) return;

        // Spawn the turret
        activeTurret = Instantiate(turretPrefab, CalculateTurretPosition(), Quaternion.identity);
        isActive = true;

        // Start turret behaviors
        StartCoroutine(OrbitTurret());
        StartCoroutine(ScanAndShoot());
    }

    // Called when the mod is unequipped/deactivated
    public void Deactivate()
    {
        if (!isActive) return;

        // Clean up the turret
        if (activeTurret != null)
        {
            Destroy(activeTurret);
        }
        StopAllCoroutines();
        isActive = false;
    }

    // Calculates the turret's position based on orbit
    private Vector3 CalculateTurretPosition()
    {
        float angle = Time.time * orbitSpeed;
        Vector3 offset = new Vector3(Mathf.Sin(angle), 1f, Mathf.Cos(angle)) * turretRadius;  // Orbit in a circle, 1 unit above ground
        return playerTransform.position + offset;
    }

    // Coroutine to make the turret orbit the player
    private IEnumerator OrbitTurret()
    {
        while (isActive)
        {
            if (activeTurret != null && playerTransform != null)
            {
                activeTurret.transform.position = CalculateTurretPosition();
                // Optional: Make turret face forward relative to player
                activeTurret.transform.rotation = playerTransform.rotation;
            }
            yield return null;
        }
    }

    // Coroutine to scan for enemies and shoot
    private IEnumerator ScanAndShoot()
    {
        while (isActive)
        {
            if (Time.time >= nextShotTime && playerWeapon != null)
            {
                // Find nearest enemy within radius
                Collider[] colliders = Physics.OverlapSphere(activeTurret.transform.position, detectionRadius);
                Script_BasicEnemy nearestEnemy = null;
                float minDistance = float.MaxValue;

                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        Script_BasicEnemy enemy = collider.GetComponent<Script_BasicEnemy>();
                        if (enemy != null && !enemy.cantTakeDamage)
                        {
                            float distance = Vector3.Distance(activeTurret.transform.position, collider.transform.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                nearestEnemy = enemy;
                            }
                        }
                    }
                }

                if (nearestEnemy != null)
                {
                    // Shoot at the nearest enemy
                    ShootAtEnemy(nearestEnemy);
                    // Set cooldown based on player's fire rate
                    nextShotTime = Time.time + (1f / playerWeapon.GetCurrentFireRate()); // Removed unnecessary GetComponent
                }
            }
            yield return new WaitForSeconds(0.1f);  // Check every 0.1 seconds
        }
    }

    // Handles shooting logic (similar to Pistol.Shoot())
    private void ShootAtEnemy(Script_BasicEnemy enemy)
    {
        if (playerWeapon == null || activeTurret == null) return;

        // Calculate damage based on player's current damage
        float turretDamage = playerWeapon.GetCurrentNextShotDamage();
        int points = 50;  // Base points, can be adjusted

        // Deal damage and award points
        enemy.TakeDamage(turretDamage, points);

        // Visual/Audio Feedback
        if (muzzleFlashVFX != null)
        {
            Instantiate(muzzleFlashVFX, activeTurret.transform.position, Quaternion.LookRotation(enemy.transform.position));
        }
        if (shootSFX != null && GetComponent<AudioSource>() != null)
        {
            GetComponent<AudioSource>().PlayOneShot(shootSFX);
        }
    }
}