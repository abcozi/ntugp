using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	private int M_roundstate;
	private int M_roundtime;
	private int M_actionpoint;
	public static GameManager gameManager;
	private Player player;

	void Awake()
	{
		if( gameManager == null )
		{
			gameManager = this;
		}
		player = GameObject.Find("Player").GetComponent<Player>();
	}

	public void M_Movement( int dir )
	{
		Vector3 location = player.P_GetLocation();
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
		M_actionpoint -= 1;
		//消耗actionpoint
		player.P_SetLocation( location );
		//更動Player的Location
	}

	public void M_RowDice( int num )
	{
		int temp = UnityEngine.Random.Range( num, num * 6 );
		M_ChangeDice( -num );
		M_GetPoint( temp );
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

	/*public void PutWard( Vector3 wardlocation )
	{

		return true;
		//回傳可以put ward
	}*/
	public void M_ChangeView(Vector3 wardLocation)
	{
		//code
	}










}
