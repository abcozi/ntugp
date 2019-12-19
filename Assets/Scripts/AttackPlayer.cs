using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class AttackPlayer : MonoBehaviour
{
	[SerializeField]
	private GameObject attackPanel;
	[SerializeField]
	private GameObject Container;
	[SerializeField]
	private GameObject Listing;
	[SerializeField]
	private GameObject noTargetText;
    /*
	private Player player;
	int playerID;
	int teamMate;
	int team;
	int targetRivals = 0;
	void Awake()
	{
	}
	// Start is called before the first frame update
    void Start()
    {
    	//set player imgs
    	Container.SetActive(true);
    	playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
    	
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Finding "+"Player" + playerID.ToString());
        GameObject obj = GameObject.Find("Player" + playerID.ToString());
        if(obj == null)
        {
            //Debug.Log("obj is null");
        }
        else
        {
            //Debug.Log("obj is not null");
        }
        player = obj.GetComponent<Player>();
        teamMate = player.P_GetTeamMate();
        team = player.P_GetTeam();

        Sprite[] sprites = Resources.LoadAll<Sprite>("CharacterImgs") as Sprite[];
        //Debug.Log("count: "+sprites.Length.ToString());
        int i = 0;
        foreach(Sprite sp in sprites)
        {
            //skip myself and those not in my area
            if(i+1 == playerID)
            {
                continue;
            }
            else
            {
                GameObject thisPlayer = GameObject.Find("Player" + (i+1).ToString());

                if(thisPlayer != null && player.playerInMyCurrentArea(thisPlayer.transform.position))
                {
                    GameObject tempListing = Instantiate(Listing, Container.transform);
                    tempListing.name = "CharacterListing"+(i+1).ToString();
                    GameObject nameObj =  tempListing.transform.GetChild(1).gameObject;
                    nameObj.GetComponent<Text>().text = thisPlayer.GetComponent<PhotonView>().Owner.NickName;
                    GameObject imgObj = tempListing.transform.GetChild(0).gameObject;
                    Image img = imgObj.GetComponent<Image>();
                    img.sprite = sp;
                    targetRivals += 1;
                }
                i += 1;
             }
         }
        if(targetRivals == 0)
        {
            Container.SetActive(false);
            noTargetText.SetActive(true);
        }
    }*/
}
