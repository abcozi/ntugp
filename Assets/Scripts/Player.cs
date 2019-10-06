/*
	Script Name: Player
	Purpose: Process player activities.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	private Rigidbody p_rigidBody;//get rigidBody of player
	public static Player player;//player object
    public Animator anim;//player's animator
	/* 
		private members(attributes) 
	*/
	private int p_id;//player id
	//private List<Item> p_items = new List<Item>();//item list of a player
	private int p_diceAmount;//amount of player's dices
	private float p_attack; //attack value of a player
	private Vector3 p_location;//player's location
	private int[,] p_view;//player's view. Displayed by a 2d array. -1: never been, 0: have been but not available now, 1: available.
	private List<int[]> p_wardLocations = new List<int[]>();//list of ward locations
	private int p_actionPoint;//record the action point owned by a player
    private bool p_movingMode = false;//define if player is in moving mode
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
		p_diceAmount = 100;//set default dice amount to 100
        p_location = player.transform.position;//set the current player location
	}
    // Start is called before the first frame update
    void Start()
    {
        //get player rigidBody
        p_rigidBody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        anim.Play("WAIT04");
        Debug.Log("Wait04");
    }

    // Update is called once per frame
    void Update()
    {
    	//update movement in every frame if p_actionPoint > 0
    	if(p_actionPoint > 0 && p_movingMode)
    	{
            //Animator plays "walk" animation at the beggining of a frame.
            //After walk animation, go back to wait.
            //anim.Play("WAIT00", -1, 0f);
            //anim.Play("WAIT04", -1, 0f);
    		P_Movement();
    	}
        else
        {
            anim.Play("WAIT00", -1, 0f);
        }
    }
    /*
		Method: P_Movement
		Purpose: wasd控制移動，判斷如果還有p_actionPoint，
		則傳送移動方向給GameManager.M_Movement，
		並且每走一步扣一點actionPoint。
    */
	public void P_Movement()
	{
        //anim.Play("RUN00_F", -1, 0f);
        if( Input.GetKeyDown( KeyCode.W ) )//往前走
	    {
	    	GameManager.gameManager.M_Movement( 0 );
	    	p_actionPoint -= 1;
	    }
	   	if( Input.GetKeyDown( KeyCode.A ) )//往左走
	    {
	    	GameManager.gameManager.M_Movement( 1 );
	   		p_actionPoint -= 1;
	   	}
	   	if( Input.GetKeyDown( KeyCode.S ) )//往後走
	   	{
    		GameManager.gameManager.M_Movement( 2 );	    		
    		p_actionPoint -= 1;
	    }
	    if( Input.GetKeyDown( KeyCode.D ) )//往右走
	    {
	    	GameManager.gameManager.M_Movement( 3 );
	    	p_actionPoint -= 1;
	    }	
        //update ui 上顯示的 action point
        P_UpdateActionPointText();
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
        actionPointText = GameObject.Find("actionPointText").GetComponent<Text>();
        //set the action point display
        actionPointText.text = "Action Point: "+p_actionPoint.ToString()+", Dice Amount: "+p_diceAmount;
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
    public void P_PutWard(int[] wardLocation)
    {
    	GameManager.gameManager.M_ChangeView(wardLocation);
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
    public int P_GetDiceAmount()
    {
    	return p_diceAmount;
    }
    public void P_SetDiceAmount(int dav)
    {
    	p_diceAmount = dav;
    }
    public float P_GetAttack()
    {
    	return p_attack;
    }
    public void P_SetAttack(float attackv)
    {
    	p_attack = attackv;
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
    public List<int[]> P_GetWardLocations()
    {
    	return p_wardLocations;
    }
    public void P_SetWardLocations(List<int[]> wardLocations)
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
}
