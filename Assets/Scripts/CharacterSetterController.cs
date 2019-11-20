using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CharacterSetterController : MonoBehaviour
{
	[SerializeField]
	private Transform Container;
	[SerializeField]
	private GameObject Listing;
	[SerializeField]
	private GameObject playGameButton;
	public int gameSceneIndex = 5;
	
	PhotonView MyPhotonView;
    // Start is called before the first frame update
    void Start () {
    	Debug.Log("Start CharacterSetter");
        Sprite[] sprites = Resources.LoadAll<Sprite>("CharacterImgs") as Sprite[];
    	Debug.Log("count: "+sprites.Length.ToString());
    	int i = 0;
    	foreach(Sprite sp in sprites)
    	{
    		//if(PhotonNetwork.IsMasterClient)
    		//{
    			//GameObject tempListing = PhotonNetwork.Instantiate("CharacterListing", new Vector3(0,0,0) ,Quaternion.identity, 0);
	     		GameObject tempListing = Instantiate(Listing, Container);
	     		
	     		tempListing.name = "CharacterListing"+(i+1).ToString();
	     		GameObject childObj =  tempListing.transform.GetChild(1).gameObject;
	     		childObj.name = "SelectedPlayerName"+(i+1).ToString();
	     		//GameObject tempListing = Instantiate(Listing, Container);
	     		CharacterButton characterButton = tempListing.GetComponent<CharacterButton>();
	     		characterButton.SetDisplay(sp, i+1);

	     		MyPhotonView = characterButton.GetComponent<PhotonView>();
	     		//MyPhotonView.RPC("SetCharacterChoices", RpcTarget.All, i+1, sp);
	            //MyPhotonView.RPC("GitParent", RpcTarget.All);
	     		i ++;
    		//}
    	}
    	//initialize player's selected character code
		Hashtable hash = new Hashtable();
		hash.Add("selectedCharacter", -1);
		PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    void Update()
    {
    	//check if every players have selected their characters, if yes,
    	//set start button to true.(in master client)
    	if(PhotonNetwork.IsMasterClient)
    	{
    		bool readyToPlay = true;
	    	foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList)
	    	{
	    		if((int)player.CustomProperties["selectedCharacter"] == -1)
	    		{
	    			readyToPlay = false;
	    			break;
	    		}
	    	}
	    	if(readyToPlay)
	    	{
	    		//set button to active
	    		playGameButton.SetActive(true);
	    	}
	    	else{
	    		//set button to inactive
	    		playGameButton.SetActive(false);
	    	}
    	}
    }
    public void PlayGameOnClick() //paired to the start button. will load all players into the multiplayer scene through the master client and AutomaticallySyncScene
    {
        Debug.Log("PlayGame clicked");
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameSceneIndex);
        }
    }
}
