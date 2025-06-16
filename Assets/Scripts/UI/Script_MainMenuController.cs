using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Script_MainMenuController : MonoBehaviour
{
    [SerializeField] Button start;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject mainCamera;

    public void StartGame()
    {
        Destroy(mainCamera);
        NetworkManager.Singleton.SceneManager.LoadScene("GameplayScene", UnityEngine.SceneManagement.LoadSceneMode.Single);

        GameObject player = Instantiate(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity); // Adjust spawn position
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, false);
    }

    public void MakeStartInteractable()
    {
        start.interactable = true;
    }
}
