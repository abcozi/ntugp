/*
	Script: LightController
	Purpose: 綁定角色頭上的光源
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LightController : MonoBehaviour
{
	private Player player;//欲綁定角色
	private Vector3 offset;//紀錄角色位置和光源位置差距
    // Start is called before the first frame update
    void Start()
    {
    	//設定角色位置與光源位置落差
        player = player = GameObject.Find("Player"+PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"].ToString()).GetComponent<Player>();
        transform.position = new Vector3(player.transform.position.x+(float)0.00039, player.transform.position.y+(float)2.41671, 
            player.transform.position.z-(float)0.0221);
        transform.rotation = Quaternion.Euler((float)89.9720, 0, 0);

        offset = transform.position - player.transform.position;        
    }

    // 光源位置變化會比角色位置變化慢，因為要先知道角色新位置才能夠算出光源新位置
    void LateUpdate()
    {

    	//改變光源位置
        transform.position = player.transform.position + offset;
    }
}
