﻿/*
	Script Name: Player
	Purpose: Process player activities.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class Player : MonoBehaviour
{
    int mapSize = Global.mapSize;
	private Rigidbody p_rigidBody;//get rigidBody of player
	public static Player player;//player object
    public Animator anim;//player's animator
	/* 
		private members(attributes) 
	*/
	private int p_id;//player id
    private string p_nickName;
	private List<Item> p_items = new List<Item>();//item list of a player
	private int p_diceAmount = 40;//amount of player's dices
	private int p_attack = 4; //attack value of a player
    private int p_defense = 3;
    private int p_itemAmountMax = 6;

	private Vector3 p_location;//player's location
    
    //player's view. Displayed by a 2d array. 0:從來沒去過, 1: 去過現在沒視野, 
    //2: 現在的視野(個人狀態：9格會有2，結盟狀態：18格會有2），每次movement都要更新。
    //3: 現在的眼的視野, 如果被拆掉, 改成1。
	private int[,] p_view = new int[15, 15];//map size is 25x25
	
    //private List<int[]> p_wardLocations = new List<int[]>();//list of ward locations
    private List<Vector3> p_wardLocations = new List<Vector3>();//list of ward locations
	private int p_actionPoint = 0;//record the action point owned by a player
    private bool p_movingMode = false;//define if player is in moving mode
	private int p_team;
    private int p_teamMate;
    private int p_wardAmountTotal = 5;
    private int p_wardAmountUnused = 5;
    private int p_wardAmountUsed = 0;
    private List<Eye> wards = new List<Eye>();
    private PhotonView photonView;
    private GameObject playerObj;
    private int skill;
    private int order;//random order among players
    private bool moveLock = false;//if lock is true, player cannot move by user
    /*  
        skill
        1.每三回合可以額外使用一顆免費骰子 
        2.每三回合可以消耗一顆骰子，使下次攻擊將能偷取骰子並將偷取到的骰子均分給自己以及隊友
        3.初始便會有三個隨機綠色道具，之後每五回合會再取得一個藍色以下道具
    */
    /* 
		methods 
	*/
	//Awake is called before Start(). Initialization
	void Awake()
	{
		//if no player currently, set player to this 
		if(player == null)
		{
			player = this;
		}
		p_location = player.transform.position;//set the current player location
	}
    // Start is called before the first frame update
    void Start()
    {
        //get player rigidBody
        Debug.Log("Initializing player...");
        p_team = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"];
        p_teamMate = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamMate"];
        order = (int)PhotonNetwork.LocalPlayer.CustomProperties["order"];
        Debug.Log("my team: "+p_team.ToString()+", mate: "+p_teamMate.ToString());
        
        p_rigidBody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        //anim.Play("WALK00_F", -1, 0f);
        //set player location randomly
        /*
        System.Random rnd = new System.Random();
        int x = rnd.Next(1, mapSize + 1); // creates a number between 1 and 15
        int z = rnd.Next(1, mapSize + 1); // creates a number between 1 and 15
        Debug.Log("x: "+x.ToString()+", z: "+z.ToString());
        P_SetLocation(new Vector3(x, 0, z));
        */
        //set view
        for(int i = 0 ; i < mapSize ; i ++)
        {
            for(int j = 0 ; j < mapSize ; j ++)
            {
                p_view[i, j] = 0;
            }
        }
        anim.Play("WAIT04");
        playerObj = gameObject;
        photonView = GetComponent<PhotonView>();
        if(order%2 == 1)
        {
            p_diceAmount = 40;
            p_attack = 3;
            p_defense = 3;
            p_itemAmountMax = 6;
            skill = 1;
            Debug.Log("sec1");
        }
        else if(order == 2)
        {
            p_diceAmount = 40;
            p_attack = 5;
            p_defense = 2;
            p_itemAmountMax = 6;
            skill = 2;
            Debug.Log("sec2");
        }
        else//order == 4
        {
            p_diceAmount = 40;
            p_attack = 4;
            p_defense = 2;
            p_itemAmountMax = 9;
            skill = 3;
            Debug.Log("sec3");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {
            /*
            Debug.Log("my photonView, id: "+photonView.ViewID.ToString()+", owner: "+photonView.Owner.NickName);
            playerObj.layer = LayerMask.NameToLayer("HideInMinimap");
            Transform transform = playerObj.transform;
            Transform[] childTrans = playerObj.GetComponentsInChildren<Transform>();
              //child is your child transform
            foreach(Transform child in childTrans){
                child.gameObject.layer = LayerMask.NameToLayer("HideInMinimap");
            }
            */
            //photonView.RPC("")
        }
        else
        {
            Debug.Log("not my photonView, id: "+photonView.ViewID.ToString()+", owner: "+photonView.Owner.NickName);
        } 
    	//update movement in every frame if p_actionPoint > 0
    	if(p_actionPoint > 0 && p_movingMode && !moveLock)
    	{
            //Animator plays "walk" animation at the beggining of a frame.
            //After walk animation, go back to wait.
            //anim.Play("WAIT00", -1, 0f);
            //Debug.Log("walk");
            anim.Play("WALK");
    		P_Movement();
    	}
        else
        {
            anim.Play("WAIT00");
        }

        //press Space: put ward at current location
        if(Input.GetKeyDown("space") && p_wardAmountUnused >= 1)
        {
            //check if able to put ward(ward amount >= 1)
            P_PutWard(p_location);
            wards.Add(new Eye(p_id, p_team, p_location));
            p_wardAmountUnused -= 1;
            p_wardAmountUsed += 1;
        }
        //else if(Input.GetKeyDown(KeyCode.Q) && )
    }
    /*
		Method: P_Movement
		Purpose: wasd控制移動，判斷如果還有p_actionPoint，
		則傳送移動方向給GameManager.M_Movement，
		並且每走一步扣一點actionPoint。
    */
	public void P_Movement()
	{
        Vector3 fromLocation = P_GetLocation();
        int x = Convert.ToInt32(Math.Round(fromLocation.x));
        int z = Convert.ToInt32(Math.Round(fromLocation.z));
        //anim.Play("WALK00_F", -1, 0f);
        if( Input.GetKeyDown( KeyCode.W ) && z + 1 <= mapSize - 1)//往前走
	    {
	    	GameManager.gameManager.M_Movement( 0 );
	    }
	   	if( Input.GetKeyDown( KeyCode.A ) && x - 1 >= 0)//往左走
	    {
	    	GameManager.gameManager.M_Movement( 1 );
	   	}
	   	if( Input.GetKeyDown( KeyCode.S ) && z - 1 >= 0 )//往後走
	   	{
    		GameManager.gameManager.M_Movement( 2 );	    		
	    }
	    if( Input.GetKeyDown( KeyCode.D ) && x + 1 <= mapSize -1)//往右走
	    {
	    	GameManager.gameManager.M_Movement( 3 );
	    }	
        //update ui 上顯示的 action point
        P_UpdateActionPointText();
        P_UpdateView(fromLocation, p_location);
	}
    /*
		Method: P_RowDice
		Purpose: 玩家說要摋幾顆骰子，在method內判斷是否合法（自己骰子夠不夠，超過上限的話修成上限），
		並傳給gamemanager的rowDice要用的骰子數量。gameManager的rowDice會藉由Player的
		dice setter更改骰子數量＆並且更改玩家的actionPoint。
    */
    public void P_RowDice(int inputValue)
    {
    	int requestDiceAmount = inputValue;
        //if request more than the amount a player has, set request amount to the current amount.
    	if(requestDiceAmount > p_diceAmount)
    	{
    		requestDiceAmount = p_diceAmount;
    	}
    	//send request dice amount to game manager
    	GameManager.gameManager.M_RowDice(requestDiceAmount);
        //update ui 上顯示的 action point
        Text diceAmountText = GameObject.Find("diceAmount").GetComponent<Text>();
        diceAmountText.text = p_diceAmount.ToString();
        
        P_UpdateActionPointText();
    }
    /*
        Method: P_UpdateActionPointText
        Purpose: 更新ui上顯示的action point
    */
    public void P_UpdateActionPointText()
    {
        //get the game component
        Text actionPointText;
        actionPointText = GameObject.Find("actionPointAmount").GetComponent<Text>();
        //set the action point display
        //actionPointText.text = "Action Point: "+p_actionPoint.ToString()+", Dice Amount: "+p_diceAmount;
        actionPointText.text = p_actionPoint.ToString();
        //check if the player still has action point. if so, stay in the moving mode.
        //if not, set false to p_movingMode.
        if(p_actionPoint > 0)
        {
            p_movingMode = true;
        }
        else
        {
            p_movingMode = false;
        }
    }
    /*
		Method: P_PutWard
		向GameManager的ChangeView傳遞要放眼的位置
		GameManager的ChangeView中判定是否能放眼
		if true，GameManager.ChangeView繼續更新ward location lists.
		並且呼叫player的view setter。
    */
    public void P_PutWard(Vector3 wardLocation)
    {
    	GameManager.gameManager.M_ChangeView(wardLocation);
    }
    public void P_UpdateView(Vector3 fromLocation, Vector3 pCurrentLocation)
    {
        int oldX = Convert.ToInt32(Math.Round(fromLocation.x));
        int oldZ = Convert.ToInt32(Math.Round(fromLocation.z));
        int x = Convert.ToInt32(Math.Round(pCurrentLocation.x));
        int z = Convert.ToInt32(Math.Round(pCurrentLocation.z));
        //將上個位置到過的地方設為1
        for(int i = oldX - 1 ; i <= oldX + 1 ; i ++)
        {
            for(int j = oldZ - 1 ; j <= oldZ + 1 ; j ++)
            {
                //忽略超過地圖範圍的點&眼的可視範圍
                if(i >= 0 && j >= 0 && i < mapSize && j < mapSize && p_view[i, j] != 3)
                {
                    p_view[i, j] = 1;
                }
            }
        }
        //將現在所在位置的視野設為2
        for(int i = x - 1 ; i <= x + 1 ; i ++)
        {
            for(int j = z - 1 ; j <= z + 1 ; j ++)
            {
                //忽略超過地圖範圍的點&眼的可視範圍
                if(i >= 0 && j >= 0 && i < mapSize && j < mapSize && p_view[i, j] != 3)
                {
                    p_view[i, j] = 2;
                }
            }
        }
        /*Debug.Log("View updated: ");*/
        string logMessage = "   [0] [1] [2] [3] [4] [5] [6] [7] [8] [9] [10] [11] "+
        "[12] [13] [14] \n";
        for(int i = 0 ; i < mapSize ; i ++)
        {
            logMessage +="["+i+"] ";
            for(int j = 0 ; j < mapSize ; j ++)
            {
                logMessage += p_view[i,j]+", ";
            }
            logMessage += "\n";
        }
        /*Debug.Log(logMessage);*/
    }
    /*
		Getters And Setters
    */
    public int P_GetId()
    {
    	return p_id;
    }
    public void P_SetId(int idv)
    {
    	p_id = idv;
    }
    public string P_GetNickName()
    {
        return p_nickName;
    }
    public void P_SetNickName(string nickName)
    {
        p_nickName = nickName;
    }
    public int P_GetDiceAmount()
    {
    	return p_diceAmount;
    }
    public void P_SetDiceAmount(int dav)
    {
    	p_diceAmount = dav;
    }
    public int P_GetAttack()
    {
    	return p_attack;
    }
    public void P_SetAttack(int attackv)
    {
    	p_attack = attackv;
    }
    public int P_GetDefense()
    {
        return p_defense;
    }
    public void P_SetDefense(int d)
    {
        p_defense = d;
    }
    public int P_GetItemAmountMax()
    {
        return p_itemAmountMax;
    }
    public void P_SetItemAmountMax(int maxV)
    {
        p_itemAmountMax = maxV;
    }
    public Vector3 P_GetLocation()
    {
    	return p_location;
    }
    public void P_SetLocation(Vector3 locationv)
    {
    	p_location = locationv;
        player.transform.position = p_location;
        //make the player face the movement direction
        
        float moveHorizontal = Input.GetAxisRaw ("Horizontal");
        float moveVertical = Input.GetAxisRaw ("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        player.transform.rotation = Quaternion.LookRotation(movement);
        player.transform.Translate (movement * 1 * Time.deltaTime, Space.World);
        
    }
    public int[,] P_GetView()
    {
    	return p_view;
    }
    public void P_SetView(int[,] viewv)
    {
    	p_view = viewv;
    }
    public List<Vector3> P_GetWardLocations()
    {
    	return p_wardLocations;
    }
    public void P_SetWardLocations(List<Vector3> wardLocations)
    {
    	p_wardLocations = wardLocations;
    }
    public int P_GetActionPoint()
    {
    	return p_actionPoint;
    }
    public void P_SetActionPoint(int actionPointV)
    {
    	p_actionPoint = actionPointV;
    }
    public int P_GetTeam()
    {
        return p_team;
    }
    public int P_GetTeamMate()
    {
        return p_teamMate;
    }
    public List<Item> P_GetItemList()
    {
        return p_items;
    }
    public void P_SetItemList(List<Item> li)
    {
        p_items = li;
    }
    public void P_AddItem(Item item)
    {
        p_items.Add(item);
    }
    public int P_GetItemListSize()
    {
        return p_items.Count;
    }
    public int P_GetSkill()
    {
        return skill;
    }
    public void P_SetSkill(int skillV)
    {
        skill = skillV;
    }
    public bool P_GetMoveLock()
    {
        return moveLock;
    }
    public void P_SetMoveLock(bool moveLock)
    {
        this.moveLock = moveLock;
    }
}   

