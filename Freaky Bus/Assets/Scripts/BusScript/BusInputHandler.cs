using UnityEngine;
using UnityEngine.InputSystem;


public class BusInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BusController busController;
    [SerializeField] private BusSeatManager busSeatManager;
    [SerializeField] private SteeringWheelUI steeringWheelUI;

    [Header("Mobile Buttons")]
    [SerializeField] private UnityEngine.UI.Button throttleButton;
    [SerializeField] private UnityEngine.UI.Button brakeButton;
    [SerializeField] private UnityEngine.UI.Button exitSeatButton;

    private BusInputActions inputActions;
    private bool isInputEnabled;
    private Vector2 steerInput;
    private bool throttleHeld;
    private bool brakeHeld;

    private void Awake()
    {
        inputActions = new BusInputActions();
    }

    public void EnableInput()
    {
        isInputEnabled = true;
        inputActions.Enable();

        inputActions.Bus.Steer.performed    += OnSteer;
        inputActions.Bus.Steer.canceled     += OnSteer;
        inputActions.Bus.Throttle.performed += ctx => throttleHeld = true;
        inputActions.Bus.Throttle.canceled  += ctx => throttleHeld = false;
        inputActions.Bus.Brake.performed    += ctx => brakeHeld = true;
        inputActions.Bus.Brake.canceled     += ctx => brakeHeld = false;
        inputActions.Bus.ExitVehicle.performed += ctx => busSeatManager.ExitDriverSeat();

        if (throttleButton != null)
        {
            var t = throttleButton.GetComponent<MobileHoldButton>();
            if (t != null)
            {
                t.OnHoldStart += () => throttleHeld = true;
                t.OnHoldEnd   += () => throttleHeld = false;
            }
        }

        if (brakeButton != null)
        {
            var b = brakeButton.GetComponent<MobileHoldButton>();
            if (b != null)
            {
                b.OnHoldStart += () => brakeHeld = true;
                b.OnHoldEnd   += () => brakeHeld = false;
            }
        }

        if (exitSeatButton != null)
            exitSeatButton.onClick.AddListener(busSeatManager.ExitDriverSeat);
    }

    public void DisableInput()
    {
        isInputEnabled = false;

        if (inputActions != null)
        {
            inputActions.Bus.Steer.performed    -= OnSteer;
            inputActions.Bus.Steer.canceled     -= OnSteer;
            inputActions.Bus.Disable();
        }

        throttleHeld = false;
        brakeHeld = false;
        steerInput = Vector2.zero;

        if (exitSeatButton != null)
            exitSeatButton.onClick.RemoveListener(busSeatManager.ExitDriverSeat);

        busController.SetInputs(0f, 0f, false);
    }

    private void Update()
    {
        // Guard — only run when input is enabled
        if (!isInputEnabled) return;

        float steer = steeringWheelUI != null
            ? steeringWheelUI.SteerValue
            : steerInput.x;

        busController.SetInputs(steer, throttleHeld ? 1f : 0f, brakeHeld);
    }

    private void OnSteer(InputAction.CallbackContext ctx)
    {
        steerInput = ctx.ReadValue<Vector2>();
    }
}