using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StorePanel : MonoBehaviour, IPointerDownHandler
{
    private static bool clicked;
    void Start()
    {
        clicked = false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        clicked = true;
    }
    public static bool GetIfClicked()
    {
        return clicked;
    }
    public static void SetClicked(bool clicked)
    {
        StorePanel.clicked = clicked;
    }

}