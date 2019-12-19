using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RPC : MonoBehaviour
{
    [PunRPC]
	void CharacterListingUpdate(bool IsChosen, int selectedPlayerId, int imgID, string playerName)
	{
		CharacterButton listing = GameObject.Find("CharacterListing"+imgID.ToString()).GetComponent<CharacterButton>();
		Text thisPlayerName = GameObject.Find("SelectedPlayerName"+imgID.ToString()).GetComponent<Text>();
		if(IsChosen)
		{
			//disable button
			listing.IsChosen = true;
			listing.selectedPlayerId = selectedPlayerId;
			thisPlayerName.text = playerName;
		}
		else
		{
			//enable button
			listing.IsChosen = false;
			listing.selectedPlayerId = -1;
			thisPlayerName.text = "";
		}
	}
	[PunRPC]
	void SetTeams(int num1, int num2, int num3, int num4)
	{
		
		if(PhotonNetwork.LocalPlayer.CustomProperties["order"] == null || 
			(PhotonNetwork.LocalPlayer.CustomProperties["order"] != null && 
				(int)PhotonNetwork.LocalPlayer.CustomProperties["order"] <= 0))
		{
			if(num1 == (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"])
			{
				PhotonNetwork.LocalPlayer.CustomProperties["team"] = 1;
				PhotonNetwork.LocalPlayer.CustomProperties["teamMate"] = num2;
				PhotonNetwork.LocalPlayer.CustomProperties["order"] = 1;
			}
			else if(num2 == (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"])
			{
				PhotonNetwork.LocalPlayer.CustomProperties["team"] = 1;
				PhotonNetwork.LocalPlayer.CustomProperties["teamMate"] = num1;
				PhotonNetwork.LocalPlayer.CustomProperties["order"] = 2;
			}
			else
			{
				if(num3 == (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"])
				{
					PhotonNetwork.LocalPlayer.CustomProperties["teamMate"] = num4;
					PhotonNetwork.LocalPlayer.CustomProperties["order"] = 3;
				}
				else
				{
					PhotonNetwork.LocalPlayer.CustomProperties["teamMate"] = num3;
					PhotonNetwork.LocalPlayer.CustomProperties["order"] = 4;
				}
				PhotonNetwork.LocalPlayer.CustomProperties["team"] = 2;
			}
		}
		if(num1 == (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"])
		{
			PhotonNetwork.LocalPlayer.CustomProperties["team"] = 1;
			PhotonNetwork.LocalPlayer.CustomProperties["teamMate"] = num2;
		}
		else if(num2 == (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"])
		{
			PhotonNetwork.LocalPlayer.CustomProperties["team"] = 1;
			PhotonNetwork.LocalPlayer.CustomProperties["teamMate"] = num1;
		}
		else
		{
			if(num3 == (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"])
			{
				PhotonNetwork.LocalPlayer.CustomProperties["teamMate"] = num4;
			}
			else
			{
				PhotonNetwork.LocalPlayer.CustomProperties["teamMate"] = num3;
			}
			PhotonNetwork.LocalPlayer.CustomProperties["team"] = 2;
		}
	}
	[PunRPC]
	void TeamRoundUpdate(int team, int round)
	{
		//Debug.Log("teamRoundUpdate; this teamRound: "+team.ToString());
		if(round <= 4)
		{
			PhotonNetwork.LocalPlayer.CustomProperties["teamRound"] = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"];
		}
		else
		{
			PhotonNetwork.LocalPlayer.CustomProperties["teamRound"] = team;
		}
		PhotonNetwork.LocalPlayer.CustomProperties["round"] = round;
		if((round > 4 && round%2==((int)PhotonNetwork.LocalPlayer.CustomProperties["team"]%2))
			|| round <= 4)
		{
			PhotonNetwork.LocalPlayer.CustomProperties["roundState"] = 0;
		}
		else
		{
			PhotonNetwork.LocalPlayer.CustomProperties["roundState"] = 1;
		}
	}
	[PunRPC]
	void HideOrShowCharacter(int pid, string layerName)
	{
		GameObject playerObj = GameObject.Find("Player"+pid.ToString());
		playerObj.layer = LayerMask.NameToLayer(layerName);
        Transform transform = playerObj.transform;
        Transform[] childTrans = playerObj.GetComponentsInChildren<Transform>();
        //child is your child transform
        foreach(Transform child in childTrans)
        {
        	child.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
	}
	[PunRPC]
	void TimeUpdate( float time )
	{
		PhotonNetwork.LocalPlayer.CustomProperties["tempTime"] = time;
	}
	[PunRPC]
	void UpdatePositionToOthers(Vector3 oldPos,Vector3 newPos, int thisPlayer)
	{
		
		PhotonNetwork.LocalPlayer.CustomProperties["locPlayer"+thisPlayer.ToString()] = newPos;
		Debug.Log("player"+thisPlayer.ToString()+" loc: "+((int)newPos.x).ToString()+", "+((int)newPos.z).ToString());
		GameObject playerImg = GameObject.Find("player"+(thisPlayer).ToString()+"Img");
		//update minimap
        //x: -145~-5, y: 5~145
        if(playerImg != null)
        {
        	Vector3 thisPos = playerImg.GetComponent<RectTransform>().anchoredPosition;
	        Debug.Log("pre loc of player"+thisPlayer.ToString()+"Img: "+
	        	thisPos.x.ToString()+", "+thisPos.y.ToString()+", "+thisPos.z.ToString());
        	//int newIconX = (int)thisPos.x+(int)((int)newPos.x-(int)oldPos.x)*10;
	        //int newIconY = (int)thisPos.z+(int)((int)newPos.z-(int)oldPos.z)*10;
	        int newIconX = -145+(int)((int)newPos.x)*10;
	        int newIconY = 5+(int)((int)newPos.z)*10;
	        
	        /*if(newIconX < -145)
	        {
	            newIconX = -145;
	        }
	        else if(newIconX > -5)
	        {
	            newIconX = -5;
	        }
	        if(newIconY < 5)
	        {
	            newIconY = 5;
	        }
	        else if(newIconY > 145)
	        {
	            newIconY = 145;
	        }*/
	        //RectTransform rect = playerImg.GetComponent<RectTransform>();
	        //thisPos = new Vector3((float)newIconX, (float)newIconY, 0.0f);
	        playerImg.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)newIconX, (float)newIconY, 0.0f);
	        Debug.Log("set playerImg"+thisPlayer.ToString()+" loc: "+newIconX.ToString()+", "
	        	+newIconY.ToString());
	    }
	    else
	    {
	    	Debug.Log("playerImg"+thisPlayer.ToString()+" is null");
	    }
	}
}
