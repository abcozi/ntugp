using Photon.Pun;
using Photon.Realtime;
//using Photon.Realtime.LoadBalancingClient;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class NetworkController : MonoBehaviourPunCallbacks
{
    /******************************************************
    * Refer to the Photon documentation and scripting API for official definitions and descriptions
    * 
    * Documentation: https://doc.photonengine.com/en-us/pun/current/getting-started/pun-intro
    * Scripting API: https://doc-api.photonengine.com/en/pun/v2/index.html
    * 
    * If your Unity editor and standalone builds do not connect with each other but the multiple standalones
    * do then try manually setting the FixedRegion in the PhotonServerSettings during the development of your project.
    * https://doc.photonengine.com/en-us/realtime/current/connection-and-authentication/regions
    *
    * ******************************************************/

    [SerializeField]
    private int gameVersion;
    [SerializeField]
    private GameObject canvasMenu;
    [SerializeField]
    private GameObject canvasLoading;

    // Start is called before the first frame update
    void Start()
    {
        string regionString = "rue";
        PhotonNetwork.GameVersion = gameVersion.ToString();
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = regionString;
        PhotonNetwork.ConnectUsingSettings();
        //Connects to Photon master servers
        //Other ways to make a connection can be found here: https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_pun_1_1_photon_network.html
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true; //Makes it so whatever scene the master client has loaded is the scene all other clients will load
        try
        {
            canvasMenu.SetActive(true);
            canvasLoading.SetActive(false);
        }
        catch
        {
        }
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
    }
}
