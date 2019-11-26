using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CanFetchCloseBtn : MonoBehaviour, IPointerDownHandler
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
        CanFetchCloseBtn.clicked = clicked;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        clicked = true;
    }
}
