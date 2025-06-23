// Assets/Scripts/Player/Weapons/Script_WeaponAnimHandling.cs
using UnityEngine;

public class Script_WeaponAnimHandling : MonoBehaviour
{
    private Weapon weapon; // Changed from Pistol to Weapon
    private float reloadSpeed = 1;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource gunShotAudio;

    public void SpeedUpReload(float percentIncrease)
    {
        reloadSpeed += percentIncrease;
        GetComponent<Animator>().SetFloat("ReloadSpeed", reloadSpeed);
    }

    public void ReloadWeapon()
    {
        if (weapon == null)
        {
            weapon = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Weapon>();
        }

        Debug.Log("Sending weapon reload");
        weapon.Reload();
    }

    public void TriggerMuzzleFlash()
    {
        muzzleFlash.Play();
        gunShotAudio.Play();
    }
}