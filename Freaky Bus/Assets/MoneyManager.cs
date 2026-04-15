using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int money = 0;
    public TextMeshProUGUI moneyText;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "" + money.ToString();
    }
    public void RemoveMoney(int amount)
    {
        money -= amount;
        if (money < 0) money = 0;
        UpdateUI();
    }
}