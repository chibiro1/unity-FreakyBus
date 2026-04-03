using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action OnHoldStart;
    public event Action OnHoldEnd;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnHoldStart?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnHoldEnd?.Invoke();
    }
}