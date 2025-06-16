using Steamworks.Data;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;
using UnityEngine;

public class Script_SessionHandler : MonoBehaviour
{
    public Lobby? activeSession { get; private set; } = null;

    public static Script_SessionHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetActiveSession(Lobby session)
    {
        activeSession = session;
    }

    public Lobby? GetActiveSession()
    {
        return activeSession;
    }

    public void LeaveActiveSession()
    {
        if (activeSession != null)
        {
            try
            {
                activeSession?.Leave();
            }
            catch
            {
                // Ignored as we are exiting the game
            }
            finally
            {
                activeSession = null;
            }
        }
    }

    public void LockSession()
    {
        return;
    }
}
