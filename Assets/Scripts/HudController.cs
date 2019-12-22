using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class HudController : MonoBehaviour
{
	[SerializeField]
	private GameObject Hud;
	[SerializeField]
	private GameObject DiceSilderPanel;
	[SerializeField]
	private GameObject ActionPointPanel;
	[SerializeField]
	private GameObject AttributePanel;
	[SerializeField]
	private Image playerImg;
	[SerializeField]
	private Text nickNameTxt;
	[SerializeField]
	private Text attackValueTxt;
	[SerializeField]
	private Text defenseValueTxt;
	[SerializeField]
	private Text skillValueText;
	[SerializeField]
	private Text diceAmountTxt;
	[SerializeField]
	private Text actionPointTxt;
	[SerializeField]
	private Text itemAmountTxt;
	[SerializeField]
	private GameObject attackPanel;
	[SerializeField]
	private GameObject Container;
	[SerializeField]
	private GameObject Listing;
	[SerializeField]
	private GameObject noTargetText;
    [SerializeField]
    private Text teamMateText;
    [SerializeField]
    private Image teamMateImg;

	private List<string> characters = new List<string>(new string[] { "alice", "brandon", "charlotte", "dean" });
    private int playerID;
    private Player player;

    int targetRivals = 0;
    //int iter = 0;
	//Awake
	void Awake()
	{
		AttributePanel.SetActive(true);
		playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
		Debug.Log("hudController: id: "+playerID.ToString());
		
		Sprite sprite = Resources.Load<Sprite>("CharacterImgs/"+characters[playerID-1]) as Sprite;
    	playerImg.sprite = sprite;
    	//set player imgs
    	Container.SetActive(true);
	}
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player"+playerID.ToString()).GetComponent<Player>();
		try
        {
            nickNameTxt.text = (string)PhotonNetwork.LocalPlayer.NickName;
            
            attackValueTxt.text = player.P_GetAttack().ToString();
            defenseValueTxt.text = player.P_GetDefense().ToString();
            skillValueText.text = player.P_GetSkill().ToString();
            diceAmountTxt.text = player.P_GetDiceAmount().ToString();
            actionPointTxt.text = player.P_GetActionPoint().ToString();
            itemAmountTxt.text = player.P_GetItemListSize().ToString();
            Debug.Log("set attack: "+player.P_GetAttack().ToString());
            Debug.Log("set def: "+player.P_GetDefense().ToString());
        
        }
        catch(System.Exception ex)
        {

        }
		
    	Sprite[] sprites = Resources.LoadAll<Sprite>("CharacterImgs") as Sprite[];
    	Debug.Log("count: "+sprites.Length.ToString());
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

    			if(thisPlayer != null)
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
    }

    // Update is called once per frame
    void Update()
    {
    	/*
    	if(iter == 0)
    	{
    		nickNameTxt.text = (string)PhotonNetwork.LocalPlayer.NickName+"_"+
		PhotonNetwork.LocalPlayer.CustomProperties["order"].ToString();
    	}
    	*/
        if(player != null && player.P_GetSkill() > 0)
        {
            setAttrs();
        }
        
    	for(int i = 0 ; i < 4 ; i ++)
    	{
    		GameObject thisPlayer = GameObject.Find("Player" + (i+1).ToString());
    		//if(i+1 != playerID && player.playerInMyCurrentArea(thisPlayer.transform.position))
    		//{
    			//if()
    			//GameObject thisPlayer = GameObject.Find("Player" + (i+1).ToString());
    			
    		//}
    	}
    	//iter += 1;
    	
    }
    public void setAttrs()
    {	
    	nickNameTxt.text = player.P_GetNickName();
    	attackValueTxt.text = player.P_GetAttack().ToString();
    	defenseValueTxt.text = player.P_GetDefense().ToString();
    	skillValueText.text = player.P_GetSkill().ToString();
    	diceAmountTxt.text = player.P_GetDiceAmount().ToString();
    	actionPointTxt.text = player.P_GetActionPoint().ToString();
    	itemAmountTxt.text = player.P_GetItemListSize().ToString();
        if((int)PhotonNetwork.LocalPlayer.CustomProperties["round"] > 4)
        {
            //公布隊友
            teamMateText.text = "隊友:";
            teamMateImg.sprite = Resources.Load<Sprite>("CharacterImgs/"+
                characters[((int)PhotonNetwork.LocalPlayer.CustomProperties["teamMate"])-1]) as Sprite;
        }
    }
}
