using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class CustomMatchmakingLobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject lobbyConnectButton; //button used for joining a Lobby.
    
    [SerializeField]
    private GameObject lobbyPanel; //panel for displaying lobby.
    [SerializeField]
    private GameObject mainPanel;//panel for displaying the main menu
    
    [SerializeField]
    private InputField playerNameInput; //Input field so player can change their NickName

    private string roomName; //string for saving room name
    private int roomSize = 4; //int for saving room size

    private List<RoomInfo> roomListings = new List<RoomInfo>(); //list of current rooms
    //private Dictionary<string, RoomInfo> roomListings = new Dictionary<string, RoomInfo>( );
    [SerializeField]
    private Transform roomsContainer; //container for holding all the room listings
    [SerializeField]
    private GameObject roomListingPrefab; //prefab for displayer each room in the lobby
    private string localPlayerNickName = "";
    int run = 0;
    [SerializeField]
    private Text localPlayerNickNameDisplay;
    public override void OnConnectedToMaster() //Callback function for when the first connection is established successfully.
    {
        PhotonNetwork.AutomaticallySyncScene = true; //Makes it so whatever scene the master client has loaded is the scene all other clients will load
        lobbyConnectButton.SetActive(true); //activate button for connecting to lobby
        roomListings = new List<RoomInfo>(); //initializing roomListing
        //check for player name saved to player prefs
        /*if(PlayerPrefs.HasKey("NickName"))
        {
            if (PlayerPrefs.GetString("NickName") == "")
            {
                PhotonNetwork.NickName = "Player " + UnityEngine.Random.Range(0, 1000); //random player name when not set
                PlayerPrefs.SetString("NickName", "Player " + UnityEngine.Random.Range(0, 1000));
            }
            else
            {
                //PhotonNetwork.NickName = PlayerPrefs.GetString("NickName"); //get saved player name
            }
        }
        else
        {
            PhotonNetwork.NickName = "Player " + UnityEngine.Random.Range(0, 1000); //random player name when not set
        }*/
        if(PhotonNetwork.NickName == null || PhotonNetwork.NickName == "")
        {
            PhotonNetwork.NickName = "Player " + UnityEngine.Random.Range(0, 1000); //random player name when not set
        }
        playerNameInput.text = PhotonNetwork.NickName; //update input field with player name
    }

    public void PlayerNameUpdateInputChanged(string nameInput) //input function for player name. paired to player name input field
    {
        PhotonNetwork.NickName = nameInput;
        PlayerPrefs.SetString("NickName", nameInput);
        Debug.Log("Username: "+nameInput);
    }

    public void JoinLobbyOnClick() //Paired to the Delay Start button
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        try
        {
            PhotonNetwork.JoinLobby(); //First tries to join a lobby
            Debug.Log("Join Lobby Succeeded. name: "+localPlayerNickName);
        }
        catch(Exception e)
        {
            Debug.LogException(e, this);
            Debug.Log("Failed to JoinLobby.");
        }
        run ++;
        localPlayerNickName = PhotonNetwork.NickName;
        localPlayerNickNameDisplay.text = "Hi, "+localPlayerNickName+"!";
    }
