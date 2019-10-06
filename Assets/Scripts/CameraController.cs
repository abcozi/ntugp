/*
    Script: CameraController
    Purpose: 綁定角色頭上的相機
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public GameObject player;//player
	private Vector3 offset;//角色與相機位置落差
    // Start is called before the first frame update
    void Start()
    {
        //紀錄剛開始角色與相機位置落差
        offset = transform.position - player.transform.position;
    }

    // The Camera Update should happen after normal Update methods.
    void LateUpdate()
    {
        //更新相機位置
        transform.position = player.transform.position + offset;
    }
}
