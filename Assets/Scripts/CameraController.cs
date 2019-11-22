/*
    Script: CameraController
    Purpose: 綁定角色頭上的相機
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraController : MonoBehaviour
{
	private Player player;//player
	private Vector3 offset;//角色與相機位置落差
    // Start is called before the first frame update
    void Start()
    {
        //紀錄剛開始角色與相機位置落差
        //offset = transform.position - player.transform.position;
        player = player = GameObject.Find("Player"+PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"].ToString()).GetComponent<Player>();
        transform.position = new Vector3(player.transform.position.x-(float)0.03, player.transform.position.y+(float)4.5, 
            player.transform.position.z-(float)1.5);
        transform.rotation = Quaternion.Euler((float)67.655, (float)-0.859, 0);
        offset = transform.position - player.transform.position;
        Debug.Log("offset: "+offset);
    }

    // The Camera Update should happen after normal Update methods.
    void LateUpdate()
    {
        //更新相機位置
        transform.position = player.transform.position + offset;
    }
}
