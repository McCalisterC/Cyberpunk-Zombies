using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class Script_JoinLobbyButton : MonoBehaviour
{

    public void TryJoinLobby()
    {
        Debug.Log("Trying to join lobby");
        SteamMatchmaking.JoinLobbyAsync(Script_SteamGameNetworkManager.instance.selectedLobby.Value.Id);
    }
}
