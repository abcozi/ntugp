using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviour
{
	private int M_roundstate;
	private int M_roundtime;
	public static GameManager gameManager;
	public GameObject diceSliderPanel;
	public GameObject actionPointPanel;
	private Player player;
	private int roundstate = 0;
	private int itemfetching = 0;
	private int round = 0;
	private int playerID = 0;
	private int playerTeam = 0;
	private int playerTeamMate = 0;
	private int teamRound = 1;
	private PhotonView photonView;

	void Awake()
	{
		if( gameManager == null )
		{
			gameManager = this;
		}
		playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
		player = GameObject.Find("Player"+playerID.ToString()).GetComponent<Player>();
		playerTeam = player.P_GetTeam();
		playerTeamMate = player.P_GetTeamMate();
		photonView = GetComponent<PhotonView>();
	}
	float tempTime = 0;
	void Update ()
	{
		if( PhotonNetwork.IsMasterClient )
		{
			M_CountingTime();
		}
		if( teamRound != player.P_GetTeam() )
		{
			diceSliderPanel.SetActive(false);
		}
	    if( roundstate == 0 && tempTime > 10 )
	    {
	    	tempTime = 0;
	    	roundstate = 1;
	    	if( teamRound == player.P_GetTeam() )
	    	{
	    		diceSliderPanel.SetActive( false );
	    		actionPointPanel.SetActive( true );
	    		player.P_RowDice(1);
	    	}
	    }
	    if ( roundstate == 1 && tempTime > 20 )
	    {
	        tempTime = 0;
	        roundstate = 0;
	    	M_RoundUpdate();
	    }
	    photonView.RPC("TimeUpdate", RpcTarget.All, tempTime);
	}

	public void M_CountingTime()
	{
		tempTime += Time.deltaTime;
	}
	public void M_RoundUpdate()
	{
		if(PhotonNetwork.IsMasterClient)
		{
			if(teamRound == 1)
			{
				Debug.Log("change teamRound to 2");
				teamRound = 2;
			}
			else
			{
				Debug.Log("change teamRound to 1");
				teamRound = 1;
			}
			photonView.RPC("TeamRoundUpdate", RpcTarget.All, teamRound);
		}
		teamRound = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamRound"];
		if(teamRound == player.P_GetTeam())
		{
			//my team's round
			diceSliderPanel.SetActive( true );
			Debug.Log("my team's round");
		}
		else
		{
			//not my team's round
			player.P_SetActionPoint(0);
			actionPointPanel.SetActive(false);
			Debug.Log("not my team's round");
		}
	}
	
	public void M_Movement( int dir )
	{
		Vector3 location = player.P_GetLocation();
		int tempActionPoint = player.P_GetActionPoint();
		Debug.Log("x: "+location.x+", y: "+location.y+", z: "+location.z);
		//紀錄現在的location
		if( dir == 0 )
		{
			location.z += 1;
			//向前走
		}
		if( dir == 1)
		{
			location.x -= 1;
			//向左走
		}
		if( dir == 2 )
		{
			location.z -= 1;
			//向後走
		}
		if( dir == 3 )
		{
			location.x += 1;
			//向右走
		}
		tempActionPoint -= 1;
		player.P_SetActionPoint( tempActionPoint );
		//消耗actionpoint
		player.P_SetLocation( location );
		//更動Player的Location
	}

	public void M_RowDice( int num )
	{
		int temp = UnityEngine.Random.Range( num, num * 6 );
		M_ChangeDice( -num );
		M_GetPoint( temp );
		tempTime = 10 - tempTime;
		roundstate = 1;
	}

	public void M_ChangeDice( int num )
	{
		int dice = player.P_GetDiceAmount();
		Debug.Log("GameManager:M_ChangeDice: dice: "+dice+", num: "+num);
		//取得P_dice
		dice += num;
		//變更dice
		player.P_SetDiceAmount( dice );
		//變更P_dice
	}

	public void M_GetPoint( int num )
	{
		player.P_SetActionPoint( num );
	}


	public void M_ChangeView( Vector3 wardLocation )
	{
		List<Vector3> temps = player.P_GetWardLocations();
		//儲存p layer的wardlocationlist到temps中
		temps.Add( wardLocation );
		//將要新增的wardLocation新增到temps中
		player.P_SetWardLocations( temps );
		//將player的wardlocationlist改成temps
		M_ChangeWardView( temps );
		//更新視野	
	}

	/*
		Method: M_ChangeView
		呼叫時需輸入要插眼的位置wardLocation
		用P_GetWardLocations暫存player所有ward的list到temps
		將wardLocation加入temps中
		用P_SetWardLocations改變player所有ward的list
		用M_ChangWardView更新View
	*/

	public void M_ChangeWardView( List<Vector3> inputs )
	{
		int[,] temp = new int [25,25];
		foreach( Vector3 input in inputs )
		{
			int x = Convert.ToInt32(Math.Round(input.x));
			int z = Convert.ToInt32(Math.Round(input.z));
			temp = player.P_GetView();
			for(int i = x - 1 ; i <= x + 1 ; i ++)
        	{
            	for(int j = z - 1 ; j <= z + 1 ; j ++)
            	{
            	    //忽略超過地圖範圍的點
            	    if(i >= 0 && j >= 0 && i < 25 && j < 25)
            	    {
            	        temp[i, j] = 3;
            	    }
            	}
        	}
        	//將每個ward周圍九宮格的視野改成3
		}
		player.P_SetView( temp );
	}

    public int M_GetTeamRound()
    {
        return teamRound;
    }
    public int M_GetRound()
    {
        return round;
    }

}
