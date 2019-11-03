/*
    Script: SetSliderMaxValue
    Purpose: 在每一次玩家可以骰骰子時，設定slider max value
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetSliderMaxValue : MonoBehaviour
{
	private Slider slider;//slider
    private Player player;//player
    // Start is called before the first frame update
    void Awake()
    {
        
    }
    void Start()
    {
        //取得slider跟player component
        slider = GameObject.Find("diceAmountSlider").GetComponent<Slider>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        //將slider max value 更新為player 目前擁有的骰子量
        slider.maxValue = player.P_GetDiceAmount();
    }
}
