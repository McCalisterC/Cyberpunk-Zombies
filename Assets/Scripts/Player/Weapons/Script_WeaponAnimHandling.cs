using UnityEngine;

public class Script_WeaponAnimHandling : MonoBehaviour
{
    private Pistol pistol;
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
        if (pistol == null)
        {
            pistol = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Pistol>();
        }

        Debug.Log("Sending pistol reload");
        pistol.Reload();
    }
    public void TriggerMuzzleFlash()
    {
        muzzleFlash.Play();
        gunShotAudio.Play();
    }
}
