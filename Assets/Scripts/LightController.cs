/*
	Script: LightController
	Purpose: 綁定角色頭上的光源
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
	public GameObject player;//欲綁定角色
	private Vector3 offset;//紀錄角色位置和光源位置差距
    // Start is called before the first frame update
    void Start()
    {
    	//設定角色位置與光源位置落差
        offset = transform.position - player.transform.position;        
    }

    // 光源位置變化會比角色位置變化慢，因為要先知道角色新位置才能夠算出光源新位置
    void LateUpdate()
    {

    	//改變光源位置
        transform.position = player.transform.position + offset;
    }
}
