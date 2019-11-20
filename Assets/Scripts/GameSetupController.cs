using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
    private PhotonView photonView;
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
        obj.name = "Player"+playerID.ToString();
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
        /* distribute teams for players*/
        if(PhotonNetwork.IsMasterClient)
        {
            /*
            int getRanNum1 = rnd.Next(1, 5);
            int getRanNum2 = rnd.Next(1, 5);
            while(getRanNum2 == getRanNum1){
                getRanNum2 = rnd.Next(1, 5);
            }
            Debug.Log("num1: "+getRanNum1.ToString()+", num2: "+getRanNum2.ToString());
            photonView = GetComponent<PhotonView>();
            */
            photonView = GetComponent<PhotonView>();
            List<int> list = new List<int>(){1, 2, 3, 4};
            List<int> TempList = new List<int>();
            int length = list.Count;
            int TempIndex = 0;

            while (length > 0) {
                TempIndex = rnd.Next(0, length);  // get random value between 0 and original length
                TempList.Add(list[TempIndex]); // add to temp list
                list.RemoveAt(TempIndex); // remove from original list
                length = list.Count;  // get new list <T> length.
            }

            list = new List<int>();
            list = TempList; // copy all items from temp list to original list.
            for(int i = 0 ; i < list.Count ; i ++)
            {
                Debug.Log("list["+i.ToString()+"]: "+list[i].ToString());
            }
            photonView.RPC("SetTeams", RpcTarget.All, list[0], list[1], list[2], list[3]);
        }
    }
}