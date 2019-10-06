/*
    Script: ClickRowDiceBtn
    Purpose: 玩家按下擲骰子的按鈕之後，將選擇的骰子量傳給P_RowDice，更新Action point
    跟骰子量之後，顯示於UI，進入移動模式。
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickRowDiceBtn : MonoBehaviour
{
	Text diceAmountText;//Slider 上選擇的骰子數量(text display)
    int diceAmountValue;//diceAmountText的純數字值
	Player player;//player
	void Awake()
	{
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
        method: clickRowDiceBtn
        purpose: 將request dice amount傳給P_RowDice，並且disable slider panel
        (註: enable action point panel在inspector區塊中的onclick()中完成)
    */
    public void clickRowDiceBtn()
    {
        //get component and parse to int
        diceAmountText = GameObject.Find("diceAmountInput").GetComponent<Text>();
        diceAmountValue = int.Parse(diceAmountText.text);
        Debug.Log("value: "+diceAmountValue);//print value in Log

        //disable dice slider panel
        GameObject diceSliderPanel = GameObject.Find("DiceSliderPanel");
        diceSliderPanel.SetActive(false);
        //send request amount to P_RowDice
        player = GameObject.Find("Player").GetComponent<Player>();
        player.P_RowDice(diceAmountValue);
    }
}
