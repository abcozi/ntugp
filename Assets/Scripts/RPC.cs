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
		Debug.Log("setteams");
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
	[PunRPC]
	void TeamRoundUpdate(int team)
	{
		Debug.Log("teamRoundUpdate; this teamRound: "+team.ToString());
		PhotonNetwork.LocalPlayer.CustomProperties["teamRound"] = team;
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
}
