/*
	Script: SliderChangeDiceAmountInput
	Purpose: 在玩家骰骰子時，透過slider滑動選擇要使用多少個骰子，浮動顯示目前選擇的骰子數量
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SliderChangeDiceAmountInput : MonoBehaviour
{
    [SerializeField]
    private Text diceAmountInputValue;//玩家選擇要用幾顆骰子
    private Player player;
    int diceMax = 3;
    void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
      //找到玩家輸入骰子數量值
      int playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
      player = GameObject.Find("Player"+playerID.ToString()).GetComponent<Player>();
      
      if(player.P_GetDiceAmount() < 3)
      {
        diceMax = player.P_GetDiceAmount();
      }
      else
      {
        diceMax = 3;
      }
      //diceAmountInputValue = GameObject.Find("diceAmountInput").GetComponent<Text>();
      diceAmountInputValue.text = diceMax.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
      method: textUpdate
      purpose: 動態更新ui上的目前選擇骰子量
    */
   	public void textUpdate(float value)
   	{
      //將從slider input的數字round後轉為text印在ui上
   		diceAmountInputValue.text = Mathf.RoundToInt(value).ToString();
   	}
}
