using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Multiplayer.Widgets;
using Unity.Services.Multiplayer;
using UnityEngine;

public class Script_LobbyList : MonoBehaviour
{
    [SerializeField]
    GameObject m_SessionItemPrefab;

    [SerializeField]
    GameObject m_ContentParent;

    IList<GameObject> m_ListItems = new List<GameObject>();
    IList<Lobby> lobbies = new List<Lobby>();

    private void Start()
    {
        RefreshSessionList();
        Script_SteamGameNetworkManager.instance.selectedLobby = null;
    }

    public async void RefreshSessionList()
    {
        await UpdateSessions();

        foreach (var listItem in m_ListItems)
        {
            Destroy(listItem);
        }

        if (lobbies == null || lobbies.Count == 0)
        {
            return;
        }

        foreach (var sessionInfo in lobbies)
        {
            var itemPrefab = Instantiate(m_SessionItemPrefab, m_ContentParent.transform);
            if (itemPrefab.TryGetComponent<Script_LobbyItem>(out var sessionItem))
            {
                sessionItem.SetSession(sessionInfo);
                sessionItem.OnSessionSelected.AddListener(SelectSession);
            }
            m_ListItems.Add(itemPrefab);
        }
    }

    async Task UpdateSessions()
    {
        lobbies = await SteamMatchmaking.LobbyList.WithKeyValue("GameName", "Cyberpunk Zombies").WithSlotsAvailable(1).RequestAsync();
    }

    void SelectSession(Lobby sessionInfo)
    {
        Script_SteamGameNetworkManager.instance.selectedLobby = sessionInfo;
    }
}
