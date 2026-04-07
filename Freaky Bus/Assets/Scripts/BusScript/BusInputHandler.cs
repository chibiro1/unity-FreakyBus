using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BusInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BusController busController;
    [SerializeField] private BusSeatManager busSeatManager;

    private SteeringWheelUI steeringWheelUI;
    private Button throttleButton, brakeButton, reverseButton, exitSeatButton;

    private BusInputActions inputActions;
    private bool isInputEnabled;
    private bool isUIInitialized;

    private Vector2 steerInput;
    private bool throttleHeld, brakeHeld, reverseHeld;

    // Change Detection Variables
    private float lastSentSteer;
    private float lastSentThrottle;
    private bool lastSentBrake;
    private const float sendThreshold = 0.01f; // Minimum change to trigger a network sync

    private void Awake()
    {
        inputActions = new BusInputActions();
        
        inputActions.Bus.Steer.performed += OnSteer;
        inputActions.Bus.Steer.canceled += OnSteer;
        inputActions.Bus.Throttle.performed += _ => throttleHeld = true;
        inputActions.Bus.Throttle.canceled += _ => throttleHeld = false;
        inputActions.Bus.Brake.performed += _ => brakeHeld = true;
        inputActions.Bus.Brake.canceled += _ => brakeHeld = false;
        inputActions.Bus.Reverse.performed += _ => reverseHeld = true;
        inputActions.Bus.Reverse.canceled += _ => reverseHeld = false;
        inputActions.Bus.ExitVehicle.performed += _ => busSeatManager.ExitDriverSeat();
    }

    public void EnableInput()
    {
        if (steeringWheelUI == null)
        {
            BusUIReferences ui = Object.FindFirstObjectByType<BusUIReferences>();
            if (ui != null) InitializeUI(ui);
        }

        isInputEnabled = true;
        inputActions.Enable();
    }

    private void InitializeUI(BusUIReferences ui)
{
    // If already initialized, don't do it again
    if (isUIInitialized) return;

    steeringWheelUI = ui.steeringWheel;
    throttleButton = ui.throttleButton;
    brakeButton = ui.brakeButton;
    reverseButton = ui.reverseButton;
    exitSeatButton = ui.exitSeatButton;

    BindUIEvents();
    isUIInitialized = true; // Mark as done
}

    private void BindUIEvents()
{
    // Throttle
    if (throttleButton != null && throttleButton.TryGetComponent<MobileHoldButton>(out var t))
    {
        t.OnHoldStart += () => throttleHeld = true;
        t.OnHoldEnd += () => throttleHeld = false;
    }

    // Brake
    if (brakeButton != null && brakeButton.TryGetComponent<MobileHoldButton>(out var b))
    {
        b.OnHoldStart += () => brakeHeld = true;
        b.OnHoldEnd += () => brakeHeld = false;
    }

    // Reverse
    if (reverseButton != null && reverseButton.TryGetComponent<MobileHoldButton>(out var r))
    {
        r.OnHoldStart += () => reverseHeld = true;
        r.OnHoldEnd += () => reverseHeld = false;
    }

    // Exit
    if (exitSeatButton != null)
    {
        exitSeatButton.onClick.RemoveAllListeners();
        exitSeatButton.onClick.AddListener(busSeatManager.ExitDriverSeat);
    }
}

    public void DisableInput()
    {
        isInputEnabled = false;
        inputActions.Disable();
        throttleHeld = false; brakeHeld = false; reverseHeld = false;
        steerInput = Vector2.zero;

        // Tell the server to stop the bus immediately
        busController.SetInputs(0f, 0f, false);
    }

    private void Update()
    {
        if (!isInputEnabled) return;

        float steer = steeringWheelUI != null ? steeringWheelUI.SteerValue : steerInput.x;
        float motorInput = (throttleHeld ? 1f : 0f) - (reverseHeld ? 1f : 0f);

        // BULLETPROOF CHECK: Only send to server if the values actually changed
        if (Mathf.Abs(steer - lastSentSteer) > sendThreshold || 
            Mathf.Abs(motorInput - lastSentThrottle) > sendThreshold || 
            brakeHeld != lastSentBrake)
        {
            busController.SetInputs(steer, motorInput, brakeHeld);

            // Store the "last sent" values
            lastSentSteer = steer;
            lastSentThrottle = motorInput;
            lastSentBrake = brakeHeld;
        }
    }

    private void OnSteer(InputAction.CallbackContext ctx) => steerInput = ctx.ReadValue<Vector2>();
}