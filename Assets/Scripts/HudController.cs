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

	private List<string> characters = new List<string>(new string[] { "alice", "brandon", "charlotte", "dean" });
    private int playerID;
    private Player player;
	//Awake
	void Awake()
	{
		AttributePanel.SetActive(true);
		playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
		Debug.Log("hudController: id: "+playerID.ToString());
		
		Sprite sprite = Resources.Load<Sprite>("CharacterImgs/"+characters[playerID-1]) as Sprite;
    	playerImg.sprite = sprite;
	}
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player"+playerID.ToString()).GetComponent<Player>();
		nickNameTxt.text = (string)PhotonNetwork.LocalPlayer.NickName;
		
    	attackValueTxt.text = player.P_GetAttack().ToString();
    	defenseValueTxt.text = player.P_GetDefense().ToString();
    	skillValueText.text = player.P_GetSkill().ToString();
    	diceAmountTxt.text = player.P_GetDiceAmount().ToString();
    	actionPointTxt.text = player.P_GetActionPoint().ToString();
    	itemAmountTxt.text = player.P_GetItemListSize().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        attackValueTxt.text = player.P_GetAttack().ToString();
    	defenseValueTxt.text = player.P_GetDefense().ToString();
    	skillValueText.text = player.P_GetSkill().ToString();
    	diceAmountTxt.text = player.P_GetDiceAmount().ToString();
    	actionPointTxt.text = player.P_GetActionPoint().ToString();
    	itemAmountTxt.text = player.P_GetItemListSize().ToString();
    }
    
}
