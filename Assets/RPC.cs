using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class RPCScript : MonoBehaviour
{
    [PunRPC]
    void GitParent(){
		this.gameObject.transform.parent = GameObject.Find ("CharacterDisplay").transform;
		//this.gameObject.GetComponent<PhotonTransformView>()
    }
}
