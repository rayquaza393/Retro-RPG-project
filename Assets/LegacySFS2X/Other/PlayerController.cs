using UnityEngine;
using UnityEngine.InputSystem;

namespace SmartFoxServer.Unity.Examples
{
    public class PlayerController : MonoBehaviour
    {
        const float FORWARD_SPEED = 10;
        const float ROTATION_SPEED = 40;

        private Vector3 lowerLimit;
        private Vector3 higherLimit;

        public bool MovementDirty { get; set; }
        private Vector2 movementInput;

        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private GameSceneController gameSceneController; 

        void Awake()
        {
            if (gameSceneController == null)
            {
                gameSceneController = Object.FindFirstObjectByType<GameSceneController>();
            }

            if (gameSceneController != null && gameSceneController.inputActionAsset != null)
            {
                moveAction = InputActionReference.Create(gameSceneController.inputActionAsset.FindAction("Player/Move"));
            }
            else
            {
                Debug.LogError("GameSceneController or inputActionAsset is not set.");
            }

            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
        }

        void Start()
        {
            MovementDirty = false;
        }

        void Update()
        {
           
            if (movementInput.y != 0)
            {
                this.transform.Translate(0, 0, movementInput.y * Time.deltaTime * FORWARD_SPEED);

                Vector3 pos = this.transform.position;
                this.transform.position = new Vector3(
                    Mathf.Clamp(pos.x, lowerLimit.x, higherLimit.x),
                    pos.y,
                    Mathf.Clamp(pos.z, lowerLimit.z, higherLimit.z));

                MovementDirty = true;
            }

            if (movementInput.x != 0)
            {
                this.transform.Rotate(Vector3.up, movementInput.x * Time.deltaTime * ROTATION_SPEED);
                MovementDirty = true;
            }
            
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                movementInput = Vector2.zero; 
            }
            else
            {
                movementInput = context.ReadValue<Vector2>();
            }
        }

        public void SetLimits(float minX, float minZ, float maxX, float maxZ)
        {
            lowerLimit = new Vector3(minX, 0, minZ);
            higherLimit = new Vector3(maxX, 0, maxZ);
        }

        void OnEnable() => moveAction.action.Enable();
        void OnDisable() => moveAction.action.Disable();
    }
}