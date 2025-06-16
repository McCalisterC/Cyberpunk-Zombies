using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] GameObject fpsArms;
    [SerializeField] float headshotMultiplier;
    [SerializeField] float initDamage;
    [SerializeField] float initFireRate;
    [SerializeField] public int clipSize
    {
        get
        {
            return _clipSize;
        }
        set
        {
            _clipSize = value;
            GameObject.FindGameObjectWithTag("UI Manager").GetComponent<Script_UIManager>().gunInfoText.text = _currentAmmoAmount + "/" + _clipSize;
        }
    }

    int _clipSize = 6;

    [SerializeField] public int currentAmmoAmount
    {
        get
        {
            return _currentAmmoAmount;
        }
        set
        {
            _currentAmmoAmount = value;
            GameObject.FindGameObjectWithTag("UI Manager").GetComponent<Script_UIManager>().gunInfoText.text = _currentAmmoAmount + "/" + _clipSize;
        }
    }

    int _currentAmmoAmount = 6;

    // Animation Variables
    public bool isReloading = false;
    [SerializeField] private bool isShooting;
    [SerializeField] private bool canNotShoot;

    // VFX Variables
    [Header("VFX")]
    [SerializeField] GameObject fleshHitEffect;

    // Input Variables
    private Input_Controller _input;

    // Variables for upgrades
    private float currentDamage;
    private float boostedDamage = 0;
    public float GetCurrentNextShotDamage() { return currentDamage + boostedDamage; }
    private float currentFireRate;

    private Camera FPCamera;

    // Mod methods
    List<Action> shootMethods = new List<Action>();
    public bool vitalTargeting = false;
    private float bloodshots = 0f;
    public void SetBloodShots(float percentage) { bloodshots = percentage; }

    public void Awake(){
        currentAmmoAmount = clipSize;
    }

    private void Start(){
        FPCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        currentDamage = initDamage;
        currentFireRate = initFireRate;
        _input = GetComponentInParent<Input_Controller>();
    }

    private void FixedUpdate() {
        ButtonReload(false);
        ButtonShoot();
    }

    private void ButtonShoot(){
        if(_input.fire){
            if(!isShooting && !canNotShoot){
                isShooting = true;
                Shoot();
            }
        }
        else{
            isShooting = false;
        }
    }

    public void Shoot(){
        if(currentAmmoAmount != 0)
        {
            if(!isReloading && isShooting){
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
                if(hit.transform != null){
                    Debug.Log("Hit Object: " + hit.transform.gameObject.name);
                    Script_BasicEnemy enemy = null;

                    float tempDamage = currentDamage + boostedDamage;
                    int points = 0;

                    if(hit.transform.tag == "Enemy Head" || (hit.transform.tag == "Enemy" && vitalTargeting))
                    {
                        GameObject fleshHit = Instantiate(fleshHitEffect, hit.point, Quaternion.FromToRotation(this.transform.position, hit.normal), hit.transform);
                        tempDamage *= headshotMultiplier;
                        points = 100;
                        enemy = hit.transform.GetComponentInParent<Script_BasicEnemy>();
                    }
                    else if (hit.transform.tag == "Enemy")
                    {
                        GameObject fleshHit = Instantiate(fleshHitEffect, hit.point, Quaternion.FromToRotation(this.transform.position, hit.normal), hit.transform);
                        points = 50;
                        enemy = hit.transform.GetComponentInParent<Script_BasicEnemy>();
                    }

                    if(enemy != null){
                        Debug.Log(tempDamage);
                        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<Script_BaseStats>().AddHealth(tempDamage * bloodshots);
                        enemy.TakeDamage(tempDamage, points);
                    }
                }

                boostedDamage = 0;

                foreach (I_Mods_DamageBoost damageBoost in GameObject.FindGameObjectWithTag("Mechanic").GetComponentsInChildren<I_Mods_DamageBoost>())
                {
                    damageBoost.currentBonus = 0;
                }
            }
        }
        else if(!isReloading){
            ButtonReload(true);
        }
    }

    public void Reload(){
        Debug.Log("Gun Reloaded");
        fpsArms.GetComponent<Animator>().SetBool("Reload", false);
        currentAmmoAmount = clipSize;
        isReloading = false;
        _input.reload = false;
    }

    public void ButtonReload(bool autoReload){
        if (_input.reload || autoReload ){
            if (currentAmmoAmount < clipSize && !isReloading){
                isReloading = true;

                fpsArms.GetComponent<Animator>().SetBool("Reload", true);
                GetComponentInParent<Script_BaseStats>().TriggerReloadMethods();
                _input.reload = false;
            }
            else
                _input.reload = false;
        }
    }

    IEnumerator CanNotShoot(){
        Debug.Log("Current fire rate: " + currentFireRate);
        yield return new WaitForSeconds((float)(1.0/currentFireRate));
        canNotShoot = false;
    }

    Vector3 GetShootingDirection(){
        Vector3 targetPos = FPCamera.gameObject.GetComponent<Transform>().position + FPCamera.gameObject.GetComponent<Transform>().forward;
        Vector3 direction = targetPos - FPCamera.gameObject.GetComponent<Transform>().position;
        return direction.normalized;
    }

    public void Disable(){
        this.gameObject.SetActive(false);
    }

    public void StopReload()
    {
        isReloading = false;
    }

    public void UpgradeDamage(float percentIncrease){
        currentDamage = initDamage * percentIncrease;
    }

    public void BoostDamage(float amount)
    {
        boostedDamage += amount;
    }

    public void UpgradeFireRate(float percentIncrease)
    {
        currentFireRate += initFireRate * percentIncrease;
        fpsArms.GetComponent<Animator>().SetFloat("FireRate", 1 + (currentFireRate - initFireRate));
    }

    public void UpgradeReloadSpeed(float percentIncrease)
    {
        fpsArms.GetComponent<Script_WeaponAnimHandling>().SpeedUpReload(percentIncrease);
    }

    public void AddShootMethod(Action method)
    {
        shootMethods.Add(method);
    }

    public void RemoveShootMethod(Action method)
    {
        shootMethods.Remove(method);
    }
}
