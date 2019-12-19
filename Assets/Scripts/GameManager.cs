using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviour
{
    private int M_roundstate;
    private int M_roundtime;
    public static GameManager gameManager;
    public GameObject diceSliderPanel;
    //public GameObject actionPointPanel;
    private Player player;
    private int roundstate = 0;
    private int itemfetching = 0;
    private int round = 0;
    private int playerID = 0;
    private int playerTeam = 0;
    private int playerTeamMate = 0;
    private int teamRound = 1;
    private int infoNum = 0, infoCloseNum = 0;
    private PhotonView photonView;
    private Map map;
    private GameObject infoText, infoImage, infoBackGroundPanel, infoCanvas;
    private List<Tuple<string, int>> itemEffectRound;
    private int roundPre;
    [SerializeField]
    private Text timerText;
    [SerializeField]
    private Text roundText;
    private List<GameObject> playerImgs = new List<GameObject>();

    float tempTime = 0;

    void Awake()
    {
        if (gameManager == null)
        {
            gameManager = this;
        }
        if (infoCanvas == null)
        {
            infoCanvas = GameObject.Find("InfoCanvas").gameObject;
            infoText = infoCanvas.transform.GetChild(0).GetChild(0).Find("InfoText").gameObject;
            infoImage = infoCanvas.transform.GetChild(0).GetChild(0).Find("Image").gameObject;
            infoBackGroundPanel = infoCanvas.transform.Find("InfoBackGroundPanel").gameObject;
        }
        playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
        player = GameObject.Find("Player"+playerID.ToString()).GetComponent<Player>();
        playerTeam = player.P_GetTeam();
        playerTeamMate = player.P_GetTeamMate();
        photonView = GetComponent<PhotonView>();

        //photonView = GetComponent<PhotonView>();
        map = GameObject.Find("Map").GetComponent<Map>();

        itemEffectRound = new List<Tuple<string, int>>();

        roundPre = round;
        //Resources.Load<Sprite>("CharacterImgs/dean");

        for(int i = 1 ; i <= 4 ; i ++)
        {
            playerImgs.Add(GameObject.Find("player"+i.ToString()+"Img"));
        }
        string[] characterNames = new string[]{"alice", "brandon", "charlotte", "dean"};
        for(int i = 0 ; i < 4 ; i ++)
        {
            playerImgs[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("CharacterImgs/"+characterNames[i]);
        }

    }
    void Start()
    {
        
            playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
            player = GameObject.Find("Player" + playerID.ToString()).GetComponent<Player>();
            playerTeam = player.P_GetTeam();
            playerTeamMate = player.P_GetTeamMate();
            if(PhotonNetwork.IsMasterClient)
            {
                //sync "tempTime", "teamRound", "round"
                photonView.RPC("TimeUpdate", RpcTarget.All, 0.0f);
                photonView.RPC("TeamRoundUpdate", RpcTarget.All, 1, 1);
                InvokeRepeating("UpdateTimer", 0.0f, 1.0f);
                //UpdateTimer();
            }
            try
            {
                PhotonNetwork.LocalPlayer.CustomProperties["lastLoc"] = player.transform.position;
                UpdateMinimap();
            }
            catch(System.Exception ex)
            {

            }
    }
    void Update()
    {
        

            if(player == null)
            {
                player = GameObject.Find("Player" + playerID.ToString()).GetComponent<Player>();
                playerTeam = player.P_GetTeam();
                playerTeamMate = player.P_GetTeamMate();
            }
            else
            {
            }
            if(playerTeam <= 0)
            {
                playerTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"];
                playerTeamMate = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamMate"];
            }
            else
            {
                
            }
            if((float)PhotonNetwork.LocalPlayer.CustomProperties["tempTime"] != tempTime)
            {
                tempTime = (float)PhotonNetwork.LocalPlayer.CustomProperties["tempTime"];
                round = (int)PhotonNetwork.LocalPlayer.CustomProperties["round"];
                teamRound = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamRound"];
                roundstate = (int)PhotonNetwork.LocalPlayer.CustomProperties["roundState"];
                
                //update round every 20 secs
                UpdateRoundState();

                //update item effect state
                if (roundPre != round)
                {
                    UpdateItemEffectState();
                    roundPre = round;
                }

                //update minimap
                UpdateMinimap();

                //master client updates timer
                //all clients update timer value got from master client
                //Debug.Log("timer: "+((int)tempTime).ToString());
                UpdateTimerRoundText();
            }

        
    }
    /*----------------------------  Custom Methods  ---------------------------*/
    public void UpdateTimerRoundText()
    {
        if(round <= 4)
        {
            timerText.text = ((int)(80.0f - tempTime)).ToString();
        }
        else
        {
            if(roundstate == 0)
            {
                //還沒骰骰子
                timerText.text = ((int)(10.0f - tempTime)).ToString();
            }
            else
            {
                timerText.text = ((int)(20.0f - tempTime)).ToString();
            }
        }
        
        string newRoundText = "回合:"+((int)round).ToString();
        teamRound = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamRound"];
        round = (int)PhotonNetwork.LocalPlayer.CustomProperties["round"];
        if(round <= 4)
        {
            //Debug.Log("my team's round");
            newRoundText += "\n(進攻)"+teamRound.ToString()+", "+roundstate.ToString();
        }
        else if(teamRound == playerTeam)
        {
            //my team's round
            //diceSliderPanel.SetActive(true);
            //Debug.Log("my team's round");
            player.P_SetActionPoint(0);
            newRoundText += "\n(進攻)"+teamRound.ToString()+", "+roundstate.ToString();
        }
        else
        {
            //not my team's round
            player.P_SetActionPoint(0);
            //actionPointPanel.SetActive(false);
            //Debug.Log("not my team's round");
            newRoundText += "\n(防守)"+teamRound.ToString()+", "+roundstate.ToString();
        }
        roundText.text = newRoundText;
    }
    
    public void UpdateRoundState()
    {
        //Debug.Log("UpdateRoundState called");
        if(round <= 4)
        {
            if(player.GetUsedDiceAmount() < 12)
            {
                //全部都進攻
                roundstate = 0;//roundstate = 0, 代表可以骰骰子的狀態
            }
            else
            {
                roundstate = 1;//roundstate = 1, 代表不可以骰骰子的狀態
            }
        }
        else//round > 4
        {
            /*roundstate要為0的滿足條件(要同時滿足)：
                1. tempTime < 10.0f
                2. 我的進攻回合
                3. roundstate = 0
            */
            if(teamRound == playerTeam)
            {
                if(roundstate == 0 && tempTime < 10.0f)
                {
                    roundstate = 0;
                }
                else if(roundstate == 0 && tempTime >= 10.0f)
                {
                    player.P_RowDice(1);
                    roundstate = 1;
                }
                else
                {
                    //roundstate == 1
                    roundstate = 1;
                }
            }
            else
            {
                roundstate = 1;
            }
        }
        PhotonNetwork.LocalPlayer.CustomProperties["roundState"] = roundstate;
        //Debug.Log("roundstate: "+roundstate.ToString());
    }
    public void UpdateTimer()
    {
        //float newTimerVal = (float)((int)(tempTime + Time.deltaTime*2.0f));
        float newTimerVal = tempTime + 1.0f;
        //if((round > 4 && newTimerVal >= 20.0f)||(round <= 4 && newTimerVal >= 80.0f))
        if((round > 4 && newTimerVal >= 20.0f)||(round <= 4 && ((int)newTimerVal % 20 == 0)))
        {
            //Debug.Log("if (int)newTimerVal % 20 : "+((int)newTimerVal % 20).ToString());
            if((round <= 4 && newTimerVal >= 80.0f)||(round > 4 && newTimerVal >= 20.0f))
            {
                newTimerVal = 0.0f;
            }
            UpdateRound();
        }
        else
        {
            //Debug.Log("else (int)newTimerVal % 20 : "+((int)newTimerVal % 20).ToString());
        }
        //Debug.Log("new timer: "+newTimerVal.ToString());
        photonView.RPC("TimeUpdate", RpcTarget.All, newTimerVal);
    }
    public void UpdateRound()
    {
        if(round < 4)
        {
            //Debug.Log("(int)tempTime % 20: "+((int)tempTime % 20).ToString());
            //Debug.Log("update round and round < 4");
            round += 1;
            //Debug.Log("teamroundUpdate to "+round.ToString());
            photonView.RPC("TeamRoundUpdate", RpcTarget.All, playerTeam, round);
        }
        else
        {
            round += 1;
            if(round % 2 == 1)
            {
                teamRound = 1;
            }
            else
            {
                teamRound = 2;
            }
            photonView.RPC("TeamRoundUpdate", RpcTarget.All, teamRound, round);
        }
    }
    public void UpdateRoundState_()
    {
        tempTime = (float)PhotonNetwork.LocalPlayer.CustomProperties["tempTime"];
        round = (int)PhotonNetwork.LocalPlayer.CustomProperties["round"];
        teamRound = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamRound"];
        roundstate = (int)PhotonNetwork.LocalPlayer.CustomProperties["roundState"];
        if(tempTime == 0)
        {
            if (round > 5 && teamRound != playerTeam)
            {
                //diceSliderPanel.SetActive(false);
                roundstate = 1;
                PhotonNetwork.LocalPlayer.CustomProperties["roundState"] = roundstate;
            }
            else{
                //diceSliderPanel.SetActive(true);
                roundstate = 0;
                PhotonNetwork.LocalPlayer.CustomProperties["roundState"] = roundstate;
            }
        }
        else
        {
            //0 < tempTime < 20
            if (roundstate == 0 && tempTime >= 10)
            {
                //tempTime = 0;
                roundstate = 1;
                PhotonNetwork.LocalPlayer.CustomProperties["roundState"] = roundstate;
                //if (teamRound == player.P_GetTeam())
                //{
                    //diceSliderPanel.SetActive(false);
                    //actionPointPanel.SetActive( true );
                    player.P_RowDice(1);
                //}
            }
            else if(roundstate == 0 && tempTime < 10)
            {
                //diceSliderPanel.SetActive(true);
            }
        }
        
        /*
        if (roundstate == 1 && tempTime > 20)
        {
            //tempTime = 0;
            roundstate = 0;
            M_RoundUpdate();
        }*/
    }
    public void UpdateTimer_()
    {
        tempTime += Time.deltaTime*2.0f;
        if(tempTime >= 20.0f)
        {
            tempTime = 0.0f;
            //M_RoundUpdate();
        }
        photonView.RPC("TimeUpdate", RpcTarget.All, tempTime);
    }
    public void UpdateMinimap()
    {
        //update minimap
        //x: -145~-5, y: 5~145
        //my icon
        /*
        int myIconX = -5+((int)player.transform.position.x)*10;
        int myIconY = 5+((int)player.transform.position.z)*10;
        if(myIconX < -145)
        {
            myIconX = -145;
        }
        else if(myIconX > -5)
        {
            myIconX = -5;
        }
        if(myIconY < 5)
        {
            myIconY = 5;
        }
        else if(myIconY > 145)
        {
            myIconY = 145;
        }
        
        playerImgs[playerID-1].transform.position = new Vector3((float)myIconX, (float)myIconY, 0.0f);
        */

        //update my position to others
        photonView.RPC("UpdatePositionToOthers", RpcTarget.All, 
            (Vector3)PhotonNetwork.LocalPlayer.CustomProperties["lastLoc"],
            player.transform.position, playerID);
        
        //disable other player's icon
        for(int i = 0 ; i < 4 ; i ++)
        {
            if(i+1 == playerID)
            {
                //Debug.Log("I appears at "+iconPosToRealPos(playerImgs[i].transform.position));
                    
                playerImgs[i].SetActive(true);
            }
            else if(i+1 == playerTeamMate)
            {
                //team mate
                if(round >= 5 || player.playerInMyArea(iconPosToRealPos(playerImgs[i].transform.position)))
                {
                    //Debug.Log("teammate appears at "+iconPosToRealPos(playerImgs[i].transform.position));
                    playerImgs[i].SetActive(true);
                }
                else
                {
                    //Debug.Log("teammate disappeared. "+iconPosToRealPos(playerImgs[i].transform.position));
                    playerImgs[i].SetActive(false);
                }
            }
            else
            {
                //rivals
                if(player.playerInMyArea(iconPosToRealPos(playerImgs[i].transform.position)))
                {
                    //Debug.Log("rival appears at "+iconPosToRealPos(playerImgs[i].transform.position));
                    
                    playerImgs[i].SetActive(true);
                }
                else
                {
                    //Debug.Log("rival disappeared. "+iconPosToRealPos(playerImgs[i].transform.position));
                    
                    playerImgs[i].SetActive(false);
                }
            }
        }
    }
    public Vector3 iconPosToRealPos(Vector3 iconPos)
    {
        Vector3 realPos = new Vector3();
        realPos.y = 0.0f;
        //x: -145~-5, y: 5~145
        realPos.x = (float)((int)(iconPos.x + 5.0f/10.0f));
        realPos.z = (float)((int)(iconPos.y - 5.0f/10.0f));
        return realPos;
    }
    public void M_RoundUpdate_()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            round += 1;
            if (teamRound == 1)
            {
                //Debug.Log("Round: "+round.ToString()+"change teamRound to 2.");
                teamRound = 2;
            }
            else
            {
                //Debug.Log("Round: "+round.ToString()+"change teamRound to 1");
                teamRound = 1;
            }
            photonView.RPC("TeamRoundUpdate", RpcTarget.All, teamRound, round);
        }
        round = (int)PhotonNetwork.LocalPlayer.CustomProperties["round"];
        string newRoundText = "回合:"+((int)round).ToString();
        teamRound = (int)PhotonNetwork.LocalPlayer.CustomProperties["teamRound"];
        if (teamRound == playerTeam)
        {
            //my team's round
            //diceSliderPanel.SetActive(true);
            //Debug.Log("my team's round");
            newRoundText += "\n(進攻)";
        }
        else
        {
            //not my team's round
            player.P_SetActionPoint(0);
            //actionPointPanel.SetActive(false);
            //Debug.Log("not my team's round. my team: "+playerTeam.ToString()+", this round: "+teamRound.ToString());
            newRoundText += "\n(防守)";
        }
        roundText.text = newRoundText;
    }

    public void M_Movement(int dir)
    {
        Vector3 location = player.P_GetLocation();
        int tempActionPoint = player.P_GetActionPoint();
        PhotonNetwork.LocalPlayer.CustomProperties["lastLoc"] = location;
        //紀錄現在的location
        if (dir == 0)
        {
            location.z += 1;
            //向前走
        }
        if (dir == 1)
        {
            location.x -= 1;
            //向左走
        }
        if (dir == 2)
        {
            location.z -= 1;
            //向後走
        }
        if (dir == 3)
        {
            location.x += 1;
            //向右走
        }
        tempActionPoint -= 1;
        player.P_SetActionPoint(tempActionPoint);
        //消耗actionpoint
        player.P_SetLocation(location);
        //更動Player的Location
        //Debug.Log("x: " + location.x + ", y: " + location.y + ", z: " + location.z);
    }

    public void M_RowDice(int num)
    {
        int temp = UnityEngine.Random.Range(num, num * 6);
        M_ChangeDice(-num);
        M_GetPoint(temp);
        //tempTime = 10 - tempTime;
        //roundstate = 1;
    }

    public void M_ChangeDice(int num)
    {
        int dice = player.P_GetDiceAmount();
        //Debug.Log("GameManager:M_ChangeDice: dice: " + dice + ", num: " + num);
        //取得P_dice
        dice += num;
        //變更dice
        player.P_SetDiceAmount(dice);
        //變更P_dice
    }

    public void M_GetPoint(int num)
    {
        player.P_SetActionPoint(num);
    }


    public void M_ChangeView(Vector3 wardLocation)
    {
        List<Vector3> temps = player.P_GetWardLocations();
        //儲存p layer的wardlocationlist到temps中
        temps.Add(wardLocation);
        //將要新增的wardLocation新增到temps中
        player.P_SetWardLocations(temps);
        //將player的wardlocationlist改成temps
        //M_ChangeWardView( temps );
        //更新視野	
    }

    /*
		Method: M_ChangeView
		呼叫時需輸入要插眼的位置wardLocation
		用P_GetWardLocations暫存player所有ward的list到temps
		將wardLocation加入temps中
		用P_SetWardLocations改變player所有ward的list
		用M_ChangWardView更新View
	*/

    public void M_ChangeWardView(List<Vector3> inputs)
    {
        int[,] temp = new int[15, 15];
        foreach (Vector3 input in inputs)
        {
            int x = Convert.ToInt32(Math.Round(input.x));
            int z = Convert.ToInt32(Math.Round(input.z));
            temp = player.P_GetView();
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = z - 1; j <= z + 1; j++)
                {
                    //忽略超過地圖範圍的點
                    if (i >= 0 && j >= 0 && i < 15 && j < 15)
                    {
                        temp[i, j] = 3;
                    }
                }
            }
            //將每個ward周圍九宮格的視野改成3
        }
        player.P_SetView(temp);
    }

    public int M_GetTeamRound()
    {
        return teamRound;
    }
    public int M_GetRound()
    {
        return round;
    }
    /*
     *purpose:顯示通知(提醒) 
     * param:info:通知內容 time:通知時間 warning:是否為警告(否則為一般提醒)
    */
    public void M_ShowInfo(string info, float time = 2, bool warning = false)
    {
        infoNum++;
        if (warning)
        {
            Sprite temp = Resources.Load<Sprite>("Icon/warning");
            infoImage.GetComponent<Image>().sprite = temp;
            infoBackGroundPanel.GetComponent<Image>().color = new Color32(239, 92, 0, 255);

        }
        else
        {
            Sprite temp = Resources.Load<Sprite>("Icon/info");
            infoImage.GetComponent<Image>().sprite = temp;
            infoBackGroundPanel.GetComponent<Image>().color = new Color32(208, 245, 145, 255);
        }
        infoText.GetComponent<Text>().text = " " + info;
        infoBackGroundPanel.SetActive(true);
        StartCoroutine(ShowInfoTime());
        IEnumerator ShowInfoTime()
        {
            yield return new WaitForSeconds(time);
            infoCloseNum++;
            if (infoNum == infoCloseNum)
                infoBackGroundPanel.SetActive(false);
        }
    }
    /*
    *purpose:使用技能
    * param:itemID:道具名稱
    */
    public void M_UsingItem(string itemID, int removeID)
    {
        bool itemRemove = true;
        switch (itemID)
        {
            case "purpleA"://可使用一次，兩回合內攻擊力提升10點，可疊加。	

                player.P_SetAttack(player.P_GetAttack() + 10);
                itemEffectRound.Add(Tuple.Create("purpleA", 2));
                break;

            case "purpleB"://可使用一次，兩回合內防禦力提升10點，可疊加

                player.P_SetDefense(player.P_GetDefense() + 10);
                itemEffectRound.Add(Tuple.Create("purpleB", 2));
                break;

            case "purpleC"://可使用一次，永久使攻擊力提升5點，可疊加。

                player.P_SetAttack(player.P_GetAttack() + 5);

                break;

            case "purpleD"://可使用一次，永久使防禦力提升5點，可疊加。

                player.P_SetDefense(player.P_GetDefense() + 5);

                break;

            case "purpleE"://存在於背包中即有效果，可以向當前面對方向增加一格視野

                M_ShowInfo("不需要使用，存在於背包中即有效果。", 1);
                itemRemove = false;
                break;

            case "purpleF"://可使用一次，將該回合可用的行動點數變為三倍。

                if (!player.P_GetMoveLock() && player.P_GetActionPoint() != 0)
                {
                    player.P_SetActionPoint(player.P_GetActionPoint() * 3);
                }
                else
                {
                    M_ShowInfo("目前的狀態無法使用該道具", 1);
                    itemRemove = false;
                }
                break;


            case "blueA"://可使用一次，三回合內攻擊力提升5點，可疊加。

                player.P_SetAttack(player.P_GetAttack() + 5);
                itemEffectRound.Add(Tuple.Create("blueA", 3));
                break;

            case "blueB"://可使用一次，三回合內防禦力提升5點，可疊加。

                player.P_SetDefense(player.P_GetDefense() + 5);
                itemEffectRound.Add(Tuple.Create("blueB", 3));
                break;

            case "blueC"://可使用一次，在地上創造傳送門。
                if (!player.P_GetMoveLock() && player.P_GetActionPoint() != 0)
                {
                    if (!map.S_CreatePortal())
                        itemRemove = false;
                }
                else
                {
                    M_ShowInfo("目前的狀態無法使用該道具", 1);
                    itemRemove = false;
                }

                break;

            case "blueD"://存在於背包中即有效果，可以藉此道具在水上行動。

                M_ShowInfo("不需要使用，存在於背包中即有效果。", 1);
                itemRemove = false;
                break;

            case "blueE"://可使用一次，隨機移動到地圖上任何一個地方。

                if (!player.P_GetMoveLock() && player.P_GetActionPoint() != 0)
                    map.S_RandomTransfer();
                else
                {
                    M_ShowInfo("目前的狀態無法使用該道具", 1);
                    itemRemove = false;
                }
                break;

            case "blueF"://可使用三次，可偵測並清除九宮格範圍之敵對眼。

                break;

            case "blueG"://可使用一次，可砍伐樹木。
                if (!player.P_GetMoveLock() && player.P_GetActionPoint() != 0)
                {
                    if (!map.S_ChopTheTree())
                        itemRemove = false;
                }
                else
                {
                    M_ShowInfo("目前的狀態無法使用該道具", 1);
                    itemRemove = false;
                }

                break;

            case "greenA"://可使用一次，永久使攻擊力提升1點，可疊加。

                player.P_SetAttack(player.P_GetAttack() + 1);

                break;

            case "greenB"://可使用一次，永久使攻擊力提升2點，可疊加。

                player.P_SetAttack(player.P_GetAttack() + 2);

                break;

            case "greenC"://可使用一次，永久使防禦力提升1點，可疊加。

                player.P_SetDefense(player.P_GetDefense() + 1);

                break;

            case "greenD"://可使用一次，永久使防禦力提升2點，可疊加。

                player.P_SetDefense(player.P_GetDefense() + 2);

                break;

            case "greenE"://可使用一次，清除九宮格範圍內所有敵對眼。

                break;

            case "whiteA"://可使用一次，三回合內攻擊力提升2點，可疊加。

                player.P_SetAttack(player.P_GetAttack() + 2);
                itemEffectRound.Add(Tuple.Create("whiteA", 3));
                break;

            case "whiteB"://可使用一次，二回合內防禦力提升3點，可疊加。

                player.P_SetDefense(player.P_GetDefense() + 3);
                itemEffectRound.Add(Tuple.Create("whiteB", 3));
                break;

            case "whiteC"://可使用一次，此回合可以向當前面對方向增加一格視野距離。


                break;

            case "whiteD"://可使用一次，可得九宮格範圍之內是否有眼存在。


                break;

            case "whiteE"://可使用一次，使該回合行動點數增加三點，可疊加。

                if (!player.P_GetMoveLock() && player.P_GetActionPoint() != 0)
                {
                    player.P_SetActionPoint(player.P_GetActionPoint() + 3);
                }
                else
                {
                    M_ShowInfo("目前的狀態無法使用該道具", 1);
                    itemRemove = false;
                }
                break;

            default:

                //Debug.LogError("道具參數有誤或是未包含全部道具的實作");
                break;

        }
        //道具使用成功則從背包中移除
        if (itemRemove)
        {
            if(player.P_GetItemTimesList()[removeID] > 1)
            {
                List<int> tempTimesList = player.P_GetItemTimesList();
                tempTimesList[removeID] -= 1;
                player.P_SetItemTimesList(tempTimesList);
                map.CloseItemInfo();
            }
            else
            {
                List<Item> tempList = player.P_GetItemList();
                List<int> tempTimesList = player.P_GetItemTimesList();
                tempList.RemoveAt(removeID);
                tempTimesList.RemoveAt(removeID);
                player.P_SetItemList(tempList);
                player.P_SetItemTimesList(tempTimesList);
                map.CloseItemInfo();
                map.S_ResetCursor();
            }
        }
    }
    /*
    *purpose:使用技能
    * param:itemID:道具名稱
    */
    private string M_ItemNoEffect(string itemID)
    {
        string itemName = "";
        foreach(Item i in map.S_GetItemList())
        {
            if (i.GetID() == itemID)
            {
                itemName = i.GetName();
            }
        }
        switch (itemID)
        {
            case "purpleA"://可使用一次，兩回合內攻擊力提升10點，可疊加。	

                player.P_SetAttack(player.P_GetAttack() - 10);
                break;

            case "purpleB"://可使用一次，兩回合內防禦力提升10點，可疊加

                player.P_SetDefense(player.P_GetDefense() - 10);
                break;

            case "blueA"://可使用一次，三回合內攻擊力提升5點，可疊加。

                player.P_SetAttack(player.P_GetAttack() - 5);
                break;

            case "blueB"://可使用一次，三回合內防禦力提升5點，可疊加。

                player.P_SetDefense(player.P_GetDefense() - 5);
                break;

            case "whiteA"://可使用一次，三回合內攻擊力提升2點，可疊加。

                player.P_SetAttack(player.P_GetAttack() - 2);
                break;

            case "whiteB"://可使用一次，二回合內防禦力提升3點，可疊加。

                player.P_SetDefense(player.P_GetDefense() - 3);
                break;

            default:

                //Debug.LogError("道具參數有誤或是未包含全部道具的實作");
                break;

        }
        return itemName;
    }
    private void UpdateItemEffectState()
    {
        string noEffectItems = "";
        bool anyItemNoEffect = false;
        List<Tuple<string, int>> temp = new List<Tuple<string, int>>();
        foreach (Tuple <string, int> item in itemEffectRound)
        {
            temp.Add(new Tuple<string, int>(item.Item1, item.Item2 - 1));
            if (item.Item2 - 1 == 0)
            {
                noEffectItems = noEffectItems + " " + M_ItemNoEffect(item.Item1);
                temp.RemoveAt(temp.Count - 1);
                anyItemNoEffect = true;
            }
        }
        itemEffectRound = new List<Tuple<string, int>>(temp);
        if(anyItemNoEffect)
            M_ShowInfo(noEffectItems + " 道具效果已失效");
    }
}
