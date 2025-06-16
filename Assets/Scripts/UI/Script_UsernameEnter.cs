using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class Script_UsernameEnter : MonoBehaviour
{
    [SerializeField] Button enterButton;
    [SerializeField] TMP_InputField inputField;

    public void CheckUsernameValid()
    {
        if (!inputField.text.IsNullOrEmpty())
        {
            enterButton.interactable = true;
        }
        else
        {
            enterButton.interactable = false;
        }
    }
    
    public void EnterUsername()
    {
        GameObject.FindGameObjectWithTag("UI Manager").GetComponent<Script_UIManager>().SetLocalUsername(inputField.text);
        gameObject.SetActive(false);
    }
}
