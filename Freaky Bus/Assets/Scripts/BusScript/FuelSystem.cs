using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FuelSystem : MonoBehaviour
{
    [Header("Fuel Settings")]
    public float startFuel = 100f;
    public float maxFuel = 100f;
    public int refuelCost = 300;
    public float fuelConsumptionRate = 1f;
    public float currentFuel;

    // These are no longer public; they are assigned dynamically
    private Slider fuelIndicatorSId;
    private TMP_Text fuelIndicatorTxt;
    private GameObject refuelButtonUI;
    private Button refuelButton;

    private bool isAtGasStation = false;
    private bool isRefueling = false;
    private bool hasLinkedUI = false;

    void Awake()
    {
        currentFuel = Mathf.Clamp(startFuel, 0, maxFuel);
    }

    // Called dynamically when a player sits down and their UI turns on
    public void LinkUI(BusUIReferences ui)
    {
        fuelIndicatorSId = ui.fuelIndicatorSId;
        fuelIndicatorTxt = ui.fuelIndicatorTxt;
        refuelButtonUI = ui.refuelButtonUI;
        refuelButton = ui.refuelButtonClickable;

        if (refuelButton != null)
        {
            refuelButton.onClick.RemoveAllListeners();
            refuelButton.onClick.AddListener(OnRefuelButtonPressed);
        }

        fuelIndicatorSId.maxValue = maxFuel;
        refuelButtonUI.SetActive(isAtGasStation); // Only show if currently at station
        hasLinkedUI = true;

        UpdateUI();
    }

    // Called dynamically when player leaves seat
    public void UnlinkUI()
    {
        hasLinkedUI = false;
        fuelIndicatorSId = null;
        fuelIndicatorTxt = null;
        
        if (refuelButton != null)
            refuelButton.onClick.RemoveAllListeners();
            
        refuelButtonUI = null;
        refuelButton = null;
    }

    public void OnRefuelButtonPressed()
    {
        if (MoneyManager.Instance == null || MoneyManager.Instance.money < refuelCost)
        {
            Debug.Log("Not enough money to refuel!");
            return;
        }

        MoneyManager.Instance.RemoveMoney(refuelCost);
        isRefueling = true;
    }

    public void ReduceFuel()
    {
        // Don't burn fuel if refueling
        if (isRefueling) return; 

        currentFuel -= Time.deltaTime * fuelConsumptionRate;
        if (currentFuel < 0) currentFuel = 0;
        
        if (hasLinkedUI) UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GasStation"))
        {
            isAtGasStation = true;
            if (hasLinkedUI && refuelButtonUI != null) refuelButtonUI.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("GasStation") && isRefueling)
        {
            currentFuel += Time.deltaTime * 10f; // Adjusted for reasonable fill speed
            if (currentFuel >= maxFuel)
            {
                currentFuel = maxFuel;
                isRefueling = false;
            }
            if (hasLinkedUI) UpdateUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GasStation"))
        {
            isAtGasStation = false;
            isRefueling = false;
            if (hasLinkedUI && refuelButtonUI != null) refuelButtonUI.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        if (!hasLinkedUI) return; // Bulletproof check

        if (fuelIndicatorSId != null) fuelIndicatorSId.value = currentFuel;
        
        if (fuelIndicatorTxt != null)
        {
            if (currentFuel <= 0)
                fuelIndicatorTxt.text = "OUT OF FUEL!!";
            else
                fuelIndicatorTxt.text = "FUEL GAUGE: " + currentFuel.ToString("0") + " %";
        }
    }
}