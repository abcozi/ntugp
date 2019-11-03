using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSetupController : MonoBehaviour
{
    private List<string> characters = new List<string>(new string[] { "alice", "brandon", "charlotte", "dean" });
    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject gameManager;
    [SerializeField]
    private GameObject cameraController;
    [SerializeField]
    private GameObject lightController;
    int mapSize = Global.mapSize;
    // This script will be added to any multiplayer scene
    void Start()
    {
        Debug.Log("Game Start!");
        CreatePlayer(); //Create a networked player object for each player that loads into the multiplayer scenes.
    }

    private void CreatePlayer()
    {
        //Debug.Log("Creating Player: "+Photon.Realtime.Player.UserID);
        //int playerID = PhotonNetwork.LocalPlayer.GetHashCode();
        int playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
        
        Debug.Log("Creating Player: "+playerID.ToString());
        System.Random rnd = new System.Random();
        int x = rnd.Next(0, mapSize); // creates a number between 1 and 15
        int z = rnd.Next(0, mapSize); // creates a number between 1 and 15
        Debug.Log("x: "+x.ToString()+", z: "+z.ToString());
        
        GameObject obj = PhotonNetwork.Instantiate("VRM/"+characters[playerID-1], new Vector3(x, 0, z), Quaternion.identity, 0);
        obj.name = "Player";
        Player player = obj.GetComponent<Player>();
        player.P_SetId(playerID);
        player.P_SetNickName(PhotonNetwork.NickName);
        Debug.Log("player id: "+player.P_GetId().ToString()+", name: "+player.P_GetNickName());
        //player.name = "Player";
        gameManager.SetActive(true);
        canvas.SetActive(true);
        cameraController.SetActive(true);
        lightController.SetActive(true);
        //player.SetActive(true);
        //player.P_SetLocation(new Vector3(0, 0, 0));
    }
}