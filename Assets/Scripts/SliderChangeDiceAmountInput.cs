/*
	Script: SliderChangeDiceAmountInput
	Purpose: 在玩家骰骰子時，透過slider滑動選擇要使用多少個骰子，浮動顯示目前選擇的骰子數量
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderChangeDiceAmountInput : MonoBehaviour
{
    Text diceAmountInputValue;//玩家選擇要用幾顆骰子
    void Awake()
    {
      //找到玩家輸入骰子數量值
      diceAmountInputValue = GameObject.Find("diceAmountInput").GetComponent<Text>();
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
      method: textUpdate
      purpose: 動態更新ui上的目前選擇骰子量
    */
   	public void textUpdate(float value)
   	{
      //將從slider input的數字round後轉為text印在ui上
   		diceAmountInputValue.text = Mathf.RoundToInt(value).ToString();
   	}
}
