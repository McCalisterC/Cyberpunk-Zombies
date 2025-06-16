using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;

public class Script_SteamGameNetworkManager : MonoBehaviour
{
    public static Script_SteamGameNetworkManager instance { get; private set; } = null;

    private FacepunchTransport transport = null;

    public Lobby? currentLobby { get; private set; } = null;
    public Lobby? selectedLobby
    {
        get
        {
            return _selectedLobby;
        }

        set
        {
            if (value != null)
            {
                Script_UIManager.Instance.joinLobbyButton.interactable = true;
            }
            else
            {
                Script_UIManager.Instance.joinLobbyButton.interactable = false;
            }

            _selectedLobby = value;
        }

    }

    private Lobby? _selectedLobby;

    public ulong hostId;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void CodeJoin(string codeToJoin)
    {
        if (SteamMatchmaking.LobbyList.WithKeyValue("JoinCode", codeToJoin).RequestAsync().Result.Length > 0)
        {
            Disconnected();
            SteamMatchmaking.LobbyList.WithKeyValue("JoinCode", codeToJoin).RequestAsync().Result[0].Join();
        }
    }

    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;

    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;

        if (NetworkManager.Singleton == null)
        {
            return;
        }
        NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;

    }

    private void OnApplicationQuit()
    {
        Disconnected();
    }

    //when you accept the invite or Join on a friend
    private async void SteamFriends_OnGameLobbyJoinRequested(Lobby _lobby, SteamId _steamId)
    {
        RoomEnter joinedLobby = await _lobby.Join();
        if (joinedLobby != RoomEnter.Success)
        {
            Debug.Log("Failed to create lobby");
        }
        else
        {
            currentLobby = _lobby;
            Debug.Log("Joined Lobby");
        }
    }

    private void SteamMatchmaking_OnLobbyGameCreated(Lobby _lobby, uint _ip, ushort _port, SteamId _steamId)
    {
        Debug.Log("Lobby was created");

    }

    //friend send you an steam invite
    private void SteamMatchmaking_OnLobbyInvite(Friend _steamId, Lobby _lobby)
    {
        Debug.Log($"Invite from {_steamId.Name}");
    }

    private void SteamMatchmaking_OnLobbyMemberLeave(Lobby _lobby, Friend _steamId)
    {
        Debug.Log("member leave: " + _steamId.Name);

        Script_SteamNetworkTransmission.instance.RemoveMeFromDictionaryServerRPC(_steamId.Id);
        Script_UIManager.Instance.UpdateLobbyPlayerList(_lobby);
    }

    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby _lobby, Friend _steamId)
    {
        Debug.Log("member join");
        Script_UIManager.Instance.UpdateLobbyPlayerList(_lobby);
    }

    private void SteamMatchmaking_OnLobbyEntered(Lobby _lobby)
    {
        currentLobby = _lobby;
        Script_UIManager.Instance.SetLobbyInfo(_lobby);
        Script_UIManager.Instance.JoinLobby();
        Script_UIManager.Instance.UpdateLobbyPlayerList(_lobby);
        if (NetworkManager.Singleton.IsHost)
        {
            Script_UIManager.Instance.SwitchToLobbyUI(true);
            return;
        }
        else
        {
            StartClient(currentLobby.Value.Owner.Id);
            Script_UIManager.Instance.SwitchToLobbyUI(false);
        }

    }

    private void SteamMatchmaking_OnLobbyCreated(Result _result, Lobby _lobby)
    {
        if (_result != Result.OK)
        {
            Debug.Log("lobby was not created");
            return;
        }

        string lobbyName = !string.IsNullOrWhiteSpace(Script_UIManager.Instance.GetLobbyName()) ? Script_UIManager.Instance.GetLobbyName() : _lobby.Owner.Name + "'s Lobby";

        _lobby.SetData("LobbyName", lobbyName);
        _lobby.SetData("GameName", "Cyberpunk Zombies");

        _lobby.SetPublic();
        _lobby.SetJoinable(true);
        _lobby.SetGameServer(_lobby.Owner.Id);
        Debug.Log($"lobby created {_lobby.Owner.Name}");
        Script_SteamNetworkTransmission.instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, _lobby.Owner.Name, NetworkManager.Singleton.LocalClientId); //
    }

    public async void StartHost(int _maxMembers)
    {
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
        NetworkManager.Singleton.StartHost();
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxMembers);
    }

    public void StartClient(SteamId _sId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        transport.targetSteamId = _sId;
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client has started");
        }
    }

    public void Disconnected()
    {
        currentLobby?.Leave();
        currentLobby = null;
        Script_UIManager.Instance.LeaveLobby();
        if (NetworkManager.Singleton == null)
        {
            return;
        }
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        }
        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("disconnected");
    }

    private void Singleton_OnClientDisconnectCallback(ulong _cliendId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        if (_cliendId == 0)
        {
            Disconnected();
        }
    }

    private void Singleton_OnClientConnectedCallback(ulong _cliendId)
    {
        Script_SteamNetworkTransmission.instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, SteamClient.Name, _cliendId);
        Script_SteamNetworkTransmission.instance.IsTheClientReadyServerRPC(false, _cliendId);
        Debug.Log($"Client has connected : {SteamClient.Name}");
    }

    private void Singleton_OnServerStarted()
    {
        Debug.Log("Host started");
    }
}