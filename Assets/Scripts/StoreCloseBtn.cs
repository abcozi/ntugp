using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class StoreCloseBtn : MonoBehaviour, IPointerDownHandler
{
    private static bool clicked;


    void Start()
    {
        clicked = false;

    }
    public static bool GetIfClicked()
    {
        return clicked;
    }
    public static void SetClicked(bool clicked)
    {
        StoreCloseBtn.clicked = clicked;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        clicked = true;
    }
}
