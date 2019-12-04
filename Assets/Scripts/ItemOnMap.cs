using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemOnMap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //adjust this to change speed
    float speed = 2.5f;
    //adjust this to change how high it goes
    float height = 0.1f;
    float delayTime;
    bool lockUpdate = true;
    Texture2D texture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot;
    private GameManager gameManager;
    private Player player;
    private int playerID = 0;
    private Map map;
    private Vector2 position;


    void Start()
    {
        map = GameObject.Find("Map").GetComponent<Map>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
        player = GameObject.Find("Player" + playerID.ToString()).GetComponent<Player>();
        StartCoroutine(Delay());
        texture = (Texture2D)Resources.Load("Icon/grab");
        hotSpot = new Vector2(texture.width/2, texture.height/2);
        position = new Vector2(this.GetComponent<Transform>().position.x, this.GetComponent<Transform>().position.z);
    }

    IEnumerator Delay()
    {	
    	delayTime = Random.Range(0.0f, 1.0f);
        yield return new WaitForSeconds(delayTime);
        lockUpdate = false;
    }


    void Update()
    {
    	if(!lockUpdate){
    		//get the objects current position and put it in a variable so we can access it later with less code
	        Vector3 pos = transform.position;
	        //calculate what the new Y position will be
	        float newY = Mathf.Sin((Time.time - delayTime) * speed);
	        //set the object's Y to the new calculated Y
	        transform.position = new Vector3(pos.x, newY * height, pos.z) ;
    	}
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool surrounding = map.Surrounding((int)position.x, (int)position.y, (int)player.P_GetLocation().x, (int)player.P_GetLocation().z);
        if (gameManager.M_GetTeamRound() == player.P_GetTeam() && surrounding && player.P_GetActionPoint() != 0 && !map.S_GetIfConfirm() && !map.isChoosingItemToDiscard)
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
