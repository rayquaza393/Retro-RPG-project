using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace InputSystem
{
	public partial class InputCallbacks : MonoBehaviour
	{
		[Tooltip("Gives info regarding movemnt")]
		public UnityEvent<Vector2> InfoMove;
		[Tooltip("Gives info regarding where camera looks")]
		public UnityEvent<Vector2> InfoLook;
		[Tooltip("Gives info regarding start of the jump")]
		public UnityEvent<bool> InfoJump;
		[Tooltip("Gives info related running")]
		public UnityEvent<bool> InfoSprint;
		[Tooltip("Info about control through mouse")]
		public UnityEvent<bool> InfoControlThroughMouse;

		[Space]
		[Tooltip("Enable analog mvements")]
		public bool analogMovement;

		public Vector2 Move { set => InfoMove?.Invoke(value); }
		public Vector2 Look { set => InfoLook?.Invoke(value); }
		public bool Jump { set => InfoJump?.Invoke(value); }
		public bool Sprint { set => InfoSprint?.Invoke(value); }

		public bool ControlThroughMouse { set => InfoControlThroughMouse?.Invoke(value); }

		public void OnMove(InputAction.CallbackContext context) => Move = analogMovement ? context.ReadValue<Vector2>().normalized : context.ReadValue<Vector2>();

		public void OnLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();

		public void OnJump(InputAction.CallbackContext context) => Jump = context.phase == InputActionPhase.Started;

		public void OnSprint(InputAction.CallbackContext context) => Sprint = context.phase == InputActionPhase.Performed;

		public void OnControlsChanged(PlayerInput playerInput) => ControlThroughMouse = playerInput.currentControlScheme == "KeyboardMouse";
    }
}