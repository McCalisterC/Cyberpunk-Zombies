using System;
using UnityEngine;
using UnityEngine.UI;

public class Script_ReloadKnockback : MonoBehaviour, I_Mods
{
    public I_Mods.Rarity rarity { get => _rarity; set => _rarity = value; }
    public string modName { get => _modName; set => _modName = value; }
    public string modDescription { get => _modDescription; set => _modDescription = value; }
    public Sprite modIcon { get => _modIcon; set => _modIcon = value; }

    [SerializeField] Sprite _modIcon;
    string _modName;
    string _modDescription;
    [SerializeField] I_Mods.Rarity _rarity;
    Script_BaseStats.ReloadMechanics method;

    private float knockbackForce;
    private string descriptor;

    [SerializeField] bool debug;

    private void Start()
    {
        method = new Script_BaseStats.ReloadMechanics { method = delegate { Knockback(knockbackForce); }, methodFloat = knockbackForce };

        switch (rarity)
        {
            case I_Mods.Rarity.Common:
                knockbackForce = 100;
                descriptor = "small";
                break;
            case I_Mods.Rarity.Rare:
                knockbackForce = 150;
                descriptor = "medium";
                break;
            case I_Mods.Rarity.Epic:
                knockbackForce = 200;
                descriptor = "large";
                break;
            case I_Mods.Rarity.Legendary:
                knockbackForce = 300;
                descriptor = "EXTREME";
                break;
        }

        modName = "Backdraft";
        modDescription = "Knockback nearby enemies " + descriptor + " amount!";
    }

    public void Activate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_BaseStats>().AddReloadMethod(method);
    }

    public void Deactivate()
    {
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponentInChildren<Script_BaseStats>().RemoveReloadMethod(method);
    }

    public int Knockback(float knockbackAmount)
    {
        GameObject player = GameObject.FindGameObjectWithTag("LocalPlayer");

        Collider[] colliders = Physics.OverlapBox(player.transform.position, player.transform.localScale, Quaternion.identity);

        foreach(Collider collider in colliders)
        {
            if (collider.tag == "Enemy")
            {
                print("Enemy knocked back");
                Vector3 direction = collider.transform.position - player.transform.position;
                collider.GetComponent<Rigidbody>().isKinematic = false;
                collider.GetComponent<Rigidbody>().AddForceAtPosition(direction.normalized * knockbackAmount, collider.transform.position, ForceMode.Impulse);
                StartCoroutine(collider.GetComponent<Script_BasicEnemy>().ToggleKinematic(true, 2));
            }
        }

        return 0;
    }
}
