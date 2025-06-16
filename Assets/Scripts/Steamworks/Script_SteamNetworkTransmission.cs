using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;

public class Script_SteamNetworkTransmission : NetworkBehaviour
{
    public static Script_SteamNetworkTransmission instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void IWishToSendAChatServerRPC(string _message, ulong _fromWho)
    {
        ChatFromServerClientRPC(_message, _fromWho);
    }

    [ClientRpc]
    private void ChatFromServerClientRPC(string _message, ulong _fromWho)
    {
        return;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMeToDictionaryServerRPC(ulong _steamId, string _steamName, ulong _clientId)
    {
        return;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveMeFromDictionaryServerRPC(ulong _steamId)
    {
        RemovePlayerFromDictionaryClientRPC(_steamId);
    }

    [ClientRpc]
    private void RemovePlayerFromDictionaryClientRPC(ulong _steamId)
    {
        Debug.Log("removing client");
    }

    [ClientRpc]
    public void UpdateClientsPlayerInfoClientRPC(ulong _steamId, string _steamName, ulong _clientId)
    {
        return;
    }

    [ServerRpc(RequireOwnership = false)]
    public void IsTheClientReadyServerRPC(bool _ready, ulong _clientId)
    {
        AClientMightBeReadyClientRPC(_ready, _clientId);
    }

    [ClientRpc]
    private void AClientMightBeReadyClientRPC(bool _ready, ulong _clientId)
    {
        return;
    }
}