// Assets/Scripts/Player/Weapons/AutomaticRifle.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticRifle : Weapon
{
    // Rifle-specific fields
    [SerializeField] private float spreadAngle = 5f; // Bullet spread for automatic fire

    private float boostedDamage = 0;
    private float currentDamage;
    public float currentFireRate;
    private Camera FPCamera;
    private float bloodshots = 0f;
    private Input_Controller _input;
    private bool canNotShoot;
    private int _clipSize;
    private int _currentAmmoAmount;
    private Coroutine shootingCoroutine;

    public override float GetHeadshotMultiplier() { return headshotMultiplier; }
    public override float GetCurrentDamage() { return currentDamage; }
    public override float GetCurrentNextShotDamage() { return currentDamage + boostedDamage; }
    public override float GetCurrentFireRate() { return currentFireRate; }

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
            if (!isReloading && shootingCoroutine == null)
            {
                shootingCoroutine = StartCoroutine(AutomaticShoot());
            }
        }
        else
        {
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
        }
    }

    private IEnumerator AutomaticShoot()
    {
        while (_input.fire && currentAmmoAmount > 0 && !isReloading)
        {
            PerformShot();
            yield return new WaitForSeconds(1f / currentFireRate);
        }

        if (currentAmmoAmount <= 0 && !isReloading)
        {
            ButtonReload(true);
        }
    }

    private void PerformShot()
    {
        foreach (Action method in shootMethods)
        {
            method();
        }

        currentAmmoAmount--;
        fpsArms.GetComponent<Animator>().SetTrigger("Shoot");
        Debug.Log("Shot Rifle, Current Ammo: " + currentAmmoAmount);

        // Add some spread to the shot direction
        Vector3 direction = GetShootingDirection();

        RaycastHit hit;
        Physics.Raycast(FPCamera.transform.position, direction, out hit);

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

    public override void Shoot()
    {
        // For automatic rifle, Shoot() is handled through AutomaticShoot coroutine
        // This method can be left empty or used for single-shot if needed
    }

    public override void Reload()
    {
        Debug.Log("Rifle Reloaded");
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
        currentFireRate += percentIncrease;
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