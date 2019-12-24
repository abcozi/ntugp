/*
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
    private int p_viewID;
    private string p_nickName;
	private List<Item> p_items = new List<Item>();//item list of a player
    private List<int> p_itemsTimes = new List<int>();//the times item could be used list of a player
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
    private int p_wardAmountUnused = 0;
    private int p_wardAmountUsed = 0;
    private List<Eye> wards = new List<Eye>();
    private PhotonView photonView;
    private GameObject playerObj;
    private int skill;
    private int order = -1;//random order among players
    private bool moveLock = false;//if lock is true, player cannot move by user
    private bool initialized = false;
    private bool alive = true;
    int usedDiceAmount = 0;
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
        if(anim == null)
        {
            anim = GetComponent<Animator>();    
        }
        if(photonView == null)
        {
            photonView = GetComponent<PhotonView>();
        }
        p_viewID = GetComponent<PhotonView>().ViewID;
    }
    // Start is called before the first frame update
    void Start()
    {
        p_rigidBody = GetComponent<Rigidbody>();
        //if(photonView.IsMine)
        //{
            //get player rigidBody
        if(PhotonNetwork.LocalPlayer.CustomProperties["order"] != null)
        {
            p_team = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"];
            p_teamMate = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamMate"];
            Debug.Log("Initializing player...");
            Debug.Log("my team: "+p_team.ToString()+", mate: "+p_teamMate.ToString());
            setAttrs();
        }
            
            //anim = GetComponent<Animator>();
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
            //playerObj = gameObject;
        //}
            try
                {
                    if(photonView.IsMine)
                    {
                        playAnimation("WAIT04", p_id);
                        //photonView.RPC("TimeUpdate", RpcTarget.All, newTimerVal);
                        photonView.RPC("UpdateDefense", RpcTarget.All, p_id, p_defense);
                        photonView.RPC("UpdateAlive", RpcTarget.All, p_id, alive);
                        photonView.RPC("UpdateDA", RpcTarget.All, p_id, p_diceAmount);
                    }
                }
                catch(System.Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
    }

    // Update is called once per frame
    void Update()
    {

        //if (photonView.IsMine)
        //{
            if (moveLock == null)
            {
                moveLock = false;
            }
            else
            {
                //do nothing
            }
            if(order == -1)
            {
                setAttrs();
            }
            else
            {
                //setAttrs();
            }
            //update movement in every frame if p_actionPoint > 0
            if(p_actionPoint > 0 && p_movingMode && !moveLock)
            {
                //Animator plays "walk" animation at the beggining of a frame.
                //After walk animation, go back to wait.
                //anim.Play("WAIT00", -1, 0f);
                //Debug.Log("walk");
                if (photonView.IsMine) {
                    if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("WALK"))
                        playAnimation("WALK", p_id);

                    P_Movement();
                }
            }
            else if (!Map.map.playerIsDoingSomething)
            {
                if (photonView.IsMine) {
                    if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("WAIT00"))
                        playAnimation("WAIT00", p_id);
                }
            }
            
            try
                {
                    if(photonView.IsMine)
                    {
                        //photonView.RPC("TimeUpdate", RpcTarget.All, newTimerVal);
                        photonView.RPC("UpdateDefense", RpcTarget.All, p_id, p_defense);
                        photonView.RPC("UpdateAlive", RpcTarget.All, p_id, alive);
                        photonView.RPC("UpdateDA", RpcTarget.All, p_id, p_diceAmount);
                    }
                    
                }
                catch(System.Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
                AttackAtempt();
        //}

    }
    public void AttackAtempt()
    {
        if(Input.GetKeyDown(KeyCode.Z) && photonView.IsMine)
        {
            if(p_actionPoint > 3 && GameManager.gameManager.M_GetTeamRound() == p_team)
            {
                int attackNum = 0;
                //attack rivals
                for (int i = 1; i <= 4; i++)
                {
                    if (i == p_id || i == p_teamMate)
                    {
                        continue;
                    }
                    else
                    {
                        try
                        {
                            //rivals
                            Vector3 rivLoc = (Vector3)PhotonNetwork.LocalPlayer.CustomProperties["locPlayer" + i.ToString()];
                            Vector3 myLoc = p_location;
                            if (Vector3.Distance(rivLoc, myLoc) <= Vector3.Distance(new Vector3(0, 0, 0), new Vector3(2, 0, 2)))
                            {
                                attackNum++;
                                //attack
                                int rivDef = (int)PhotonNetwork.LocalPlayer.CustomProperties["def" + i.ToString()];
                                bool rivalAlive = (bool)PhotonNetwork.LocalPlayer.CustomProperties["alive" + i.ToString()];
                                bool quota = (bool)PhotonNetwork.LocalPlayer.CustomProperties["attackQuota"];
                                if (rivDef < p_attack && rivalAlive && quota)
                                {

                                    Debug.Log("attack rival");
                                    photonView.RPC("AttackRivals", RpcTarget.All, i, p_attack - rivDef);
                                    PhotonNetwork.Instantiate("VFX/GotAttackVFX", new Vector3(rivLoc.x, 1.0f, rivLoc.z), Quaternion.identity, 0);
                                    p_actionPoint -= 3;
                                    GameManager.gameManager.M_ShowInfo("成功攻擊敵人，消耗行動點數3點");
                                    PhotonNetwork.LocalPlayer.CustomProperties["attackQuota"] = false;
                                    break;

                                }
                                else if (rivDef >= p_attack && rivalAlive && quota)
                                {
                                    GameManager.gameManager.M_ShowInfo("攻擊力不足");
                                }
                                else if (rivDef < p_attack && rivalAlive && !quota)
                                {
                                    if (GameManager.gameManager.M_GetTeamRound() == p_team)
                                    {
                                        GameManager.gameManager.M_ShowInfo("攻擊冷卻中");
                                    }
                                    else
                                    {
                                        GameManager.gameManager.M_ShowInfo("防守狀態無法攻擊");

                                    }
                                }


                            }
                            else
                            {
                                //not attack
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.Log(ex.ToString());
                        }
                    }
                }
                if (attackNum == 0)
                    GameManager.gameManager.M_ShowInfo("周圍沒有敵人", 1, false);
            }
            else if(p_actionPoint <= 3 && GameManager.gameManager.M_GetTeamRound() == p_team)
                GameManager.gameManager.M_ShowInfo("行動點數不足", 1, false);


        }
    }
    /*
		Method: P_Movement
		Purpose: wasd控制移動，判斷如果還有p_actionPoint，
		則傳送移動方向給GameManager.M_Movement，
		並且每走一步扣一點actionPoint。
    */
    public void setAttrs()
    {
        
        if(PhotonNetwork.LocalPlayer.CustomProperties["order"] != null &&
         (int)PhotonNetwork.LocalPlayer.CustomProperties["order"] > 0)
        {
            p_team = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"];
            p_teamMate = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamMate"];
            order = (int)PhotonNetwork.LocalPlayer.CustomProperties["order"];
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
            Debug.Log("Player Set attrs: order: "+order.ToString()+", dice: "+
            p_diceAmount.ToString()+", attack: "+p_attack.ToString()+", defense: "
            + p_defense.ToString()+", skill: "+skill.ToString());
        }
    }
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
        usedDiceAmount += requestDiceAmount;
        //update ui 上顯示的 action point
        Text diceAmountText = GameObject.Find("diceAmount").GetComponent<Text>();
        diceAmountText.text = p_diceAmount.ToString();
        
        P_UpdateActionPointText();
        PhotonNetwork.LocalPlayer.CustomProperties["roundState"] = 1;
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
    public List<Eye> P_GetWards()
    {
        return wards;
    }
    public void P_SetWards(List<Eye> wards)
    {
        this.wards = wards;
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
    public List<int> P_GetItemTimesList()
    {
        return p_itemsTimes;
    }
    public void P_SetItemTimesList(List<int> li)
    {
        p_itemsTimes = li;
    }
    public void P_AddItem(Item item)
    {
        p_items.Add(item);
    }
    public void P_AddItemTimes(int times)
    {
        p_itemsTimes.Add(times);
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
    public bool playerInMyArea(Vector3 pos)
    {
        bool flag = false;
        List<Vector3> wardLocs = player.P_GetWardLocations();
        foreach(Vector3 loc in wardLocs)
        {
            float dist = Vector3.Distance(loc, pos);
            if(dist < Vector3.Distance(new Vector3(0, 0, 0), new Vector3(1, 1, 0)))
            {
                flag = true;
                return flag;
            }
        }
        float dis = Vector3.Distance(player.transform.position, pos);
        if(dis < Vector3.Distance(new Vector3(0, 0, 0), new Vector3(1, 1, 0)))
        {
            flag = true;
            return flag;
        }
        return flag;
    }
    public bool playerInMyCurrentArea(Vector3 pos)
    {
        bool flag = false;
        float dis = Vector3.Distance(player.transform.position, pos);
        if(dis < Vector3.Distance(new Vector3(0, 0, 0), new Vector3(1, 1, 0)))
        {
            flag = true;
        }
        return flag;
    }
    public int GetUsedDiceAmount()
    {
        return usedDiceAmount;
    }
     public int P_GetWardAmountUnused()
    {
        return p_wardAmountUnused;
    }
    public void P_SetWardAmountUnused(int wardAmountUnused)
    {
        p_wardAmountUnused = wardAmountUnused;
    }
    public int P_GetWardAmountUsed()
    {
        return p_wardAmountUsed;
    }
    public void P_SetWardAmountUsed(int wardAmountUsed)
    {
        p_wardAmountUsed = wardAmountUsed;
    }
    public void playAnimation(string animation, int p_id)
    {
        if (this.p_id == p_id)
            photonView.RPC("playAnimationRPC", RpcTarget.All, animation, p_viewID);
    }
    [PunRPC]
    void playAnimationRPC(string animation, int p_viewID)
    {
        PhotonView.Find(p_viewID).gameObject.GetComponent<Animator>().Play(animation);
    }
}   

