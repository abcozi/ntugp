/*
	Global Variables
*/
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    public static int mapSize = 15;
    public static int seed = 0;

    void Awake()
    {
        string timeStamp = PhotonNetwork.ServerTimestamp.ToString();
        timeStamp = timeStamp.Substring(1, 4);
        seed = int.Parse(timeStamp);
    }

}
