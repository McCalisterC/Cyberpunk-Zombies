using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Script_LobbyPlayerList : MonoBehaviour
{
    [SerializeField] GameObject playerUIPrefab;

    private List<GameObject> players = new List<GameObject>();
    private bool isUpdating = false;

    public void UpdatePlayerList(Lobby _lobby)
    {
        if (isUpdating)
        {
            
            return;
        }
        else
        {
            isUpdating = true;
        }

        foreach (var player in players)
        {
            Destroy(player.gameObject);
        }
        players.Clear();

        foreach (var player in _lobby.Members)
        {
            Debug.Log($"Adding {player.Name} to list ");
            GameObject playerInfo = Instantiate(playerUIPrefab, gameObject.transform);

            Debug.Log("Reached avatar convertion");
            Texture2D playerAvatar = CovertIcon(GetAvatar(player).Result.Value);

            Debug.Log("Resolved avatar");
            playerInfo.GetComponent<Script_PlayerInfoUIInit>().Init(player.Name, playerAvatar);
            players.Add(playerInfo);
        }

        isUpdating = false;
    }

    public void ResetLobbyPlayerList()
    {

    }

    private static async Task<Image?> GetAvatar(Friend playerAvatar)
    {
        try
        {
            // Get Avatar using await
            return await SteamFriends.GetLargeAvatarAsync(playerAvatar.Id);
        }
        catch (Exception e)
        {
            // If something goes wrong, log it
            Debug.Log(e);
            return null;
        }
    }

    public static Texture2D CovertIcon(Image image)
    {
        // Create a new Texture2D
        var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);

        // Set filter type, or else its really blury
        avatar.filterMode = FilterMode.Trilinear;

        // Flip image
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x, y);
                avatar.SetPixel(x, (int)image.Height - y, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }

        avatar.Apply();
        return avatar;
    }
}
