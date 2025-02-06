using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pistol : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] Camera FPCamera;
    [SerializeField] float headshotMultiplier;
    [SerializeField] int damageAmount;
    [SerializeField] double fireRate;
    [SerializeField] public int clipSize;
    [SerializeField] public int currentAmmoAmount;

    // Animation Variables
    private Animator thisAnim;
    private bool isReloading;
    [SerializeField] private bool isShooting;
    [SerializeField] private bool canNotShoot;

    // Input Variables
    private Input_Controller _input;
    public void Awake(){
        currentAmmoAmount = clipSize;
        thisAnim = this.gameObject.GetComponent<Animator>();
    }

    private void Start(){
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
                currentAmmoAmount--;
                Debug.Log("Shot Gun, Current Ammo: " + currentAmmoAmount);
                RaycastHit hit;
                Vector3 direction = GetShootingDirection();
                Physics.Raycast(FPCamera.transform.position, direction, out hit);
                canNotShoot = true;
                StartCoroutine("CanNotShoot");
                if(hit.transform != null){
                    Debug.Log("Hit Object: " + hit.transform.gameObject.name);
                    /*var hitbox = hit.collider.GetComponent<EnemyHitboxScript>();
                    if(hitbox){
                        float tempDamage;
                        tempDamage = _damageAmount;

                        if(hitbox.isHead){
                            hitbox.DealDamage(Mathf.CeilToInt(tempDamage * headshotMultiplier), direction, true);
                        }
                        else
                            hitbox.DealDamage(Mathf.CeilToInt(tempDamage), direction, false);
                    }*/
                }
            }
        }
        else if(!isReloading){
            ButtonReload(true);
        }
    }

    public void Reload(){
        Debug.Log("Gun Reloaded");
        currentAmmoAmount = clipSize;
        isReloading = false;
        _input.reload = false;
    }

    public void ButtonReload(bool autoReload){
        if (_input.reload || autoReload){
            if(currentAmmoAmount < clipSize){
                isReloading = true;

                //thisAnim.SetTrigger("Reload"); Used for animation, right now will simply reload
                Reload();
            }
            else
                _input.reload = false;
        }
    }

    IEnumerator CanNotShoot(){
        yield return new WaitForSeconds((float)(1.0/fireRate));
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
}
