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

	private List<string> characters = new List<string>(new string[] { "alice", "brandon", "charlotte", "dean" });
    private int playerID;
	//Awake
	void Awake()
	{
		AttributePanel.SetActive(true);
		playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
		Sprite sprite = Resources.Load<Sprite>("CharacterImgs/"+characters[playerID-1]) as Sprite;
    	playerImg.sprite = sprite;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
