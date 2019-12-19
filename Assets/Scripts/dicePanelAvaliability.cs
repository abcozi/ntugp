using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class dicePanelAvaliability : MonoBehaviour
{
	[SerializeField]
	private GameObject panel;
    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            player = GameObject.Find("Player"+PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"].ToString()).GetComponent<Player>();
        }
        catch(System.Exception ex)
        {

        }
    }
    public void diceBtnClicked()
    {
        if(player == null)
        {
            player = GameObject.Find("Player"+PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"].ToString()).GetComponent<Player>();
        }
    	Debug.Log("diceBtnClicked called");
        int roundState = (int)PhotonNetwork.LocalPlayer.CustomProperties["roundState"];
    	if(roundState == 0 && !panel.active)
    	{
    		//dice panel availible
    		panel.SetActive(true);
    		Debug.Log("state:0, set true");
    	}
        else if(roundState == 0 && panel.active)
        {
            //dice panel availible
            panel.SetActive(false);
            Debug.Log("state:0, clicked on so set false");
        }
    	else
    	{
    		//dice panel not availible
    		panel.SetActive(false);
    		Debug.Log("state:1, set false");
    	}
        if((int)PhotonNetwork.LocalPlayer.CustomProperties["round"] <= 4 && player.GetUsedDiceAmount() >= 12)
        {
            panel.SetActive(false);
            Debug.Log("used 12 dices already, set false");
        }
    }
}
