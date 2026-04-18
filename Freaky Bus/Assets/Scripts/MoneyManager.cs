using UnityEngine;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int money = 0;
    
    // Bulletproof: An event that broadcasts when money changes
    public event Action<int> OnMoneyChanged; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddMoney(int amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money); // Tell anyone listening that money changed
    }

    public void RemoveMoney(int amount)
    {
        money -= amount;
        if (money < 0) money = 0;
        OnMoneyChanged?.Invoke(money);
    }
}