using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagItem : MonoBehaviour, IDragHandler, IDropHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    Texture2D texture, selectTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot;
    private GameManager gameManager;
    private Player player;
    private int playerID = 0;
    private Map map;
    private Vector2 position;


    public static bool Click { get; set; }
    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = Input.mousePosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Click = true;
        if (map.isChoosingItemToDiscard || map.isArbitrarilyDiscard)
        {
            map.DiscardItem(this.gameObject);
        }
        else if (!map.S_GetIfConfirm())
        {
            string resourceName = this.gameObject.GetComponent<RawImage>().texture.name.ToString();
            string ItemID = resourceName.Substring(0, 1).ToLower() + resourceName.Substring(1);
            int removeID = 0;
            if (this.gameObject.transform.parent.name.Substring(12) == "Extra")
            {
                removeID = player.P_GetItemList().Count - 1;
            }
            else 
            {
                int removeNum = int.Parse(this.gameObject.transform.parent.name.Substring(12));
                removeID = removeNum - 1;
            }
            gameManager.M_UsingItem(ItemID, removeID);
        }
    }
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        map = GameObject.Find("Map").GetComponent<Map>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Click = false;
        texture = (Texture2D)Resources.Load("Icon/discard");
        selectTexture = (Texture2D)Resources.Load("Icon/select");
        hotSpot = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(gameManager == null)
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if(map == null)
            map = GameObject.Find("Map").GetComponent<Map>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        texture = (Texture2D)Resources.Load("Icon/discard");
        if (map.isChoosingItemToDiscard || map.isArbitrarilyDiscard)
        {
            hotSpot = new Vector2(texture.width / 2, texture.height / 2);
            Cursor.SetCursor(texture, hotSpot, cursorMode);
        }
        else
        {
            hotSpot = new Vector2(0, texture.height / 3);
            Cursor.SetCursor(selectTexture, hotSpot, cursorMode);
        }
            
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
}
