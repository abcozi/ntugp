using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class barrenHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Texture2D texture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot;
    private Map map;
    private Vector2 position;
    private Player player;
    private int playerID = 0;

    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.Find("Map").GetComponent<Map>();
        texture = (Texture2D)Resources.Load("Icon/information");
        hotSpot = new Vector2(texture.width / 2, texture.height);
        playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
        player = GameObject.Find("Player" + playerID.ToString()).GetComponent<Player>();
        position = new Vector2(this.GetComponent<Transform>().position.x, this.GetComponent<Transform>().position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool surrounding = map.Surrounding((int)position.x, (int)position.y, (int)player.P_GetLocation().x, (int)player.P_GetLocation().z);
        if (surrounding && !map.S_GetIfIsFetchingItem() && !map.S_GetIfConfirm() && !map.isChoosingItemToDiscard)
            Cursor.SetCursor(texture, hotSpot, cursorMode);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    void OnDestroy()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
}
