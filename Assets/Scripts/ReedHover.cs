using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReedHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Texture2D texture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot;
    private GameManager gameManager;
    private Player player;
    private int playerID = 0;
    private Map map;
    private Vector2 position;
    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.Find("Map").GetComponent<Map>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
        player = GameObject.Find("Player" + playerID.ToString()).GetComponent<Player>();
        texture = (Texture2D)Resources.Load("Icon/search");
        position = new Vector2(this.GetComponent<Transform>().position.x, this.GetComponent<Transform>().position.z);
        hotSpot = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        bool surrounding = map.Surrounding((int)position.x, (int)position.y, (int)player.P_GetLocation().x, (int)player.P_GetLocation().z);
        if ((gameManager.M_GetTeamRound() == player.P_GetTeam()) && surrounding && player.P_GetActionPoint() != 0 && !map.S_GetIfConfirm() && !map.isChoosingItemToDiscard)
            Cursor.SetCursor(texture, hotSpot, cursorMode);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
}
