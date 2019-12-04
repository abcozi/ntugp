using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class BagBeClick : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static bool Click { get; set; }
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot;

    public void OnPointerDown(PointerEventData eventData)
    {
        Click = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        Click = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}
