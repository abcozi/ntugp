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
   
    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.Find("Map").GetComponent<Map>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