/*
    public override void OnRoomListUpdate(List<RoomInfo> roomList) //Once in lobby this function is called every time there is an update to the room list
    {
        int tempIndex;
        Debug.Log("on roomlist update");
        foreach (Transform child in roomsContainer)
        {
            //child is your child transform
            Destroy(child.gameObject);
        }
        foreach (RoomInfo room in roomList) //loop through each room in room list
        {
            *//*
            if (roomListings != null) //try to find existing room listing
            {
                tempIndex = roomListings.FindIndex(ByName(room.Name)); 
            }
            else
            {
                tempIndex = -1;
            }
            Debug.Log("tempIndex: "+tempIndex);
            if (tempIndex != -1) //remove listing because it has been closed
            {
                roomListings.RemoveAt(tempIndex);
                Destroy(roomsContainer.GetChild(tempIndex).gameObject);
            }*//*
            if (room.PlayerCount > 0 && !roomListings.Contains(room)) //add room listing because it is new
            {
                roomListings.Add(room);
            }
        }
        if(roomListings != null)
        {
           Debug.Log("current room amount: "+roomListings.Count.ToString());
        }
        foreach(RoomInfo room in roomListings)
        {
            ListRoom(room);
        }
    }


    static System.Predicate<RoomInfo> ByName(string name) //predicate function for seach through room list
    {
        return delegate (RoomInfo room)
        {
            return room.Name == name;
        };
    }
*/
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("roomListUpdated. name: "+localPlayerNickName);

        //pr用来暂时保存ui上的房间button，获取后先将原先显示的房间ui销毁掉
        foreach (Transform child in roomsContainer)
        {
            //child is your child transform
            Destroy(child.gameObject);
        }
        //遍历房间信息改变了的房间，注意这个roomList不包括所有房间，而是有属性改变了的房间，比如新增的
        foreach (var r in roomList)
        {
        //清除关闭或不可显示或已经移除了的房间                
            if (!r.IsOpen || !r.IsVisible || r.RemovedFromList)
            {
                if (roomListings.Contains(r))//如果该房间之前有，现在没有了就去掉它
                {
                    roomListings.Remove(r);                    
                }
                continue;
            }
            //更新房间信息
            if (roomListings.Contains(r))
            {
                int i = roomListings.IndexOf(r); 
                roomListings[i] = r;
            }
            //添加新房间
            else
            {
                roomListings.Add(r);//如果该房间之前没有，现在有了就加进roomListings
            }
        }

        foreach(var r in roomListings)
        {
            //这个函数用来在canvas上添加房间ui的，自己定义
            ListRoom(r);
        }

        Debug.Log("===roomList count:" + roomList.Count + "===roomListings count:" + roomListings.Count);
        //messageText.text = "===roomList count:" + roomList.Count + "===roomListings count:" + roomListings.Count;
    }    

    void ListRoom(RoomInfo room) //displays new room listing for the current room
    {
        if (room.IsVisible)
        {
            Debug.Log("list room: "+room.Name);
            GameObject tempListing = Instantiate(roomListingPrefab, roomsContainer);
            RoomButton tempButton = tempListing.GetComponent<RoomButton>();
            if(room.IsOpen)
            {
                int playerCount = room.PlayerCount;
                if(playerCount >= room.MaxPlayers)
                {
                    tempButton.SetRoom(room.Name, "Full");
                }
                else
                {
                    tempButton.SetRoom(room.Name, room.MaxPlayers, playerCount);
                }
            }
            else{
                tempButton.SetRoom(room.Name, "Playing");
            }
        }
    }
/*
    public void UpdateRoomList(RoomInfo room)
    {
        //这里我已经把房间ui做成prefab保存在Resources文件夹了
        GameObject go = Instantiate(roomListingPrefab, roomsContainer);           
        go.transform.parent = roomInLobbyPanel.transform;//roomInLobbyPanel是我用来放显示房间列表的panel
        go.transform.localScale = Vector3.one;
        go.name = name;
        go.transform.Find("RoomName").GetComponent<Text>().text = name;//设置好文字为房间名
        //绑定选择房间事件（自定义）
        go.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            this.ChooseRoom(go.name);
        });
    }
*/
    public void RoomNameInputChanged(string nameIn) //input function for changing room name. paired to room name input field
    {
        roomName = nameIn;
    }
    /*public void OnRoomSizeInputChanged(string sizeIn) //input function for changing room size. paired to room size input field
    {
        roomSize = sizeIn;
    }*/

    public void CreateRoomOnClick() //function paired to the create room button
    {
        Debug.Log("Creating room now");
        //int roomSizeInt = int.Parse(roomSize);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 4, PublishUserId = true };
        PhotonNetwork.CreateRoom(roomName, roomOps); //attempting to create a new room
        try
        {
            Debug.Log("Created rooms: "+roomListings.Count.ToString());
        }
        catch(Exception e)
        {

        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message) //create room will fail if room already exists
    {
        Debug.Log("Tried to create a new room but failed, there must already be a room with the same name");
    }

    

    public void MatchmakingCancelOnClick() //Paired to the cancel button. Used to go back to the main menu
    {
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        try
        {
            PhotonNetwork.LeaveLobby();
        }
        catch(Exception e)
        {
        }
    }
}