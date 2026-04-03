using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;


public class BusInputHandler : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private BusController busController;
    [SerializeField] private SteeringWheelUI steeringWheelUI;

    [Header("Mobile Buttons")]
    [SerializeField] private UnityEngine.UI.Button throttleButton;
    [SerializeField] private UnityEngine.UI.Button brakeButton;
    [SerializeField] private UnityEngine.UI.Button exitButton;

    private BusInputActions inputActions;

    private Vector2 steerInput;
    private bool throttleHeld;
    private bool brakeHeld;

    private void Awake()
    {
        inputActions = new BusInputActions();
    }

    public void EnableInput()
    {
        inputActions.Enable();

        inputActions.Bus.Steer.performed    += OnSteer;
        inputActions.Bus.Steer.canceled     += OnSteer;
        inputActions.Bus.Throttle.performed += ctx => throttleHeld = true;
        inputActions.Bus.Throttle.canceled  += ctx => throttleHeld = false;
        inputActions.Bus.Brake.performed    += ctx => brakeHeld = true;
        inputActions.Bus.Brake.canceled     += ctx => brakeHeld = false;
        inputActions.Bus.ExitVehicle.performed += OnExit;

        if (throttleButton != null)
        {
            var throttleTrigger = throttleButton.GetComponent<MobileHoldButton>();
            if (throttleTrigger != null)
            {
                throttleTrigger.OnHoldStart += () => throttleHeld = true;
                throttleTrigger.OnHoldEnd   += () => throttleHeld = false;
            }
        }

        if (brakeButton != null)
        {
            var brakeTrigger = brakeButton.GetComponent<MobileHoldButton>();
            if (brakeTrigger != null)
            {
                brakeTrigger.OnHoldStart += () => brakeHeld = true;
                brakeTrigger.OnHoldEnd   += () => brakeHeld = false;
            }
        }

        if (exitButton != null)
            exitButton.onClick.AddListener(OnMobileExit);
    }

    public void DisableInput()
    {
        inputActions.Bus.Steer.performed    -= OnSteer;
        inputActions.Bus.Steer.canceled     -= OnSteer;
        inputActions.Bus.ExitVehicle.performed -= OnExit;
        inputActions.Bus.Disable();
        inputActions.Dispose();

        throttleHeld = false;
        brakeHeld = false;
        steerInput = Vector2.zero;

        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnMobileExit);
    }

    private void Update()
    {
        if (!IsOwner) return;

        float steer = steeringWheelUI != null
            ? steeringWheelUI.SteerValue
            : steerInput.x;

        float throttle = throttleHeld ? 1f : 0f;

        busController.SetInputs(steer, throttle, brakeHeld);
    }

    private void OnSteer(InputAction.CallbackContext ctx)
    {
        steerInput = ctx.ReadValue<Vector2>();
    }

    private void OnExit(InputAction.CallbackContext ctx)
    {
        TriggerExit();
    }

    private void OnMobileExit()
    {
        TriggerExit();
    }

    private void TriggerExit()
    {
        BusSeatManager busSeatManager = GetComponent<BusSeatManager>();
        if (busSeatManager != null)
            busSeatManager.RequestExit();
    }
}