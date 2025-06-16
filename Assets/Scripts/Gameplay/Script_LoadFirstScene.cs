using UnityEngine;
using UnityEngine.SceneManagement;

public class Script_LoadFirstScene : MonoBehaviour
{
    public void Start()
    {
        SceneManager.LoadScene("GameplayScene");
    }
}
