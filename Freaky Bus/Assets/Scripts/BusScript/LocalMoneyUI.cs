using UnityEngine;
using TMPro;

public class LocalMoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start()
    {
        // Subscribe to the money updates
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateUI;
            UpdateUI(MoneyManager.Instance.money); // Initial sync
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks!
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateUI;
        }
    }

    private void UpdateUI(int currentMoney)
    {
        if (moneyText != null)
            moneyText.text = currentMoney.ToString();
    }
}