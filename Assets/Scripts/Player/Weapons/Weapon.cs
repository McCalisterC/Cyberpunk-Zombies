// Assets/Scripts/Player/Weapons/Base Class/Weapon.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] protected GameObject fpsArms;
    [SerializeField] protected float headshotMultiplier;
    [SerializeField] protected float initDamage;
    [SerializeField] protected float initFireRate;
    [SerializeField] protected int initClipSize;

    // Properties
    public abstract float GetHeadshotMultiplier();
    public abstract float GetCurrentDamage();
    public abstract float GetCurrentNextShotDamage();
    public abstract float GetCurrentFireRate();
    public abstract int clipSize { get; set; }
    public abstract int currentAmmoAmount { get; set; }

    // State
    public bool isReloading = false;
    public bool vitalTargeting = false;

    // VFX
    [Header("VFX")]
    [SerializeField] protected GameObject fleshHitEffect;

    // Mod methods
    protected List<Action> shootMethods = new List<Action>();

    // Abstract methods that must be implemented
    public abstract void Shoot();
    public abstract void Reload();
    public abstract void UpgradeDamage(float percentIncrease);
    public abstract void BoostDamage(float amount);
    public abstract void UpgradeFireRate(float percentIncrease);
    public abstract void UpgradeReloadSpeed(float percentIncrease);
    public abstract void AddShootMethod(Action method);
    public abstract void RemoveShootMethod(Action method);

    // Common methods that can be overridden if needed
    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }

    public virtual void StopReload()
    {
        isReloading = false;
    }

    public virtual void SetBloodShots(float percentage)
    {
        // Implementation can be added in derived classes if needed
    }

    protected virtual void UpdateUI()
    {
        if (GameObject.FindGameObjectWithTag("UI Manager") != null)
        {
            GameObject.FindGameObjectWithTag("UI Manager").GetComponent<Script_UIManager>().gunInfoText.text =
                currentAmmoAmount + "/" + clipSize;
        }
    }

    public void SetFPSArms(GameObject arms)
    {
        fpsArms = arms;
        fpsArms.GetComponent<Animator>().SetFloat("FireRate", initFireRate);
    }
}