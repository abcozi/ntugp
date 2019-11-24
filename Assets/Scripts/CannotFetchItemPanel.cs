using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;

public class CannotFetchItemPanel : MonoBehaviour, IPointerDownHandler
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
        CannotFetchItemPanel.clicked = clicked;
    }
}
