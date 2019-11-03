using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class RPC : MonoBehaviour
{
	/*
	[PunRPC]
	void SetCharacterChoices(int imgID, Sprite sp)
	{
		GameObject tempListing = GameObject.Find("CharacterListing(clone)").GetComponent<GameObject>();
		tempListing.name = "CharacterListing"+imgID.ToString();
	    GameObject childObj =  tempListing.transform.GetChild(1).gameObject;
	    childObj.name = "SelectedPlayerName"+imgID.ToString();
	    //GameObject tempListing = Instantiate(Listing, Container);
	    CharacterButton characterButton = tempListing.GetComponent<CharacterButton>();
	    characterButton.SetDisplay(sp, imgID); 		
	}
    [PunRPC]
    void GitParent(CharacterButton button)
    {
    	//this.gameObject = button;
		this.gameObject.transform.parent = GameObject.Find ("CharacterDisplay").transform;
		//this.gameObject.GetComponent<PhotonTransformView>()
		this.gameObject.name = button.name;
		//this.gameObject.transform.GetChild(1).gameObject.name = "SelectedPlayerName"+imgID.ToString();
    	
    }
    */
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
    /*
    [PunRPC]
	void CharacterListingUpdate(bool IsChosen, int selectedPlayerId, int imgID)
	{
		Text txt = GameObject.Find("SelectedPlayerName"+imgID.ToString()).GetComponent<Text>();
    		
	    if(!IsChosen)
    	{
    		Debug.Log("first click");
    		txt.text = PhotonNetwork.NickName;
    		IsChosen = true;
    		selectedPlayerId = PhotonNetwork.LocalPlayer.GetHashCode();
    	}
    	else if(PhotonNetwork.LocalPlayer.GetHashCode() == selectedPlayerId)
    	{
    		//if same player click on the character again, cancel select
    		Debug.Log("click again");
    		txt.text = "";
    		IsChosen = false;
    		selectedPlayerId = -1;
    	}
    	else
    	{
    		Debug.Log("else");
    	}
	}
	*/
}
