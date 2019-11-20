using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CharacterButton : MonoBehaviour
{
    [SerializeField]
    private Image img; 
    [SerializeField]
    private Text selectedPlayerName;
    public bool IsChosen = false;
    public int selectedPlayerId = -1;
    public int imgID;
    private PhotonView photonView;

    public void SelectCharacterOnClick()
    {	
    	photonView = this.photonView = GetComponent<PhotonView>();

    	string name = "";
		if(!IsChosen&& (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"] == -1)
    	{
	   		//if this character is not chosen yet and the player does not 
	   		//choose any character now, legal
	   		Debug.Log("first click");
	   		selectedPlayerName.text = PhotonNetwork.NickName;
    		name = PhotonNetwork.NickName;	    		
    		IsChosen = true;
	    	selectedPlayerId = PhotonNetwork.LocalPlayer.GetHashCode();
	   		
	   		//update player's selected character code
			Hashtable hash = new Hashtable();				
			hash.Add("selectedCharacter", imgID);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

			//send updates to network
			photonView.RPC("CharacterListingUpdate", RpcTarget.All, true, selectedPlayerId, imgID, name);
		
	    }
	    else if(PhotonNetwork.LocalPlayer.GetHashCode() == selectedPlayerId)
	    {
	    	//if same player click on the character again, cancel select
	   		Debug.Log("click again");
	   		selectedPlayerName.text = "";
	    	name = "";
	   		IsChosen = false;
	   		selectedPlayerId = -1;
	    		
	    	//update player's selected character code
			Hashtable hash = new Hashtable();
			hash.Add("selectedCharacter", -1);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hash);				
		
			//send updates to network
			photonView.RPC("CharacterListingUpdate", RpcTarget.All, false, selectedPlayerId, imgID, name);
			
	    }
	    else
	    {
	    	Debug.Log("else");
	   	}
    }
    public void SetDisplay(Sprite characterImg, int id)
    {
    	img.sprite = characterImg;
    	selectedPlayerName.text = "";
    	imgID = id;
    }
}
