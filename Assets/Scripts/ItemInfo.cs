using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Map map;

    public void OnPointerEnter(PointerEventData eventData)
    {
        map.ShowItemInfo(this.gameObject);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        map.CloseItemInfo();

    }
   

    void Awake()
    {
        map = GameObject.Find("Map").GetComponent<Map>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (map == null)
            map = GameObject.Find("Map").GetComponent<Map>();
    }
}

