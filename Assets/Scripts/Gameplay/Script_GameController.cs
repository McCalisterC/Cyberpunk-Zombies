using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Script_GameController : NetworkBehaviour
{
    [Header("Gameplay Settings")]
    [SerializeField] float spawnIntervals = 1f;
    [SerializeField] int initSpawnAmount = 6;
    [SerializeField] int spawnAmount = 6;
    [SerializeField] int spawnIncreaseIncrements = 2;
    [SerializeField]
    Script_Spawner[] spawns = new Script_Spawner[0];
    [SerializeField] Script_Saloon saloon;
    public GameObject deathTransportPos;

    [Header("Enemy Settings")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject scrapPrefab;

    [Header("UI Settings")]
    [SerializeField] TMP_Text roundUI;
    [SerializeField] TMP_Text roundTimerUI;
    [SerializeField] GameObject networkUI;

    [Header("End Game")]
    [SerializeField] GameObject endGameCamera;
    [SerializeField] TMP_Text gameEndText;

    [Header("Audio")]
    [SerializeField] AudioSource menuMusicSource;
    [SerializeField] AudioSource gameplayMusic;
    [SerializeField] AudioSource gameOverMusic;
    [SerializeField] private float crossoverFadeTime = 3f;

    private int round = 0;
    public int debugNextRound = 0;
    public int GetRound() { return round; }

    private int currentSpawns = 0;
    private int enemiesLeft = 0;
    public bool isTransitioning = false;
    private List<GameObject> players;
    private List<GameObject> alivePlayers;
    public List<GameObject> deadPlayers = new List<GameObject>();
    private bool gameOver = false;
    public bool GetGameOver()
    {
        return gameOver;
    }
    public List<GameObject> GetPlayers() { return alivePlayers; }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectedCallback;
    }

    private void ClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.LocalClientId != 0)
        {
            Script_UIManager.Instance.SwitchToLobbyUI(false);
            NetworkManager.LocalClient.PlayerObject.GetComponent<CharacterController>().enabled = false;
            NetworkManager.LocalClient.PlayerObject.transform.position = new Vector3(NetworkManager.LocalClientId * 2, 10, 0);
            NetworkManager.LocalClient.PlayerObject.GetComponent<CharacterController>().enabled = true;
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    public void StartGameRpc()
    {

        currentSpawns = 0;
        enemiesLeft = 0;
        isTransitioning = false;
        round = 0;

        StartRound();

        StartCoroutine(MixAudioSources(menuMusicSource, true));
        StartCoroutine(MixAudioSources(gameplayMusic, false));

        GameObject.FindGameObjectWithTag("Chat Input").gameObject.SetActive(false);

        Script_UIManager.Instance.ToggleNetworkUI(false);
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Script_OtherControls>().EnableInput();
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Script_BaseStats>().SetNameRpc(SteamClient.Name);
        players = GameObject.FindGameObjectsWithTag("Player").ToList<GameObject>();
        players.Add(GameObject.FindGameObjectWithTag("LocalPlayer"));
        alivePlayers = new List<GameObject>(players);
    }

    public void StartRound()
    {
        if (debugNextRound > 0)
        {
            round = debugNextRound;
            debugNextRound = 0;
        }
        else
        {
            round++;
        }

        if (NetworkManager.IsServer)
        {
            spawnAmount = initSpawnAmount + (spawnIncreaseIncrements * round);
            currentSpawns = 0;
            enemiesLeft = spawnAmount;
            StartCoroutine(SpawnCycle());
            StartRoundRpc(round);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void StartRoundRpc(int newRound)
    {
        roundUI.text = newRound.ToString();
        saloon.GetComponent<Script_Saloon>().ResetScrapCost();
        foreach (GameObject player in deadPlayers)
        {
            player.GetComponent<Script_BaseStats>().Revive(new Vector3(NetworkManager.LocalClientId * 2, 10, 0));
            alivePlayers.Add(player);
        }
        deadPlayers.Clear();
    }

    IEnumerator SpawnCycle()
    {
        if (currentSpawns < spawnAmount)
        {
            Script_Spawner spawner = spawns[Random.Range(0, spawns.Length)];
            if (spawner != null)
            {
                spawner.SpawnEnemy(enemyPrefab);
                currentSpawns++;
            }

            yield return new WaitForSeconds(spawnIntervals);
            StartCoroutine(SpawnCycle());
        }
    }

    [Rpc(SendTo.Server)]
    public void EnemyDeathRpc(NetworkObjectReference enemy, NetworkObjectReference playerThatKilled, int pointsAdded)
    {
        GameObject enemyGameObject = enemy;
        Transform scrapPosition = enemyGameObject.GetComponent<Script_BasicEnemy>().scrapSpawnPoint.transform;

        Debug.Log("Enemy has died");

        GameObject playerCredit = playerThatKilled;
        playerCredit.GetComponent<Script_PlayerUpgrades>().AddPointsRpc(pointsAdded);

        // New: Increment kills for the player who killed the enemy
        playerCredit.GetComponent<Script_PlayerUpgrades>().IncrementKillsRpc();

        // After handling death and points, trigger on-kill mods for the player
        playerCredit.GetComponent<Script_BaseStats>().TriggerOnEnemyKill(enemyGameObject.transform.position);

        enemyGameObject.GetComponent<Script_BasicEnemy>().RagDollRpc();
        if (UnityEngine.Random.Range(1, 10) <= 2)
        {
            Debug.Log("Enemy has dropped scrap");
            SpawnScrapRpc(scrapPosition.position);
        }

        enemiesLeft--;

        if (enemiesLeft <= 0)
        {
            StopCoroutine(SpawnCycle());
            Debug.Log("Round ended");
            StartRoundTransitionRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnScrapRpc(Vector3 position)
    {
        GameObject scrap = Instantiate(scrapPrefab, position, Quaternion.identity);
        scrap.GetComponent<Rigidbody>().AddForceAtPosition((Vector3.up * 2), Random.insideUnitSphere);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void StartRoundTransitionRpc()
    {
        float time = Time.time + 60;
        saloon.DoorToggle(true);
        isTransitioning = true;
        roundTimerUI.enabled = true;
        StartCoroutine(RoundTransition(time));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EndRoundTransitionEarlyRpc()
    {
        if (isTransitioning)
        {
            roundTimerUI.enabled = false;
            isTransitioning = false;
            saloon.DoorToggle(false);
            saloon.KickOutPlayer();
            StartRound();
        }
    }

    IEnumerator RoundTransition(float endTime)
    {
        if (isTransitioning)
        {
            if (Time.time >= endTime)
            {
                roundTimerUI.enabled = false;
                isTransitioning = false;
                saloon.DoorToggle(false);
                saloon.KickOutPlayer();
                StartRound();
            }
            else
            {
                roundTimerUI.text = (endTime - Time.time).ToString("F2");
                yield return new WaitForEndOfFrame();
                StartCoroutine(RoundTransition(endTime));
            }
        }
    }

    public void PlayerDeath(GameObject player)
    {
        if (alivePlayers.Contains(player))
            alivePlayers.Remove(player);
        if (!deadPlayers.Contains(player))
            deadPlayers.Add(player);

        // New: Increment deaths for the player who died
        if (IsServer)
        {
            player.GetComponent<Script_PlayerUpgrades>().IncrementDeathsRpc();
        }

        if (alivePlayers.Count == 0)
        {
            foreach (GameObject deadPlayer in deadPlayers)
            {
                deadPlayer.SetActive(false);
            }

            gameEndText.text = "You survived " + (round - 1) + " waves!";

            gameOver = true;

            StartCoroutine(MixAudioSources(gameplayMusic, true));
            StartCoroutine(MixAudioSources(gameOverMusic, false));
            Script_UIManager.Instance.DisableSpectatorCamera();
            Script_UIManager.Instance.ToggleSpectatorUI(false);

            endGameCamera.GetComponent<CinemachineCamera>().enabled = true;
            endGameCamera.GetComponent<PlayableDirector>().Play();
        }

        else if (GameObject.FindGameObjectWithTag("LocalPlayer") == null)
        {
            Script_UIManager.Instance.CheckSpectatorCamera();
        }
    }

    public void EndGame()
    {
        if (IsServer)
        {
            NetworkManager.SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
        }
    }

    IEnumerator MixAudioSources(AudioSource source, bool current)
    {
        if (current)
        {

            float percentage = 0;
            while (source.volume > 0)
            {
                source.volume = Mathf.Lerp(0.3f, 0, percentage);
                percentage += Time.deltaTime / crossoverFadeTime;
                yield return null;
            }

            source.Stop();
        }

        else
        {
            float percentage = 0;
            source.Play();
            while (source.volume < 0.3)
            {
                source.volume = Mathf.Lerp(0f, 0.3f, percentage);
                percentage += Time.deltaTime / crossoverFadeTime;
                yield return null;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DebugNextRoundRpc(int nextRound)
    {
        debugNextRound = nextRound;
    }

    public void SetSessionInfo(Lobby session)
    {
        Script_SessionHandler.Instance.SetActiveSession(session);
    }
}