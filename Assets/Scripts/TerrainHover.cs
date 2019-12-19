using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TerrainHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Texture2D texture, trapTexture, destroyPotalTexture;
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
        texture = (Texture2D)Resources.Load("Icon/fetchItem");
        trapTexture = (Texture2D)Resources.Load("Icon/trap");
        destroyPotalTexture = (Texture2D)Resources.Load("Icon/destroyPortal");
        position = new Vector2(this.GetComponent<Transform>().position.x, this.GetComponent<Transform>().position.z);
        hotSpot = new Vector2(texture.width/8, texture.height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        bool surrounding = map.Surrounding((int)position.x, (int)position.y, (int)player.P_GetLocation().x, (int)player.P_GetLocation().z);
        if ((gameManager.M_GetRound() > 4 && gameManager.M_GetTeamRound() != player.P_GetTeam()) && (map.S_GetTerrain((int)position.x, (int)position.y) == 1 || map.S_GetTerrain((int)position.x, (int)position.y) == 4)
            && surrounding && !map.S_GetIfIsFetchingItem() && !map.S_GetIfConfirm() && !map.isChoosingItemToDiscard)
        {
            hotSpot = new Vector2(texture.width / 8, texture.height);
            Cursor.SetCursor(texture, hotSpot, cursorMode);
        }
        if ((gameManager.M_GetRound() <= 4 ||gameManager.M_GetTeamRound() == player.P_GetTeam()) && (map.S_GetTerrain((int)position.x, (int)position.y) == -4) && surrounding && !map.S_GetIfConfirm() 
            && player.P_GetActionPoint() > 0 && !map.isChoosingItemToDiscard)
        {
            hotSpot = new Vector2(texture.width / 1.5f, texture.height* 0.2f);
            Cursor.SetCursor(trapTexture, hotSpot, cursorMode);
            
        }
        if ((gameManager.M_GetRound() <= 4 || gameManager.M_GetTeamRound() == player.P_GetTeam()) && (map.S_GetTerrain((int)position.x, (int)position.y) == 6) && surrounding && !map.S_GetIfConfirm() 
            && player.P_GetActionPoint() > 0 && !map.isChoosingItemToDiscard)
        {
            hotSpot = new Vector2(texture.width / 1.5f, texture.height * 0.2f);
            Cursor.SetCursor(destroyPotalTexture, hotSpot, cursorMode);

        }

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
