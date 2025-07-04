using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

	public class Input_Controller : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Space(10)]
		public bool fire;
		public bool reload;

		[Space(10)]
		public bool interact;
		public bool endRound;

		[Header("UI Controls")]  // New section for clarity
		public bool showStats;   // New: Toggles the player stats UI
		public bool chatbox;

		public bool grapple; // New: Grapple hook input

		[Space(10)]

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = false;
		public bool cursorInputForLook = true;

		private void Awake()
		{
			Cursor.lockState = CursorLockMode.None;
		}

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnFire(InputValue value){
			FireInput(value.isPressed);
		}

		public void OnReload(InputValue value){
			ReloadInput(value.isPressed);
		}

		public void OnEndRoundEarly(InputValue value)
		{
			EndRoundInput(value.isPressed);
		}

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

		public void OnChatBox(InputValue value)
		{
			ChatboxInput(value.isPressed);
		}

		// New: Handler for ShowStats input action (add "ShowStats" action in your Input Action asset)
		public void OnShowStats(InputValue value)
		{
			StatsInput(value.isPressed);
		}

		// New: Handler for Grapple input action (you'll need to add "Grapple" action in your Input Action asset mapped to Q key)
		public void OnGrapple(InputValue value)
		{
			GrappleInput(value.isPressed);
		}
#endif


    public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void FireInput(bool newFireState)
		{
			fire = newFireState;
		}

		public void ReloadInput(bool newReloadState){
			reload = newReloadState;
		}

		public void EndRoundInput(bool newEndRoundState)
		{
			endRound = newEndRoundState;
		}

		public void InteractInput(bool newInteractState)
		{
			interact = newInteractState;
		}

		public void ChatboxInput(bool newInteractState)
		{
			chatbox = newInteractState;
		}

		public void StatsInput(bool newInteractState)
		{
			showStats = newInteractState;
		}

    private void OnApplicationFocus(bool hasFocus)
			{
				if (Cursor.lockState == CursorLockMode.Locked)
					SetCursorState(cursorLocked);
			}

		public void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
			cursorLocked = newState;
		}

		public void ToggleEnabled(bool enabled)
		{
	        if (enabled)
			{
				GetComponent<PlayerInput>().ActivateInput();
			}
			else
				GetComponent<PlayerInput>().DeactivateInput();
		}

		public void GrappleInput(bool newGrappleState)
		{
			grapple = newGrappleState;
		}
	}