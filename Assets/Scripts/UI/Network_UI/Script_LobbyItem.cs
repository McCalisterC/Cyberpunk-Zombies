using Steamworks.Data;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Script_LobbyItem : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    TMP_Text m_SessionNameText;

    [SerializeField]
    TMP_Text m_SessionPlayersText;

    public UnityEvent<Lobby> OnSessionSelected;

    Lobby lobbyInfo;

    public void SetSession(Lobby _lobby)
    {
        lobbyInfo = _lobby;
        SetSessionName(lobbyInfo.GetData("LobbyName"));
        var currentPlayers = lobbyInfo.MemberCount;
        SetPlayers(currentPlayers, lobbyInfo.MaxMembers);
    }

    void SetSessionName(string sessionName)
    {
        m_SessionNameText.text = sessionName;
    }

    void SetPlayers(int currentPlayers, int maxPlayers)
    {
        m_SessionPlayersText.text = $"{currentPlayers}/{maxPlayers}";
    }

    public void OnSelect(BaseEventData eventData)
    {
        Script_SteamGameNetworkManager.instance.selectedLobby = lobbyInfo;
    }
}
