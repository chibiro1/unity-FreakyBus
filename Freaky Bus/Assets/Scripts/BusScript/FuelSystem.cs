using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class FuelSystem : MonoBehaviour
{
    public float startFuel;
    public float maxFuel = 100f;
    public int refuelCost = 300;
    public float fuelConsumptionRate;
    public Slider fuelIndicatorSId;
    public TMP_Text fuelIndicatorTxt;
    public float currentFuel;

    [Header("Gas Station Interaction")]
    public GameObject refuelButtonUI;   // Assign your UI Button GameObject here
    private bool isAtGasStation = false;
    private bool isRefueling = false;

    void Awake()
    {
        currentFuel = startFuel;
        refuelButtonUI.SetActive(false); // Hide button at start
    }

    void Start()
    {
        if (startFuel > maxFuel)
            startFuel = maxFuel;
        fuelIndicatorSId.maxValue = maxFuel;
        currentFuel = startFuel;
        UpdateUI();
    }

    // Call this from the UI Button OnClick()
    public void OnRefuelButtonPressed()
    {
        if (MoneyManager.Instance == null) return;

        if (MoneyManager.Instance.money < refuelCost)
        {
            Debug.Log("Not enough money to refuel!");
            return;
        }

        MoneyManager.Instance.RemoveMoney(refuelCost);

        isRefueling = true;

        Debug.Log("Refueling started (-300)");
    }

    public void ReduceFuel()
    {
        currentFuel -= Time.deltaTime * fuelConsumptionRate;
        startFuel = currentFuel;
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GasStation"))
        {
            isAtGasStation = true;
            refuelButtonUI.SetActive(true); // Show button when entering
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("GasStation") && isRefueling)
        {
            currentFuel += Time.deltaTime * 0.2f;
            if (currentFuel > maxFuel)
            {
                currentFuel = maxFuel;
                isRefueling = false; // Stop refueling when full
            }
            startFuel = currentFuel;
            UpdateUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GasStation"))
        {
            isAtGasStation = false;
            isRefueling = false;        // Stop refueling when leaving
            refuelButtonUI.SetActive(false); // Hide button when leaving
        }
    }

    void UpdateUI()
    {
        fuelIndicatorSId.value = currentFuel;
        fuelIndicatorTxt.text = "FUEL GAUGE: " + currentFuel.ToString("0") + " %";

        if (currentFuel <= 0)
        {
            currentFuel = 0;
            startFuel = 0;
            fuelIndicatorTxt.text = "OUT OF FUEL!!";
        }
    }
}