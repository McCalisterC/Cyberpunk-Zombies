// Assets/Scripts/Player/Weapons/Base Class/Pistol.cs
using System;
using System.Collections;
using UnityEngine;

public class Pistol : Weapon
{
    // Existing fields that need to be made protected or adjusted
    private float boostedDamage = 0;
    private float currentDamage;
    private float currentFireRate;
    private Camera FPCamera;
    private float bloodshots = 0f;
    private Input_Controller _input;
    private bool isShooting;
    private bool canNotShoot;
    private int _clipSize;
    private int _currentAmmoAmount;

    // New: Optional field for unlocked skin (if you want to define it here instead of UIManager)
    [SerializeField] private Material unlockedSkinMaterial; // Assign in Inspector for level 5 skin

    public override float GetHeadshotMultiplier() { return headshotMultiplier; }
    public override float GetCurrentDamage() { return currentDamage; }
    public override float GetCurrentNextShotDamage() { return currentDamage + boostedDamage; }
    public override float GetCurrentFireRate() { return currentFireRate; }

    // New: Grapple hook component
    private GrappleHook grappleHook;

    public override int clipSize
    {
        get => _clipSize;
        set
        {
            _clipSize = value;
            UpdateUI();
        }
    }

    public override int currentAmmoAmount
    {
        get => _currentAmmoAmount;
        set
        {
            _currentAmmoAmount = value;
            UpdateUI();
        }
    }

    public override void SetBloodShots(float percentage) { bloodshots = percentage; }


    public void Awake()
    {
        _clipSize = initClipSize;
        currentAmmoAmount = clipSize;
        
        // New: Setup grapple hook
        SetupGrappleHook();
    }
    
    // New: Setup grapple hook component
    private void SetupGrappleHook()
    {
        // Add GrappleHook component if it doesn't exist
        grappleHook = GetComponent<GrappleHook>();
        if (grappleHook == null)
        {
            grappleHook = gameObject.AddComponent<GrappleHook>();
        }
    }

    private void Start()
    {
        FPCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        currentDamage = initDamage;
        currentFireRate = initFireRate;
        _input = GetComponentInParent<Input_Controller>();
    }

    private void FixedUpdate()
    {
        ButtonReload(false);
        ButtonShoot();
    }

    private void ButtonShoot()
    {
        if (_input.fire)
        {
            if (!isShooting && !canNotShoot)
            {
                isShooting = true;
                Shoot();
            }
        }
        else
        {
            isShooting = false;
        }
    }

    public override void Shoot()
    {
        if (currentAmmoAmount != 0)
        {
            if (!isReloading && isShooting)
            {
                foreach (Action method in shootMethods)
                {
                    method();
                }

                currentAmmoAmount--;
                fpsArms.GetComponent<Animator>().SetTrigger("Shoot");
                Debug.Log("Shot Gun, Current Ammo: " + currentAmmoAmount);

                RaycastHit hit;
                Vector3 direction = GetShootingDirection();
                Physics.Raycast(FPCamera.transform.position, direction, out hit);
                canNotShoot = true;
                StartCoroutine("CanNotShoot");

                if (hit.transform != null)
                {
                    Debug.Log("Hit Object: " + hit.transform.gameObject.name);
                    Script_BasicEnemy enemy = null;

                    float tempDamage = currentDamage + boostedDamage;
                    int points = 0;

                    if (hit.transform.tag == "Enemy Head" || (hit.transform.tag == "Enemy" && vitalTargeting))
                    {
                        GameObject fleshHit = Instantiate(fleshHitEffect, hit.point,
                            Quaternion.FromToRotation(transform.position, hit.normal), hit.transform);
                        tempDamage *= headshotMultiplier;
                        points = 100;
                        enemy = hit.transform.GetComponentInParent<Script_BasicEnemy>();
                    }
                    else if (hit.transform.tag == "Enemy")
                    {
                        GameObject fleshHit = Instantiate(fleshHitEffect, hit.point,
                            Quaternion.FromToRotation(transform.position, hit.normal), hit.transform);
                        points = 50;
                        enemy = hit.transform.GetComponentInParent<Script_BasicEnemy>();
                    }

                    if (enemy != null)
                    {
                        Debug.Log(tempDamage);
                        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>()
                            .AddHealth(tempDamage * bloodshots);
                        enemy.TakeDamage(tempDamage, points);
                    }
                }

                boostedDamage = 0;

                foreach (I_Mods_DamageBoost damageBoost in GameObject.FindGameObjectWithTag("Mechanic")
                    .GetComponentsInChildren<I_Mods_DamageBoost>())
                {
                    damageBoost.currentBonus = 0;
                }
            }
        }
        else if (!isReloading)
        {
            ButtonReload(true);
        }
    }

    public override void Reload()
    {
        Debug.Log("Gun Reloaded");
        fpsArms.GetComponent<Animator>().SetBool("Reload", false);
        currentAmmoAmount = clipSize;
        isReloading = false;
        _input.reload = false;
    }

    public void ButtonReload(bool autoReload)
    {
        if (_input.reload || autoReload)
        {
            if (currentAmmoAmount < clipSize && !isReloading)
            {
                isReloading = true;
                fpsArms.GetComponent<Animator>().SetBool("Reload", true);
                GetComponentInParent<Script_BaseStats>().TriggerReloadMethods();
                _input.reload = false;
            }
            else
                _input.reload = false;
        }
    }

    IEnumerator CanNotShoot()
    {
        Debug.Log("Current fire rate: " + currentFireRate);
        yield return new WaitForSeconds(1f / currentFireRate);
        canNotShoot = false;
    }

    Vector3 GetShootingDirection()
    {
        Vector3 targetPos = FPCamera.transform.position + FPCamera.transform.forward;
        Vector3 direction = targetPos - FPCamera.transform.position;
        return direction.normalized;
    }

    public override void UpgradeDamage(float percentIncrease)
    {
        currentDamage = initDamage * percentIncrease;
    }

    public override void BoostDamage(float amount)
    {
        boostedDamage += amount;
    }

    public override void UpgradeFireRate(float percentIncrease)
    {
        currentFireRate += initFireRate * percentIncrease;
        fpsArms.GetComponent<Animator>().SetFloat("FireRate", 1 + (currentFireRate - initFireRate));
    }

    public override void UpgradeReloadSpeed(float percentIncrease)
    {
        fpsArms.GetComponent<Script_WeaponAnimHandling>().SpeedUpReload(percentIncrease);
    }

    public override void AddShootMethod(Action method)
    {
        shootMethods.Add(method);
    }

    public override void RemoveShootMethod(Action method)
    {
        shootMethods.Remove(method);
    }
}