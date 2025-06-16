using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Script_BaseStats : NetworkBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] float health = 150;
    [SerializeField] float regenTimer = 2f;

    private float maxHealth;
    private Coroutine lastRegenTimer;
    private Coroutine lastRegen;
    public bool isDead = false;
    private bool invulnerable = false;
    private string playerName = "";

    private bool godMode = false;

    public bool GetDeathStatus() { return isDead; }

    // Mod Methods
    public struct ReloadMechanics
    {
        public Action<float> method;
        public float methodFloat;
    }

    private List<ReloadMechanics> reloadMethods = new List<ReloadMechanics>();
    private List<Action> takeDamageMethods = new List<Action>();
    public bool DeathTax = false;

    void Start()
    {
        Script_UIManager.Instance.healthBar.maxValue = health;
        Script_UIManager.Instance.healthBar.value = health;
        maxHealth = health;
    }

    public void TakeDamage(float damage){
        if (invulnerable || godMode)
        {
            Debug.Log("Can't take damage");
            return;
        }

        health -= damage;

        foreach (Action action in takeDamageMethods)
        {
            action();
        }

        if (tag == "LocalPlayer")
            Script_UIManager.Instance.healthBar.value = health;

        if (lastRegenTimer != null)
        {
            StopCoroutine(lastRegenTimer);
        }
        
        if (lastRegen != null)
        {
            StopCoroutine(lastRegen);
        }

        if (health <= 0)
        {
            if (DeathTax)
            {
                health = 10;
                if (tag == "LocalPlayer")
                    Script_UIManager.Instance.healthBar.value = health;

                DeathTax = false;
                invulnerable = true;

                I_Mods modToDestroy = null;

                Script_Mechanic mechanicScript = GameObject.FindGameObjectWithTag("Mechanic").GetComponentInChildren<Script_Mechanic>();

                List<I_Mods> activeMods = mechanicScript.GetScrapHandler().GetActiveMods();
                foreach (I_Mods mod in activeMods)
                {
                    if (mod.modName == "Death Tax")
                    {
                        modToDestroy = mod;
                    }
                }

                if (modToDestroy != null)
                {
                    GameObject modIcon = mechanicScript.GetScrapHandler().modIcons[mechanicScript.GetScrapHandler().GetActiveMods().IndexOf(modToDestroy)];
                    mechanicScript.GetScrapHandler().modIcons.Remove(modIcon);
                    Destroy(modIcon);
                    GameObject.FindGameObjectWithTag("Mechanic").GetComponentInChildren<Script_Mechanic>().GetScrapHandler().GetActiveMods().Remove(modToDestroy);
                }

                StartCoroutine(InvulernableBuffer());
                StartRegen();
            }

            else
            {
                GetComponentInChildren<CinemachineCamera>().enabled = false;
                GetComponent<CharacterController>().enabled = false;
                transform.position = new Vector3(NetworkManager.LocalClientId * 2, 0.3f, 0);
                GetComponent<CharacterController>().enabled = true;
                PlayerDeathRpc();
            }
        }

        else
            StartRegen();
    }

    IEnumerator InvulernableBuffer()
    {
        yield return new WaitForSeconds(2);
        invulnerable = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayerDeathRpc()
    {
        isDead = true;
        GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().PlayerDeath(gameObject);
        if (tag == "LocalPlayer")
        {
            if (GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().GetPlayers().Count > 0)
            {
                Script_UIManager.Instance.SpectatorCamera(0);
                Script_UIManager.Instance.ToggleSpectatorUI(true);
            }
            Script_UIManager.Instance.ToggleGameplayUI(false);
            GetComponentInChildren<CinemachineCamera>().enabled = false;
        }

        if (tag == "LocalPlayer")
            GetComponent<PlayerInput>().SwitchCurrentActionMap("ChatBox");
        
        foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.enabled = false;
        }

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            skinnedMeshRenderer.enabled = false;
        }

        GetComponent<CharacterController>().enabled = false;
        transform.position = GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().deathTransportPos.transform.position;
        GetComponent<CharacterController>().enabled = true;
    }

    public void StartRegen()
    {
        if (health < maxHealth)
        {
            lastRegenTimer = StartCoroutine(RegenTimer());
        }
    }

    IEnumerator RegenTimer()
    {
        yield return new WaitForSeconds(regenTimer);
        lastRegen = StartCoroutine(Regen());
    }

    IEnumerator Regen()
    {
        if (health < maxHealth)
        {
            if (health + 10 >= maxHealth)
            {
                health = maxHealth;
            }
            else
                health += 10;

            if (tag == "LocalPlayer")
                Script_UIManager.Instance.healthBar.value = health;

            if (health == maxHealth)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
            lastRegen = StartCoroutine(Regen());
        }
    }

    public void UpgradeHealth(float value)
    {
        maxHealth += value;
        health = maxHealth;
        Script_UIManager.Instance.healthBar.maxValue = health;
        Script_UIManager.Instance.healthBar.value = health;
    }

    public void UpgradeRegenTime(float value)
    {
        regenTimer = regenTimer - (regenTimer * value);
    }

    public void Revive(Vector3 respawnPoint)
    {
        isDead = false;
        health = maxHealth;

        if (tag == "LocalPlayer")
            GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");

        foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.enabled = true;
        }

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            skinnedMeshRenderer.enabled = true;
        }

        GetComponent<CharacterController>().enabled = false;
        transform.position = respawnPoint;
        GetComponent<CharacterController>().enabled = true;

        if (tag == "LocalPlayer")
        {
            Script_UIManager.Instance.ToggleGameplayUI(true);
            Script_UIManager.Instance.ToggleSpectatorUI(false);
            Script_UIManager.Instance.healthBar.value = health;
            GetComponent<Script_OtherControls>().ReactivateCamera();
        }
    }

    public void AddReloadMethod(ReloadMechanics method)
    {
        reloadMethods.Add(method);
    }

    public void RemoveReloadMethod(ReloadMechanics method)
    {
        reloadMethods.Remove(method);
    }

    public void AddTakeDamageMethod(Action method)
    {
        takeDamageMethods.Add(method);
    }

    public void RemoveTakeDamageMethod(Action method)
    {
        takeDamageMethods.Remove(method);
    }

    public void TriggerReloadMethods()
    {
        foreach (ReloadMechanics mechanics in reloadMethods)
        {
            mechanics.method(mechanics.methodFloat);
        }
    }

    public void AddHealth(float value)
    {
        if (value + health <= maxHealth)
        {
            health += value;
        }
        else
        {
            health = maxHealth;
            StopCoroutine(Regen());
            StopCoroutine(RegenTimer());
        }

        if (tag == "LocalPlayer")
            Script_UIManager.Instance.healthBar.value = health;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetNameRpc(string name)
    {
        playerName = name;
        GetComponentInChildren<TMP_Text>().text = playerName;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public bool ToggleGodMode()
    {
        godMode = !godMode;

        return godMode;
    }
}
