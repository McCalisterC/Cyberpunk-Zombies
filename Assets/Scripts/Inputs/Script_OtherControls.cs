using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Script_OtherControls : NetworkBehaviour
{
    public I_Interactable currentInteractable;

    // Input Variables
    [SerializeField] private Input_Controller _input;

    GameObject chatInput;
    GameObject chatText;

    private void Start()
    {
        if (IsLocalPlayer)
        {
            gameObject.tag = "LocalPlayer";

            chatInput = GameObject.FindGameObjectWithTag("Chat Input");
            chatText = GameObject.FindGameObjectWithTag("Chat Text");
        }
    }

    public void EnableInput()
    {
        if (IsLocalPlayer)
        {
            GetComponent<PlayerInput>().enabled = true;
            GetComponent<Input_Controller>().enabled = true;
            GetComponentInChildren<CinemachineCamera>().enabled = true;
            ToggleCursor(true);
            ToggleInput(true);
        }
    }

    public void DisableInput()
    {
        if (IsLocalPlayer)
        {
            GetComponent<PlayerInput>().enabled = false;
            GetComponent<Input_Controller>().enabled = false;
            GetComponentInChildren<CinemachineCamera>().enabled = false;
            ToggleCursor(false);
            ToggleInput(false);
        }
    }

    public void ReactivateCamera()
    {
        Script_UIManager.Instance.DisableSpectatorCamera();
        GetComponentInChildren<CinemachineCamera>().enabled = true;
    }

    private void Update()
    {
        if (_input != null)
        {
            SkipTimer();
            CheckInteract();
            CheckChat();
        }
    }

    public void SkipTimer()
    {
        if (_input.endRound == true)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Script_GameController>().EndRoundTransitionEarlyRpc();
            _input.endRound = false;
        }
    }

    public void CheckInteract()
    {
        if (_input.interact == true)
        {
            Debug.Log("Reached Interact");
            if (currentInteractable != null)
            {
                currentInteractable.OnInteract();
            }
            _input.interact = false;
        }
    }

    public void CheckChat()
    {
        if (_input.chatbox == true)
        {
            chatInput.SetActive(true);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(chatInput);

            _input.SetCursorState(false);
            GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");

            _input.chatbox = false;
        }
    }

    public void ToggleCursor()
    {
        _input.SetCursorState(!_input.cursorLocked);
    }

    public void ToggleCursor(bool toggle)
    {
        _input.SetCursorState(toggle);
    }

    public void ToggleInput(bool input)
    {
        _input.ToggleEnabled(input);
    }
}
