using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.Networking;


public class Map : MonoBehaviour
{
    //每個grid:2維array存各個地形或道具的分布
    //gridObj:初始化(生成)prefab的2維array
    //mapSize:地圖邊長

    private int seed = 0;
    private int[,] grassGrid, waterGrid, emptyGrid, isolatedGrid, portalGrid, puddleGrid, trapGrid, treeGrid, reedGrid, barrenGrid, storeGrid, triggeredGrid;
    private Item[,] itemGrid;
    private List<Item> itemList, purpleItemList, blueItemList, greenItemList, whiteItemList;
    private int[,] S_Terrain;
    private int[,] portalPairAuto, portalPairFromPlayer;
    private GameObject[,] gridObj, gridItemObj;
    private Item purpleA, purpleB, purpleC, purpleD, purpleE, purpleF, blueA, blueB, blueC, blueD, blueE, blueF, blueG;
    private Item greenA, greenB, greenC, greenD, greenE, whiteA, whiteB, whiteC, whiteD, whiteE;
    private int mapSize = 15, portalNumAuto, allPortalNumFromPlayer = 0;
    private int playerID = 0;
    private int storeNum;
    private int storeItemNum = 4, bagItemNum = 15;
    private int[,] storeLocation;
    private Player player;
    private GameManager gameManager;
    private GameObject mapEventCanvas, cannotFetchItemPanel, storeCanvas, itemInfoCanvas, itemInfoPanel;
    private GameObject storePanel;
    private GameObject bagCanvas, bagPanel;
    private GameObject[] storeItemObj, bagItemObj;
    private bool inStore = false, onTrap = false, onPuddle = false, inReed = false, onWater = false, confirm = false;
    private int[,] chopTreeLock , fetchItemLock;
    private int fetchItemI, fetchItemJ;
    private Dictionary<string, int> barrenRecoverRound, portalRecoverRound;
    private int roundPre, storeRoundPre;
    private Vector3 locationPre;
    public GameObject grassPrefab, trapPrefab, waterPrefab, portalPrefab, puddlePrefab, treePrefab, reedPrefab, barrenPrefab, outerGrassPrefab, storePrefab;
    public GameObject purpleAPrefab, purpleBPrefab, purpleCPrefab, purpleDPrefab, purpleEPrefab, purpleFPrefab, blueAPrefab, blueBPrefab, blueCPrefab, blueDPrefab, blueEPrefab, blueFPrefab, blueGPrefab;
    private PhotonView photonView;
    private bool myRound, isfetchingItem = false;
    [SerializeField]
    private Transform cam;
    private ArrayList storeList;
    private List<Item> storeItemList;
    private int inStoreNum;
    private int[,] itemSoldOutGrid;
    private int playerIDNum = 4;
    private bool resetStoreItem = false;
    private PlayerInfo playerInfo;
    private Dictionary<int, PlayerInfo> playersInfo;
    public CursorMode cursorMode = CursorMode.Auto;
    private GameObject boat;
    private GameObject confirmText, confirmBackGroundPanel, confirmCanvas;
    private int confirmResult = -1;
    private int fetchItemTime = 5, chopTreeTime = 3, createPortalTime = 1;
    private Vector2 initBagRect = new Vector2(-365, 148), chooseItemToDiscardRect = new Vector2(10, 84);
    public bool isChoosingItemToDiscard{ get; set; }
    public bool isArbitrarilyDiscard { get; set; }

    //barren = 0, grass = 1, puddle = 2, reed = 3, trap = 4, tree = 5, portal = 6,  water = 7, store = 8, isolated grass = 100
    //initialization
    private void Awake()
    {
        seed = Global.seed;
        Random.InitState(seed);
    }
    void Start()
    {
        //initialize each variable
        photonView = GetComponent<PhotonView>();
        playersInfo = new Dictionary<int, PlayerInfo>();

        grassGrid = new int[mapSize, mapSize];
        waterGrid = new int[mapSize, mapSize];
        emptyGrid = new int[mapSize, mapSize];
        isolatedGrid = new int[mapSize, mapSize];
        portalGrid = new int[mapSize, mapSize];
        puddleGrid = new int[mapSize, mapSize];
        trapGrid = new int[mapSize, mapSize];
        treeGrid = new int[mapSize, mapSize];
        reedGrid = new int[mapSize, mapSize];
        barrenGrid = new int[mapSize, mapSize];
        storeGrid = new int[mapSize, mapSize];
        triggeredGrid = new int[mapSize, mapSize];
        FillArray(ref emptyGrid, 1, mapSize, mapSize);
        itemGrid = new Item[mapSize, mapSize];
        chopTreeLock = new int[mapSize, mapSize];
        fetchItemLock = new int[mapSize, mapSize];
        storeItemObj = new GameObject[storeItemNum];
        bagItemObj = new GameObject[bagItemNum + 1];
        InitItem();
        isArbitrarilyDiscard = false;


        itemList = new List<Item> { purpleA, purpleB, purpleC, purpleD, purpleE, purpleF, blueA, blueB, blueC, blueD, blueE, blueF, blueG
                                                                ,greenA, greenB, greenC, greenD, greenE, whiteA, whiteB, whiteC, whiteD, whiteE};
        purpleItemList = new List<Item> { purpleA, purpleB, purpleC, purpleD, purpleE, purpleF };
        blueItemList = new List<Item> { blueA, blueB, blueC, blueD, blueE, blueF, blueG };
        greenItemList = new List<Item> { greenA, greenB, greenC, greenD, greenE };
        whiteItemList = new List<Item> { whiteA, whiteB, whiteC, whiteD, whiteE };

        gridObj = new GameObject[mapSize, mapSize];
        gridItemObj = new GameObject[mapSize, mapSize];

        barrenRecoverRound = new Dictionary<string, int>();
        portalRecoverRound = new Dictionary<string, int>();

        //生成各種地形與道具
        CreateMap();

        portalPairFromPlayer = new int[mapSize * mapSize, 5];
        for (int i = 0; i < mapSize * mapSize; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                portalPairFromPlayer[i, j] = -1;
            }
        }

    }

    //update is called once per frame
    void Update()
    {
        //定期更新地形table
        SaveToTerrain();
        GetObject();
        photonView.RPC("UpdatePlayers", RpcTarget.All, playerID, player.P_GetNickName(), (int)player.P_GetLocation().x, (int)player.P_GetLocation().z);
        //foreach (KeyValuePair<int, int> playerViewID in playersViewID)
        //{
        //    Debug.LogError(playerViewID.Key);
        //    Debug.LogError(playerViewID.Value);
        //}

        //取得回合
        if (gameManager.M_GetTeamRound() == player.P_GetTeam())
        {
            //my team's round
            myRound = true;

        }
        else
        {
            //not my team's round
            myRound = false;
        }

        //當使用者點擊
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Plane p = new Plane(Camera.main.transform.forward, transform.position);

            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;


            bool pannelBeClicked = CannotFetchItemPanel.GetIfClicked();
            bool closeBtnBeClicked = CannotFetchCloseBtn.GetIfClicked();
            bool bagBeClicked = BagItem.Click || BagBeClick.Click;
            GameObject bagPaneBackground = bagPanel.transform.Find("BackgroundImage").gameObject;

            //關閉土地資訊
            if (closeBtnBeClicked)
            {
                ResetFetchItemPanel();
                CannotFetchCloseBtn.SetClicked(false);
            }
            //點擊土地資訊pannel
            else if (pannelBeClicked)
            {
                CannotFetchItemPanel.SetClicked(false);
            }
            else if (confirmBackGroundPanel.activeSelf)
            {
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
            }
            else if (bagBeClicked)
            {
                BagItem.Click = false;
                BagBeClick.Click = false;
            }
            //點擊panel以外
            else if (Physics.Raycast(ray, out hit, 100) && !storePanel.activeSelf && !isfetchingItem)
            {
                int i = (int)System.Math.Round(hit.collider.gameObject.transform.position.x, 0), j = (int)System.Math.Round(hit.collider.gameObject.transform.position.z, 0);
                fetchItemI = i;
                fetchItemJ = j;
                //判斷是否是周圍9宮格
                if (Surrounding(i, j, (int)player.P_GetLocation().x, (int)player.P_GetLocation().z))
                {
                    //我方回合
                    if (myRound)
                    {
                        if (hit.collider.gameObject.tag == "ItemOnMap" && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
                        {
                            S_GetItem(i, j);
                        }
                        else if (hit.collider.gameObject.tag == "Barren")
                        {
                            ResetFetchItemPanel();
                            cannotFetchItemPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(mousePos.x, mousePos.y);
                            GameObject information = cannotFetchItemPanel.transform.Find("Information").gameObject;

                            Text text = information.GetComponent<Text>();

                            text.text = "不可採集狀態\n\n" + barrenRecoverRound[i.ToString() + "," + j.ToString()] + " 回合後可採集";

                            cannotFetchItemPanel.SetActive(true);
                        }
                        else if (hit.collider.gameObject.tag == "Trap" && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock()) {
                            confirmResult = -1;
                            confirmText.GetComponent<Text>().text = "是否消耗2點行動點數重設陷阱?";
                            confirmBackGroundPanel.SetActive(true);
                            Cursor.SetCursor(null, Vector2.zero, cursorMode);
                            StartCoroutine(WaiterForPlayerClickBtn());
                            IEnumerator WaiterForPlayerClickBtn()
                            {
                                while (confirmResult == -1)
                                {
                                    yield return new WaitForSeconds(0.1f);
                                }
                                if (confirmResult == 1)
                                {
                                    if (triggeredGrid[i, j] == 1)
                                    {
                                        if (player.P_GetActionPoint() <= 2)
                                            gameManager.M_ShowInfo("行動點數不足");
                                        else
                                        {
                                            player.P_SetActionPoint(player.P_GetActionPoint() - 2);
                                            S_RecoverTheTrap(i, j);
                                        }
                                    }
                                    else
                                    {
                                        gameManager.M_ShowInfo("陷阱已被其他玩家重設");
                                    }                          
                                }
                            }
                        }
                        else if (hit.collider.gameObject.tag == "Reed" && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
                        {
                            confirmResult = -1;
                            confirmText.GetComponent<Text>().text = "是否消耗2點行動點數進行搜索?";
                            confirmBackGroundPanel.SetActive(true);
                            Cursor.SetCursor(null, Vector2.zero, cursorMode);
                            StartCoroutine(WaiterForPlayerClickBtn());
                            IEnumerator WaiterForPlayerClickBtn()
                            {
                                while (confirmResult == -1)
                                {
                                    yield return new WaitForSeconds(0.1f);
                                }
                                if (confirmResult == 1)
                                {
                                    if (player.P_GetActionPoint() <= 2)
                                        gameManager.M_ShowInfo("行動點數不足");
                                    else
                                    {
                                        player.P_SetActionPoint(player.P_GetActionPoint() - 2);
                                        S_Search(i, j);
                                    }

                                }
                            }
                        }
                        else if (hit.collider.gameObject.tag == "Portal" && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
                        {
                            confirmResult = -1;
                            confirmText.GetComponent<Text>().text = "是否消耗4點行動點數破壞該傳送門?";
                            confirmBackGroundPanel.SetActive(true);
                            Cursor.SetCursor(null, Vector2.zero, cursorMode);
                            StartCoroutine(WaiterForPlayerClickBtn());
                            IEnumerator WaiterForPlayerClickBtn()
                            {
                                while (confirmResult == -1)
                                {
                                    yield return new WaitForSeconds(0.1f);
                                }
                                if (confirmResult == 1)
                                {
                                    if(portalGrid[i, j] == 1)
                                    {
                                        if (player.P_GetActionPoint() <= 4)
                                            gameManager.M_ShowInfo("行動點數不足");
                                        else
                                        {
                                            player.P_SetActionPoint(player.P_GetActionPoint() - 4);
                                            S_DestroyPortal(i, j);
                                        }
                                    }
                                    else
                                    {
                                        gameManager.M_ShowInfo("傳送門已被其他玩家破壞");
                                    }
                                }
                            }
                        }
                        else
                        {
                            ResetFetchItemPanel();
                        }
                    }
                    //對方回合
                    else if (!myRound)
                    {
                        if (hit.collider.gameObject.tag == "Grass Normal")
                        {
                            if (fetchItemLock[fetchItemI, fetchItemJ] == 1)
                            {
                                gameManager.M_ShowInfo("其他玩家正在採集此地塊");
                            }
                            else if (grassGrid[fetchItemI, fetchItemJ] == 1)
                            {
                                Cursor.SetCursor(null, Vector2.zero, cursorMode);
                                S_FetchItem(fetchItemI, fetchItemJ);
                            }
                            else if (trapGrid[fetchItemI, fetchItemJ] == 1)
                            {
                                S_StepOnTheTrap(fetchItemI, fetchItemJ);
                            }

                        }
                        else if (hit.collider.gameObject.tag == "Barren")
                        {
                            ResetFetchItemPanel();
                            cannotFetchItemPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(mousePos.x, mousePos.y);
                            GameObject information = cannotFetchItemPanel.transform.Find("Information").gameObject;

                            Text text = information.GetComponent<Text>();

                            text.text = "不可採集狀態\n\n" + barrenRecoverRound[i.ToString() + "," + j.ToString()] + " 回合後可採集";

                            cannotFetchItemPanel.SetActive(true);
                        }
                        else
                        {
                            ResetFetchItemPanel();
                        }
                    }

                }
                else
                {
                    ResetFetchItemPanel();
                }

            }
        }

        if (roundPre != gameManager.M_GetRound())
        {
            UpdateBarrenState();
            UpdatePortalState();
            ResetFetchItemPanel();
            resetStoreItem = false;
            if (!isfetchingItem)
                player.P_SetMoveLock(false);

            ForceMouseOn();
            roundPre = gameManager.M_GetRound();
        }

        if (locationPre != player.P_GetLocation())
        {
            inStore = false;
            onTrap = false;
            onPuddle = false;
            inReed = false;
            if (onWater)
            {
                float offset = 0;
                if (player.GetComponent<Transform>().rotation.eulerAngles.y == 90 || player.GetComponent<Transform>().rotation.eulerAngles.y == 270)
                    offset = 0.15f;
                boat.GetComponent<Transform>().position = new Vector3(player.P_GetLocation().x, 0, player.P_GetLocation().z - offset);
                boat.GetComponent<Transform>().rotation = Quaternion.Euler(0, player.GetComponent<Transform>().rotation.eulerAngles.y, 0);

            }
            photonView.RPC("ShowUpUpdateView", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
            storePanel.SetActive(false);
            confirmBackGroundPanel.SetActive(false);
            ResetFetchItemPanel();
            locationPre = player.P_GetLocation();
        }

        int playerLocationI = (int)player.P_GetLocation().x, playerLocationJ = (int)player.P_GetLocation().z;
        if (trapGrid[playerLocationI, playerLocationJ] == 1 && !onTrap)
        {
            onTrap = true;
            S_StepOnTheTrap(playerLocationI, playerLocationJ);
        }
        if (puddleGrid[playerLocationI, playerLocationJ] == 1 && !onPuddle) {
            onPuddle = true;
            S_StepOnThePuddle(playerLocationI, playerLocationJ);
        }
        //if (reedGrid[playerLocationI, playerLocationJ] == 1 && !inReed)
        //{
        //    inReed = true;
        //    photonView.RPC("HideUpdateView", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
        //}
        if (waterGrid[playerLocationI, playerLocationJ] == 1 && !onWater)
        {
            onWater = true;
            float offset = 0;
            if (System.Math.Floor(player.GetComponent<Transform>().rotation.eulerAngles.y) == 90 || System.Math.Ceiling(player.GetComponent<Transform>().rotation.eulerAngles.y) == -90)
                offset = 0.1f;
            boat = PhotonNetwork.Instantiate("Item/Boat", new Vector3(player.P_GetLocation().x, 0, player.P_GetLocation().z - offset), Quaternion.Euler(0, player.GetComponent<Transform>().rotation.eulerAngles.y, 0), 0);
        }
        if (waterGrid[playerLocationI, playerLocationJ] != 1)
        {
            if (onWater == true)
            {
                onWater = false;
                PhotonNetwork.Destroy(boat);
            }
        }
        if (storeGrid[playerLocationI, playerLocationJ] == 1 && inStore == false)
        {
            inStore = true;
            for (int x = 0; x < storeNum; x++)
            {
                if (storeLocation[x, 0] == playerLocationI && storeLocation[x, 1] == playerLocationJ)
                {
                    inStoreNum = x;
                    List<Item> tempItemList = new List<Item>((List<Item>)storeList[x]);
                    for (int i = 0; i < storeItemNum; i++)
                    {
                        Texture temp = (Texture2D)Resources.Load("Item/Icon/" + tempItemList[i].GetResourceName());
                        storeItemObj[i].transform.Find("ItemImage" + (i + 1).ToString()).GetComponent<RawImage>().texture = temp;
                        storeItemObj[i].transform.Find("ItemName" + (i + 1).ToString()).GetComponent<Text>().text = tempItemList[i].GetName();
                        storeItemObj[i].transform.Find("Price" + (i + 1).ToString()).GetComponent<Text>().text = tempItemList[i].GetPrice().ToString();
                        storeItemObj[i].transform.Find("SoldOutItem" + (i + 1).ToString()).gameObject.SetActive(false);
                        if (itemSoldOutGrid[x, i] == 1)
                        {
                            storeItemObj[i].transform.Find("SoldOutItem" + (i + 1).ToString()).gameObject.SetActive(true);
                        }
                    }
                    break;
                }
            }

            storePanel.SetActive(true);
        }

        if (inStore)
        {
            if (!myRound)
            {
                CloseStore();
                CloseItemInfo();
            }
            else
            {
                for (int i = 0; i < storeItemNum; i++)
                {
                    if (itemSoldOutGrid[inStoreNum, i] == 1)
                    {
                        storeItemObj[i].transform.Find("SoldOutItem" + (i + 1).ToString()).gameObject.SetActive(true);
                    }
                }
            }

        }

        if (portalGrid[playerLocationI, playerLocationJ] == 1 && !portalRecoverRound.ContainsKey(playerLocationI.ToString() + "," + playerLocationJ.ToString()))
        {
            int transferI = S_GetTransferLocation(playerLocationI, playerLocationJ).Item1, transferJ = S_GetTransferLocation(playerLocationI, playerLocationJ).Item2;
            if (transferI != -1 && transferJ != -1)
            {
                photonView.RPC("UpdatePortalStatus", RpcTarget.All, playerLocationI, playerLocationJ, transferI, transferJ);  
                player.P_SetMoveLock(true);
                gameManager.M_ShowInfo("傳送中", 1);
                StartCoroutine(ReleaseMoveLock());
                IEnumerator ReleaseMoveLock()
                {
                    yield return new WaitForSeconds(1);
                    player.P_SetLocation(new Vector3(transferI, 0, transferJ));
                    player.P_SetMoveLock(false);
                }
            }

        }
        

        if (gameManager.M_GetRound() % 5 == 0 && resetStoreItem == false)
        {
            CreateStoreItem();
            resetStoreItem = true;
        }

        if (confirmBackGroundPanel.activeSelf)
            confirm = true;
        else
            confirm = false;

        //player與地形行走互動與限制
        MoveOnMapControl();

        UpdateBagItem();

        if(player.P_GetItemListSize() > player.P_GetItemAmountMax())
        {
            ChooseItemToDiscard();
        }

    }
    [PunRPC]
    private void UpdatePortalStatus(int playerLocationI, int playerLocationJ, int transferI, int transferJ)
    {
        portalRecoverRound.Add(playerLocationI.ToString() + "," + playerLocationJ.ToString(), 5);
        portalRecoverRound.Add(transferI.ToString() + "," + transferJ.ToString(), 5);
    }

    /*
     purpose:取得i row j col的地形
     */
    public int S_GetTerrain(int i, int j)
    {
        return (S_Terrain[i, j]);
    }

    /*
     purpose:踩到陷阱, i row j col 陷阱被觸發
    */
    public void S_StepOnTheTrap(int i, int j)
    {
        if (trapGrid[i, j] == 1)
        {
            photonView.RPC("UpdateStepOnTheTrapMap", RpcTarget.All, i, j);
            player.P_SetMoveLock(true);
            player.P_SetActionPoint(0);
            gameManager.M_ShowInfo("踩到陷阱", 3, true);
        }
    }
    [PunRPC]
    private void UpdateStepOnTheTrapMap(int i, int j)
    {
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(trapPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        triggeredGrid[i, j] = 1;
    }
    public void S_RecoverTheTrap(int i, int j)
    {
        photonView.RPC("UpdateRecoverTheTrapMap", RpcTarget.All, i, j);
        gameManager.M_ShowInfo("陷阱已重新設置", 1);

    }
    [PunRPC]
    private void UpdateRecoverTheTrapMap(int i, int j)
    {
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        triggeredGrid[i, j] = 0;
    }
    /*
     purpose:踩到水坑
    */
    public void S_StepOnThePuddle(int i, int j)
    {
        player.P_SetMoveLock(true);
        gameManager.M_ShowInfo("踩到水坑", 1, true);
        player.P_SetActionPoint(player.P_GetActionPoint() - 1);
        StartCoroutine(ReleaseMoveLock());
        IEnumerator ReleaseMoveLock()
        {
            yield return new WaitForSeconds(1);
            player.P_SetMoveLock(false);
        }
    }
    /*
     purpose:採集, i row j col 生成採集後的土壤
    */

    public void S_FetchItem(int i, int j)
    {
        isfetchingItem = true;
        StartCoroutine(Fetch());
        player.P_SetMoveLock(true);
        IEnumerator Fetch()
        {
            photonView.RPC("SetFetchItemLock", RpcTarget.All, i, j);
            for (int x = 0; x < fetchItemTime - 1; x++)
            {
                int times = x % 3 + 1;
                gameManager.M_ShowInfo("採集中" + new string('.', times), 1);
                yield return new WaitForSeconds(1);
            }
            player.P_SetMoveLock(false);
            photonView.RPC("UpdateFetchBarrenMap", RpcTarget.All, i, j);
            isfetchingItem = false;
            ForceMouseOn();
            int randomResult = Random.Range(0, 100);
            if (randomResult < 80)
            {
                randomResult = Random.Range(0, 100);
                if (randomResult <= Item.GetPurplePossibility())
                {
                    randomResult = Random.Range(0, purpleItemList.Count);
                    player.P_AddItem(purpleItemList[randomResult]);
                    gameManager.M_ShowInfo("採集到 : " + purpleItemList[randomResult].GetName());
                }
                else if (randomResult > Item.GetPurplePossibility() && randomResult < Item.GetPurplePossibility() + Item.GetBluePossibility())
                {
                    randomResult = Random.Range(0, blueItemList.Count);
                    player.P_AddItem(blueItemList[randomResult]);
                    gameManager.M_ShowInfo("採集到 : " + blueItemList[randomResult].GetName());
                }
                else if (randomResult >= (Item.GetPurplePossibility() + Item.GetBluePossibility()) && randomResult < (Item.GetPurplePossibility() + Item.GetBluePossibility() + Item.GetGreenPossibility()))
                {
                    randomResult = Random.Range(0, greenItemList.Count);
                    player.P_AddItem(greenItemList[randomResult]);
                    gameManager.M_ShowInfo("採集到 : " + greenItemList[randomResult].GetName());
                }
                else if (randomResult >= (Item.GetPurplePossibility() + Item.GetBluePossibility() + Item.GetGreenPossibility()))
                {
                    randomResult = Random.Range(0, whiteItemList.Count);
                    player.P_AddItem(whiteItemList[randomResult]);
                    gameManager.M_ShowInfo("採集到 : " + whiteItemList[randomResult].GetName());
                }

            }
            else
            {
                gameManager.M_ShowInfo("沒有採集到任何道具");
            }


        }

    }

    public bool S_GetIfIsFetchingItem()
    {
        return isfetchingItem;
    }
    [PunRPC]
    private void SetFetchItemLock(int i, int j)
    {
        fetchItemLock[i, j] = 1;
    }
    [PunRPC]
    private void UpdateFetchBarrenMap(int i, int j)
    {
        grassGrid[i, j] = 0;
        barrenGrid[i, j] = 1;
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(barrenPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        barrenRecoverRound.Add(i.ToString() + "," + j.ToString(), 5);
        fetchItemLock[i, j] = 0;
    }

    public void S_GetItem(int i, int j)
    {

        player.P_AddItem(itemGrid[i, j]);
        gameManager.M_ShowInfo("拾取 : " + itemGrid[i, j].GetName());
        photonView.RPC("UpdateItemOnMap", RpcTarget.All, i, j);
    }
    [PunRPC]
    private void UpdateItemOnMap(int i, int j)
    {
        itemGrid[i, j] = null;
        Destroy(gridItemObj[i, j]);
    }
    public void S_Search(int i, int j)
    {
        bool searchOtherPlayer = false;
        int[] beSearchedList = new int[playerIDNum + 1];
        foreach (KeyValuePair<int, PlayerInfo> playerInfo in playersInfo)
        {
            //Debug.LogError(playerInfo.Key);
            //Debug.LogError(playerInfo.Value.nickName);
            //Debug.LogError(playerInfo.Value.locationI);
            //Debug.LogError(playerInfo.Value.locationJ);
            //Debug.LogError(i);
            //Debug.LogError(j);
            if (playerInfo.Value.locationI == i && playerInfo.Value.locationJ == j && playerInfo.Key != playerID)
            {
                gameManager.M_ShowInfo("搜索出敵人 : " + playerInfo.Value.nickName);
                searchOtherPlayer = true;
                beSearchedList[playerInfo.Key] = 1;
            }
        }

        photonView.RPC("BeSearched", RpcTarget.All, beSearchedList);
        if (!searchOtherPlayer)
        {
            gameManager.M_ShowInfo("沒有搜索到任何人", 1);
        }
    }
    /*
    purpose:過數個回合後，被採集的土壤恢復
   */
    public void S_BarrenRecover(int i, int j)
    {
        grassGrid[i, j] = 1;
        barrenGrid[i, j] = 0;
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
    }
    /*
    purpose:砍樹，樹變成grass
   */
    public bool S_ChopTheTree()
    {
        int frontI = GetFrontLocation((int)player.P_GetLocation().x, (int)player.P_GetLocation().z, (int)player.GetComponent<Transform>().rotation.eulerAngles.y).Item1;
        int frontJ = GetFrontLocation((int)player.P_GetLocation().x, (int)player.P_GetLocation().z, (int)player.GetComponent<Transform>().rotation.eulerAngles.y).Item2;
        if (chopTreeLock[frontI, frontJ] == 1)
        {
            gameManager.M_ShowInfo("其他玩家正在砍伐此樹木");
            return false;
        }
        else if (treeGrid[frontI, frontJ] == 1 && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
        {
            photonView.RPC("SetChopTreeLock", RpcTarget.All, frontI, frontJ);
            player.P_SetMoveLock(true);
            StartCoroutine(Chop());
            IEnumerator Chop()
            {
                for (int x = 0; x < chopTreeTime - 1; x++)
                {
                    int times = x % 3 + 1;
                    gameManager.M_ShowInfo("砍伐中" + new string('.', times), 1);
                    yield return new WaitForSeconds(1);
                }
                photonView.RPC("UpdateTreeMap", RpcTarget.All, frontI, frontJ);
                player.P_SetMoveLock(false);
                int randomResult = Random.Range(0, 100);
                if (randomResult < 80)
                {
                    randomResult = Random.Range(0, 100);
                    if (randomResult <= Item.GetPurplePossibility())
                    {
                        randomResult = Random.Range(0, purpleItemList.Count);
                        player.P_AddItem(purpleItemList[randomResult]);
                        gameManager.M_ShowInfo("取得樹上掉落的 : " + purpleItemList[randomResult].GetName());
                    }
                    else if (randomResult > Item.GetPurplePossibility() && randomResult < Item.GetPurplePossibility() + Item.GetBluePossibility())
                    {
                        randomResult = Random.Range(0, blueItemList.Count);
                        player.P_AddItem(blueItemList[randomResult]);
                        gameManager.M_ShowInfo("取得樹上掉落的 : " + blueItemList[randomResult].GetName());
                    }
                    else if (randomResult >= (Item.GetPurplePossibility() + Item.GetBluePossibility()) && randomResult < (Item.GetPurplePossibility() + Item.GetBluePossibility() + Item.GetGreenPossibility()))
                    {
                        randomResult = Random.Range(0, greenItemList.Count);
                        player.P_AddItem(greenItemList[randomResult]);
                        gameManager.M_ShowInfo("取得樹上掉落的 : " + greenItemList[randomResult].GetName());
                    }
                    else if (randomResult >= (Item.GetPurplePossibility() + Item.GetBluePossibility() + Item.GetGreenPossibility()))
                    {
                        randomResult = Random.Range(0, whiteItemList.Count);
                        player.P_AddItem(whiteItemList[randomResult]);
                        gameManager.M_ShowInfo("取得樹上掉落的 : " + whiteItemList[randomResult].GetName());
                    }
                }
            }
            return true;
        }
        else if (treeGrid[frontI, frontJ] == 0 && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
        {
            gameManager.M_ShowInfo("前方沒有樹木");
            return false;
        }
        return false;
    }
    [PunRPC]
    private void SetChopTreeLock(int i, int j)
    {
        chopTreeLock[i, j] = 1;
    }
    [PunRPC]
    private void UpdateTreeMap(int i, int j)
    {
        treeGrid[i, j] = 0;
        grassGrid[i, j] = 1;
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        chopTreeLock[i, j] = 0;
    }
    /*
     purpose:建造傳送門
     */
    public bool S_CreatePortal()
    {
        int frontI = GetFrontLocation((int)player.P_GetLocation().x, (int)player.P_GetLocation().z, (int)player.GetComponent<Transform>().rotation.eulerAngles.y).Item1;
        int frontJ = GetFrontLocation((int)player.P_GetLocation().x, (int)player.P_GetLocation().z, (int)player.GetComponent<Transform>().rotation.eulerAngles.y).Item2;
        if (player.P_GetActionPoint() != 0 && (grassGrid[frontI, frontJ] == 1 || trapGrid[frontI, frontJ] == 1) && !player.P_GetMoveLock())
        {
            if(trapGrid[frontI, frontJ] == 1)
            {
                S_StepOnTheTrap(frontI, frontJ);
                return false;
            }
            else
            {
                player.P_SetMoveLock(true);
                StartCoroutine(Create());
                IEnumerator Create()
                {
                    for (int x = 0; x < createPortalTime - 1; x++)
                    {
                        int times = x % 3 + 1;
                        gameManager.M_ShowInfo("創造中" + new string('.', times), 1);
                        yield return new WaitForSeconds(1);
                    }
                    photonView.RPC("CreateUpdatePortalMap", RpcTarget.All, frontI, frontJ, playerID);
                    player.P_SetMoveLock(false);
                }
                return true;
            }
        }
        else if (player.P_GetActionPoint() != 0 && grassGrid[frontI, frontJ] != 1 && !player.P_GetMoveLock())
        {
            gameManager.M_ShowInfo("前方不可創造傳送門");
            return false;
        }
        return false;
    }
    [PunRPC]
    public void CreateUpdatePortalMap(int i, int j, int playerID)
    {
        grassGrid[i, j] = 0;
        portalGrid[i, j] = 1;
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(portalPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        bool newPair = true, single = true;
        for (int x = 0; x < allPortalNumFromPlayer; x++)
        {
            if (portalPairFromPlayer[x, 0] == playerID && portalPairFromPlayer[x, 1] != -1 && portalPairFromPlayer[x, 2] != -1 && portalPairFromPlayer[x, 3] == -1 && portalPairFromPlayer[x, 4] == -1)
            {
                portalPairFromPlayer[x, 3] = i;
                portalPairFromPlayer[x, 4] = j;
                for (int y = 0; y < allPortalNumFromPlayer; y++)
                {
                    if (portalPairFromPlayer[y, 3] == portalPairFromPlayer[x, 1] && portalPairFromPlayer[y, 4] == portalPairFromPlayer[x, 2])
                    {
                        portalPairFromPlayer[y, 1] = i;
                        portalPairFromPlayer[y, 2] = j;
                        newPair = false;
                    }
                }
                if (newPair)
                {
                    portalPairFromPlayer[allPortalNumFromPlayer, 0] = playerID;
                    portalPairFromPlayer[allPortalNumFromPlayer, 1] = i;
                    portalPairFromPlayer[allPortalNumFromPlayer, 2] = j;
                    portalPairFromPlayer[allPortalNumFromPlayer, 3] = portalPairFromPlayer[x, 1];
                    portalPairFromPlayer[allPortalNumFromPlayer, 4] = portalPairFromPlayer[x, 2];
                    allPortalNumFromPlayer++;
                }
                single = false;
                break;
            }
        }
        if (single)
        {
            portalPairFromPlayer[allPortalNumFromPlayer, 0] = playerID;
            portalPairFromPlayer[allPortalNumFromPlayer, 1] = i;
            portalPairFromPlayer[allPortalNumFromPlayer, 2] = j;
            portalPairFromPlayer[allPortalNumFromPlayer, 3] = -1;
            portalPairFromPlayer[allPortalNumFromPlayer, 4] = -1;
            allPortalNumFromPlayer++;
        }
    }

    /*
    purpose:破壞傳送門
    */
    public void S_DestroyPortal(int i, int j)
    {
        photonView.RPC("DestroyUpdatePortalMap", RpcTarget.All, i, j);
    }
    [PunRPC]
    public void DestroyUpdatePortalMap(int i, int j)
    {
        grassGrid[i, j] = 1;
        portalGrid[i, j] = 0;
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;

        for (int x = 0; x < portalNumAuto; x++)
        {
            if (portalPairAuto[x, 0] == i && portalPairAuto[x, 1] == j)
            {
                portalPairAuto[x, 0] = -1;
                portalPairAuto[x, 1] = -1;
                portalRecoverRound.Remove(i.ToString() + "," + j.ToString());
                portalRecoverRound.Remove(portalPairAuto[x, 2].ToString() + "," + portalPairAuto[x, 3].ToString());
            }
            if (portalPairAuto[x, 2] == i && portalPairAuto[x, 3] == j)
            {
                portalPairAuto[x, 2] = -1;
                portalPairAuto[x, 3] = -1;
            }
            
            
        }
        for (int x = 0; x < allPortalNumFromPlayer; x++)
        {
            if (portalPairFromPlayer[x, 1] == i && portalPairFromPlayer[x, 2] == j)
            {
                portalPairFromPlayer[x, 1] = -1;
                portalPairFromPlayer[x, 2] = -1;
                portalRecoverRound.Remove(i.ToString() + "," + j.ToString());
                portalRecoverRound.Remove(portalPairFromPlayer[x, 3].ToString() + "," + portalPairFromPlayer[x, 4].ToString());
            }
            if (portalPairFromPlayer[x, 3] == i && portalPairFromPlayer[x, 4] == j)
            {
                portalPairFromPlayer[x, 3] = -1;
                portalPairFromPlayer[x, 4] = -1;
            }
        }
        
    }
    /*
     purpose:取得i row j col傳送門所傳送的地點
     如果沒有對應的門 回傳 -1 -1
     */
    public (int, int) S_GetTransferLocation(int i, int j)
    {
        for (int x = 0; x < portalNumAuto; x++)
        {
            if (portalPairAuto[x, 0] == i && portalPairAuto[x, 1] == j)
            {
                return (portalPairAuto[x, 2], portalPairAuto[x, 3]);
            }
        }
        for (int x = 0; x < allPortalNumFromPlayer; x++)
        {
            if (portalPairFromPlayer[x, 1] == i && portalPairFromPlayer[x, 2] == j)
            {
                return (portalPairFromPlayer[x, 3], portalPairFromPlayer[x, 4]);
            }
        }
        return (-1, -1);
    }
    public bool S_GetIfConfirm()
    {
        return confirm;
    }
    public void S_RandomTransfer()
    {
        int i = Random.Range(0, 15);
        int j = Random.Range(0, 15);

        while (!(reedGrid[i, j] == 1 || grassGrid[i, j] == 1 || puddleGrid[i, j] == 1 || storeGrid[i, j] == 1 || trapGrid[i, j] == 1 || portalGrid[i, j] == 1))
        {
            i = Random.Range(0, mapSize);
            j = Random.Range(0, mapSize);
        }
        player.P_SetMoveLock(true);
        gameManager.M_ShowInfo("傳送中", 1);
        StartCoroutine(ReleaseMoveLock());
        IEnumerator ReleaseMoveLock()
        {
            yield return new WaitForSeconds(1);
            player.P_SetMoveLock(false);
            player.P_SetLocation(new Vector3(i, 0, j));
            if (reedGrid[i , j] == 1)
            {
                inReed = true;
                photonView.RPC("HideUpdateView", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
                locationPre = player.P_GetLocation();
            }
            
        }
    }
    private void CreateMap()
    {
        CreateGrass();
        CreateWater();
        FillGrass();
        CreateWater();
        FillWater();
        FindIsolatedGrass();
        RemoveOnlyOneWater();
        FillEmpty();
        CreatePortal();
        CreatePuddle();
        CreateTrap();
        CreateTree();
        CreateReed();
        CreateBarren();
        CreateStore();
        CreateItem();
        CreateOuterGrass();

    }

    public void ShowItemInfo(GameObject itemImage)
    {

        string resourceName = itemImage.GetComponent<RawImage>().texture.name.ToString();

        foreach (Item item in itemList)
        {
            if (item.GetResourceName() == resourceName)
            {
                itemInfoPanel.transform.GetChild(0).Find("ItemNameText").gameObject.GetComponent<Text>().text = item.GetName();
                itemInfoPanel.transform.GetChild(1).Find("ItemInfoText").gameObject.GetComponent<Text>().text = item.GetDescription();
                itemInfoPanel.SetActive(true);
                break;
            }
        }
        //double anchorPositionX = itemImage.GetComponent<RectTransform>().position.x + 0.3 * itemImage.GetComponent<RectTransform>().rect.width;
        //double anchorPositionY = itemImage.GetComponent<RectTransform>().position.y - 0.3 * itemImage.GetComponent<RectTransform>().rect.height;
        double anchorPositionX = itemImage.GetComponent<RectTransform>().position.x + 0.15 * itemImage.GetComponent<RectTransform>().rect.width;
        double anchorPositionY = itemImage.GetComponent<RectTransform>().position.y - 0.15 * itemImage.GetComponent<RectTransform>().rect.height;
        itemInfoPanel.GetComponent<RectTransform>().position = new Vector2((float)anchorPositionX, (float)anchorPositionY);
    }
    private void ForceMouseOn()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (grassGrid[i, j] == 1 && Surrounding(i, j, (int)player.P_GetLocation().x, (int)player.P_GetLocation().z))
                {
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
                else if (barrenGrid[i, j] == 1 && Surrounding(i, j, (int)player.P_GetLocation().x, (int)player.P_GetLocation().z))
                {
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(barrenPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
                else if (reedGrid[i, j] == 1 && Surrounding(i, j, (int)player.P_GetLocation().x, (int)player.P_GetLocation().z))
                {
                    float temp = gridObj[i, j].GetComponent<Transform>().rotation.eulerAngles.y;
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(reedPrefab, transform.position + new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, temp, 0)));
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    public void CloseItemInfo()
    {
        itemInfoPanel.SetActive(false);
    }

    public void BuyItem(int itemNum)
    {
        int price = int.Parse(storeItemObj[itemNum - 1].transform.Find("Price" + (itemNum).ToString()).GetComponent<Text>().text);
        if (player.P_GetDiceAmount() > price)
        {
            player.P_SetDiceAmount(player.P_GetDiceAmount() - price);
            foreach (Item i in itemList)
            {
                if (i.GetName() == storeItemObj[itemNum - 1].transform.Find("ItemName" + itemNum.ToString()).GetComponent<Text>().text.ToString())
                {
                    player.P_AddItem(i);
                    gameManager.M_ShowInfo("成功購買" + i.GetName(), 1);
                }
            }
            photonView.RPC("ItemSoldOut", RpcTarget.All, inStoreNum, itemNum);
            storeItemObj[itemNum - 1].transform.Find("SoldOutItem" + itemNum.ToString()).gameObject.SetActive(true);
        }
        else
        {
            gameManager.M_ShowInfo("骰子數不足");
        }
    }
    [PunRPC]
    private void ItemSoldOut(int inStoreNum, int itemNum)
    {
        itemSoldOutGrid[inStoreNum, itemNum - 1] = 1;
    }
    //關閉商店
    public void CloseStore()
    {
        storePanel.SetActive(false);
    }
    //開啟包包
    public void BagControl()
    {
        if (!bagPanel.activeSelf)
        {
            bagPanel.GetComponent<RectTransform>().anchoredPosition = initBagRect;
            for (int i = 0; i < bagItemNum; i++)
            {
                bagItemObj[i].transform.Find("ItemImage".ToString()).gameObject.SetActive(false);
                bagItemObj[i].transform.Find("ItemImageBackground".ToString()).gameObject.SetActive(false);
                bagItemObj[i].transform.Find("BeDragCover".ToString()).gameObject.SetActive(false);
                bagItemObj[i].transform.Find("NotOpenImage".ToString()).gameObject.SetActive(false);
                if (i < player.P_GetItemList().Count)
                {
                    bagItemObj[i].transform.Find("ItemImage".ToString()).gameObject.SetActive(true);
                    bagItemObj[i].transform.Find("ItemImageBackground".ToString()).gameObject.SetActive(true);
                    bagItemObj[i].transform.Find("BeDragCover".ToString()).gameObject.SetActive(true);
                    Texture temp = (Texture2D)Resources.Load("Item/Icon/" + player.P_GetItemList()[i].GetResourceName());
                    bagItemObj[i].transform.Find("ItemImage".ToString()).GetComponent<RawImage>().texture = temp;
                    bagItemObj[i].transform.Find("ItemImageBackground".ToString()).GetComponent<RawImage>().texture = temp;

                }
                else if (i >= player.P_GetItemAmountMax() && i < bagItemNum)
                {
                    bagItemObj[i].transform.Find("NotOpenImage".ToString()).gameObject.SetActive(true);
                }
            }
            bagPanel.SetActive(true);
        }
        else
        {
            bagPanel.SetActive(false);
        }
        
        
    }
    //關閉包包
    public void CloseBag()
    {
        bagPanel.SetActive(false);
    }
    //更新荒蕪地形狀態
    private void UpdateBarrenState()
    {
        Dictionary<string, int> temp = new Dictionary<string, int>(barrenRecoverRound);
        foreach (KeyValuePair<string, int> item in barrenRecoverRound)
        {
            temp[item.Key] = item.Value - 1;
            if (temp[item.Key] == 0)
            {
                int i = int.Parse(item.Key.Substring(0, item.Key.IndexOf(",")));
                int j = int.Parse(item.Key.Substring(item.Key.IndexOf(",") + 1));

                S_BarrenRecover(i, j);
                temp.Remove(item.Key);
            }
        }
        barrenRecoverRound = new Dictionary<string, int>(temp);
    }
    //更新傳送門狀態
    private void UpdatePortalState()
    {
        Dictionary<string, int> temp = new Dictionary<string, int>(portalRecoverRound);
        foreach (KeyValuePair<string, int> item in portalRecoverRound)
        {
            temp[item.Key] = item.Value - 1;
            if (temp[item.Key] == 0)
            {
                int i = int.Parse(item.Key.Substring(0, item.Key.IndexOf(",")));
                int j = int.Parse(item.Key.Substring(item.Key.IndexOf(",") + 1));

                temp.Remove(item.Key);
            }
        }
        portalRecoverRound = new Dictionary<string, int>(temp);
    }
    //初始化道具
    private void InitItem()
    {
        purpleA = new Item("purpleA", "PurpleA", "PurpleA", "可使用一次，兩回合內攻擊力提升10點，可疊加。", "purple", 5);
        purpleB = new Item("purpleB", "PurpleB", "PurpleB", "可使用一次，兩回合內防禦力提升10點，可疊加。", "purple", 5);
        purpleC = new Item("purpleC", "PurpleC", "PurpleC", "可使用一次，永久使攻擊力提升5點，可疊加。", "purple", 5);
        purpleD = new Item("purpleD", "PurpleD", "PurpleD", "可使用一次，永久使防禦力提升5點，可疊加。", "purple", 5);
        purpleE = new Item("purpleE", "PurpleE", "PurpleE", "存在於背包中即有效果，可以向當前面對方向增加一格視野距離，不可疊加。", "purple", 5);
        purpleF = new Item("purpleF", "PurpleF", "PurpleF", "可使用一次，將該回合可用的行動點數變為三倍。", "purple", 5);

        blueA = new Item("blueA", "BlueA", "BlueA", "可使用一次，三回合內攻擊力提升5點，可疊加。", "blue", 4);
        blueB = new Item("blueB", "BlueB", "BlueB", "可使用一次，三回合內防禦力提升5點，可疊加。", "blue", 4);
        blueC = new Item("blueC", "BlueC", "BlueC", "可使用一次，在地上創造傳送門。", "blue", 4);
        blueD = new Item("blueD", "BlueD", "BlueD", "存在於背包中即有效果，可以藉此道具在水上行動。", "blue", 4);
        blueE = new Item("blueE", "BlueE", "BlueE", "可使用一次，隨機移動到地圖上任何一個地方。", "blue", 4);
        blueF = new Item("blueF", "BlueF", "BlueF", "可使用三次，可偵測並清除九宮格範圍之敵對眼。", "blue", 4);
        blueG = new Item("blueG", "BlueG", "BlueG", "可使用一次，可砍伐樹木。", "blue", 4);

        greenA = new Item("greenA", "GreenA", "GreenA", "可使用一次，永久使攻擊力提升1點，可疊加。", "green", 3);
        greenB = new Item("greenB", "GreenB", "GreenB", "可使用一次，永久使攻擊力提升2點，可疊加。", "green", 3);
        greenC = new Item("greenC", "GreenC", "GreenC", "可使用一次，永久使防禦力提升1點，可疊加。", "green", 3);
        greenD = new Item("greenD", "GreenD", "GreenD", "可使用一次，永久使防禦力提升1點，可疊加。", "green", 3);
        greenE = new Item("greenE", "GreenE", "GreenE", "可使用一次，清除九宮格範圍內所有敵對眼。", "green", 3);

        whiteA = new Item("whiteA", "WhiteA", "WhiteA", "可使用一次，三回合內攻擊力提升2點，可疊加。", "green", 2);
        whiteB = new Item("whiteB", "WhiteB", "WhiteB", "可使用一次，二回合內防禦力提升3點，可疊加。", "green", 2);
        whiteC = new Item("whiteC", "WhiteC", "WhiteC", "可使用一次，此回合可以向當前面對方向增加一格視野距離。", "green", 2);
        whiteD = new Item("whiteD", "WhiteD", "WhiteD", "可使用一次，可使用一次，可得九宮格範圍之內是否有眼存在。", "green", 2);
        whiteE = new Item("whiteE", "WhiteE", "WhiteE", "可使用一次，使該回合行動點數增加三點，可疊加。", "green", 2);


    }
    [PunRPC]
    private void UpdatePlayers(int playerID, string playerNickName, int playerLocationI, int playerLocationJ)
    {
        playerInfo = new PlayerInfo(playerNickName, playerLocationI, playerLocationJ);

        if (playersInfo.ContainsKey(playerID))
        {
            playersInfo.Remove(playerID);
        }
        playersInfo.Add(playerID, playerInfo);

    }
    /*
    purpose:建造grass地形，隨機產生
    生成位置在Vector3(i, 0, j)，i與j的範圍是0~mapSize
    */
    private void CreateGrass()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (Random.Range(0, 100) < 80)
                {
                    grassGrid[i, j] = 1;
                    emptyGrid[i, j] = 0;
                }
                if (grassGrid[i, j] == 1)
                {
                    //生成prefab
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    /*
    purpose:建造water地形，先用FindEnclosedPosition函數找出被grass包圍的empty地形
    把找到的生成water地形
    */
    private void CreateWater()
    {
        int[,] NotEmptyGrid = new int[mapSize, mapSize];

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (emptyGrid[i, j] == 0)
                {
                    NotEmptyGrid[i, j] = 1;
                }
            }
        }
        FindEnclosedPosition(emptyGrid, NotEmptyGrid, ref waterGrid);
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (waterGrid[i, j] == 1)
                {
                    emptyGrid[i, j] = 0;
                    gridObj[i, j] = Instantiate(waterPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }

            }
        }
    }
    /*
    purpose:由於建造grass可能不連續，容易導致有些grass被孤立，走不到，因此需要填補起來
    方式:如果empty周圍3個都是grass，也將empty的生成grass
    如果empty周圍2個都是grass，也將empty的生成grass
    */
    private void FillGrass()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                int grassCount = 0;
                if (grassGrid[i, j] == 0 && !IsBoundary(i, j))
                {
                    if (grassGrid[i - 1, j] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i + 1, j] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i, j - 1] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i, j + 1] == 1)
                    {
                        grassCount++;
                    }
                }
                if (grassCount == 3 && emptyGrid[i, j] == 1)
                {
                    grassGrid[i, j] = 1;
                    emptyGrid[i, j] = 0;
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                int grassCount = 0;
                if (grassGrid[i, j] == 0 && !IsBoundary(i, j))
                {
                    if (grassGrid[i - 1, j] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i + 1, j] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i, j - 1] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i, j + 1] == 1)
                    {
                        grassCount++;
                    }
                }
                if (grassCount == 2 && emptyGrid[i, j] == 1)
                {
                    grassGrid[i, j] = 1;
                    emptyGrid[i, j] = 0;
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    /*
    purpose:由於建造的water可能太分散，因此把grass周圍3個為water的grass改成water
    讓湖泊變大
    */
    private void FillWater()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                int waterCount = 0;
                if (waterGrid[i, j] == 0 && !IsBoundary(i, j))
                {
                    if (waterGrid[i - 1, j] == 1)
                    {
                        waterCount++;
                    }
                    if (waterGrid[i + 1, j] == 1)
                    {
                        waterCount++;
                    }
                    if (waterGrid[i, j - 1] == 1)
                    {
                        waterCount++;
                    }
                    if (waterGrid[i, j + 1] == 1)
                    {
                        waterCount++;
                    }
                }
                if (waterCount == 3 && grassGrid[i, j] == 1)
                {
                    waterGrid[i, j] = 1;
                    grassGrid[i, j] = 0;
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(waterPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    /*
    purpose:找出被孤立，走不過去的grass，利用FindEnclosedPosition函數
    */
    private void FindIsolatedGrass()
    {
        int[,] grassEmptyGrid = new int[mapSize, mapSize];

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (grassEmptyGrid[i, j] == 0)
                {
                    grassEmptyGrid[i, j] = 1;
                }
            }
        }
        FindEnclosedPosition(grassGrid, grassEmptyGrid, ref isolatedGrid);
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                //if (isolatedGrid[i, j] == 1)
                //{
                //    gridObj[i, j] = Instantiate(starPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                //    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                //}
            }
        }
    }
    /*
    purpose:去除只有單一的water
    */
    private void RemoveOnlyOneWater()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                int grassCount = 0;
                if (waterGrid[i, j] == 1 && !IsBoundary(i, j))
                {
                    if (grassGrid[i - 1, j] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i + 1, j] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i, j - 1] == 1)
                    {
                        grassCount++;
                    }
                    if (grassGrid[i, j + 1] == 1)
                    {
                        grassCount++;
                    }
                }
                if (grassCount == 4)
                {
                    waterGrid[i, j] = 0;
                    grassGrid[i, j] = 1;
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    private void FillEmpty()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (emptyGrid[i, j] == 1)
                {
                    emptyGrid[i, j] = 0;
                    grassGrid[i, j] = 1;
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    /*puddle
    //purpose:建造傳送門
    */
    private void CreatePortal()
    {
        int i, j;

        portalNumAuto = Random.Range(2, 5);
        while (portalNumAuto % 2 != 0)
        {
            portalNumAuto = Random.Range(2, 5);
        }
        portalPairAuto = new int[portalNumAuto, 4];

        for (int x = 0; x < portalNumAuto; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                portalPairAuto[x, y] = -1;
            }
        }

        for (int x = 0; x < portalNumAuto; x++)
        {
            i = Random.Range(0, mapSize);
            j = Random.Range(0, mapSize);
            while (grassGrid[i, j] != 1)
            {
                i = Random.Range(0, mapSize);
                j = Random.Range(0, mapSize);
            }
            grassGrid[i, j] = 0;
            portalGrid[i, j] = 1;
            Destroy(gridObj[i, j]);


            if (x % 2 == 0)
            {
                portalPairAuto[x, 0] = i;
                portalPairAuto[x, 1] = j;
                portalPairAuto[x + 1, 2] = i;
                portalPairAuto[x + 1, 3] = j;
            }
            else
            {
                portalPairAuto[x, 0] = i;
                portalPairAuto[x, 1] = j;
                portalPairAuto[x - 1, 2] = i;
                portalPairAuto[x - 1, 3] = j;
            }
            gridObj[i, j] = Instantiate(portalPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Map").transform;

        }
    }
    /*
    //purpose:建造水坑
    */
    private void CreatePuddle()
    {
        int randomResult;
        int itemPossibility = mapSize / 3;

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                randomResult = Random.Range(0, 100);
                if (grassGrid[i, j] == 1 && (randomResult < itemPossibility))
                {
                    grassGrid[i, j] = 0;
                    puddleGrid[i, j] = 1;
                    Destroy(gridObj[i, j]);
                    int rotatePossibility = Random.Range(0, 4);
                    gridObj[i, j] = Instantiate(puddlePrefab, transform.position + new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, rotatePossibility * 90, 0)));
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    private void CreateTrap()
    {
        int randomResult;
        int trapPossibility = mapSize / 5;
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                randomResult = Random.Range(0, 100);
                if (grassGrid[i, j] == 1 && (randomResult < trapPossibility))
                {
                    grassGrid[i, j] = 0;
                    trapGrid[i, j] = 1;
                }
            }
        }
    }
    private void CreateTree()
    {
        int randomResult;
        int itemPossibility = mapSize / 3;

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                randomResult = Random.Range(0, 100);
                if (grassGrid[i, j] == 1 && (randomResult < itemPossibility))
                {
                    grassGrid[i, j] = 0;
                    treeGrid[i, j] = 1;
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(treePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    private void CreateReed()
    {
        int randomResult;
        int itemPossibility = mapSize / 3;
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                randomResult = Random.Range(0, 100);
                if (grassGrid[i, j] == 1 && (randomResult < itemPossibility))
                {
                    grassGrid[i, j] = 0;
                    reedGrid[i, j] = 1;
                    Destroy(gridObj[i, j]);
                    int rotatePossibility = Random.Range(0, 4);
                    gridObj[i, j] = Instantiate(reedPrefab, transform.position + new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, rotatePossibility * 90, 0)));
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                }
            }
        }
    }
    private void CreateBarren()
    {
        int randomResult;
        int itemPossibility = mapSize / 3;

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                randomResult = Random.Range(0, 100);
                if (grassGrid[i, j] == 1 && (randomResult < itemPossibility))
                {
                    grassGrid[i, j] = 0;
                    barrenGrid[i, j] = 1;
                    Destroy(gridObj[i, j]);
                    int rotatePossibility = Random.Range(0, 4);
                    gridObj[i, j] = Instantiate(barrenPrefab, transform.position + new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, rotatePossibility * 90, 0)));
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;

                    barrenRecoverRound.Add(i.ToString() + "," + j.ToString(), 5);
                }
            }
        }
    }
    private void CreateStore()
    {
        int i, j;

        storeNum = Random.Range(1, 5);
        storeLocation = new int[storeNum, 2];
        for (int x = 0; x < storeNum; x++)
        {
            bool storeNearby = false;
            ArrayList surroundings = new ArrayList();
            i = Random.Range(0, mapSize);
            j = Random.Range(0, mapSize);
            while (grassGrid[i, j] != 1)
            {
                i = Random.Range(0, mapSize);
                j = Random.Range(0, mapSize);
            }


            if (j != (mapSize - 1))
            {
                if (storeGrid[i, j + 1] == 1)
                {
                    storeNearby = true;
                }
                if (grassGrid[i, j + 1] == 1)
                {
                    surroundings.Add(2);
                }
            }
            if (j != 0)
            {
                if (storeGrid[i, j - 1] == 1)
                {
                    storeNearby = true;
                }
                if (grassGrid[i, j - 1] == 1)
                {
                    surroundings.Add(0);
                }
            }
            if (i != (mapSize - 1))
            {
                if (storeGrid[i + 1, j] == 1)
                {
                    storeNearby = true;
                }
                if (grassGrid[i + 1, j] == 1)
                {
                    surroundings.Add(3);
                }
            }
            if (i != 0)
            {
                if (storeGrid[i - 1, j] == 1)
                {
                    storeNearby = true;
                }
                if (grassGrid[i - 1, j] == 1)
                {
                    surroundings.Add(1);
                }
            }

            if (surroundings.Count == 0 || storeNearby)
            {
                x--;
            }
            else
            {
                grassGrid[i, j] = 0;
                storeGrid[i, j] = 1;
                storeLocation[x, 0] = i;
                storeLocation[x, 1] = j;
                Destroy(gridObj[i, j]);
                int rotateRandom = Random.Range(0, surroundings.Count);
                int rotate = (int)surroundings[rotateRandom];
                gridObj[i, j] = Instantiate(storePrefab, transform.position + new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, rotate * 90, 0)));
                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;


            }


        }
    }
    /*
     purpose:生成商店道具 
    */
    private void CreateStoreItem()
    {
        itemSoldOutGrid = new int[storeNum, storeItemNum];
        storeList = new ArrayList();
        for (int i = 0; i < storeNum; i++)
        {
            storeItemList = new List<Item>();
            for (int j = 0; j < storeItemNum; j++)
            {
                int randomResult = Random.Range(0, 100);
                if (randomResult < Item.GetPurpleStorePossibility())
                {
                    randomResult = Random.Range(0, purpleItemList.Count);
                    storeItemList.Add(purpleItemList[randomResult]);
                }
                else if (randomResult >= Item.GetPurpleStorePossibility() && randomResult < (Item.GetPurpleStorePossibility() + Item.GetBlueStorePossibility()))
                {
                    randomResult = Random.Range(0, blueItemList.Count);
                    storeItemList.Add(blueItemList[randomResult]);
                }
                else if (randomResult >= (Item.GetPurpleStorePossibility() + Item.GetBlueStorePossibility()) && randomResult < (Item.GetPurpleStorePossibility() + Item.GetBlueStorePossibility() + Item.GetGreenStorePossibility()))
                {
                    randomResult = Random.Range(0, greenItemList.Count);
                    storeItemList.Add(greenItemList[randomResult]);
                }
                else if (randomResult >= (Item.GetPurpleStorePossibility() + Item.GetBlueStorePossibility() + Item.GetGreenStorePossibility()))
                {
                    randomResult = Random.Range(0, whiteItemList.Count);
                    storeItemList.Add(whiteItemList[randomResult]);
                }
            }
            storeList.Add(storeItemList);
        }


    }
    /*
    //purpose:隨機生成道具
    //*/
    private void CreateItem()
    {
        int randomResult;
        int itemPossibility = 100;

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                randomResult = Random.Range(0, 100);
                if (randomResult < itemPossibility)
                {
                    randomResult = Random.Range(0, 100);
                    if ((grassGrid[i, j] == 1 || trapGrid[i, j] == 1) && randomResult <= Item.GetPurplePossibility())
                    {
                        randomResult = Random.Range(0, purpleItemList.Count);
                        gridItemObj[i, j] = Instantiate((GameObject)Resources.Load("Item/" + purpleItemList[randomResult].GetResourceName()), transform.position + new Vector3(i, 0, j), Quaternion.identity);
                        gridItemObj[i, j].transform.parent = GameObject.Find("Map").transform;
                        itemGrid[i, j] = purpleItemList[randomResult];
                    }
                    else if ((grassGrid[i, j] == 1 || trapGrid[i, j] == 1) && randomResult > Item.GetPurplePossibility() && randomResult < Item.GetPurplePossibility() + Item.GetBluePossibility())
                    {
                        randomResult = Random.Range(0, blueItemList.Count);
                        gridItemObj[i, j] = Instantiate((GameObject)Resources.Load("Item/" + blueItemList[randomResult].GetResourceName()), transform.position + new Vector3(i, 0, j), Quaternion.identity);
                        gridItemObj[i, j].transform.parent = GameObject.Find("Map").transform;
                        itemGrid[i, j] = blueItemList[randomResult];
                    }
                }

            }
        }
    }
    /*
     purpose:將產生的地圖存入terrian 2d-array以供存取
     */
    //barren = 0, grass = 1, puddle, reed = 3, trap = 4, tree = 5, portal = 6,  water = 7, store = 8, isolated grass = 100
    private void SaveToTerrain()
    {
        S_Terrain = new int[mapSize, mapSize];
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (barrenGrid[i, j] == 1)
                {
                    S_Terrain[i, j] = 0;
                }
                else if (grassGrid[i, j] == 1)
                {
                    S_Terrain[i, j] = 1;
                }
                else if (puddleGrid[i, j] == 1)
                {
                    S_Terrain[i, j] = 2;
                }
                else if (reedGrid[i, j] == 1)
                {
                    S_Terrain[i, j] = 3;
                }
                else if (trapGrid[i, j] == 1)
                {
                    if (triggeredGrid[i, j] == 1)
                        S_Terrain[i, j] = -4;
                    else
                        S_Terrain[i, j] = 4;
                }
                else if (treeGrid[i, j] == 1)
                {
                    S_Terrain[i, j] = 5;
                }
                else if (portalGrid[i, j] == 1)
                {
                    S_Terrain[i, j] = 6;
                }
                else if (waterGrid[i, j] == 1)
                {
                    S_Terrain[i, j] = 7;
                }
                else if (storeGrid[i, j] == 1)
                {
                    S_Terrain[i, j] = 8;
                }
            }
        }
        //for (int i = 0; i < mapSize; i++)
        //{
        //    for (int j = 0; j < mapSize; j++)
        //    {
        //        print(S_Terrain[i, j]);
        //    }
        //}
    }
    //初始化UI物件
    private void GetObject()
    {
        if (player == null)
        {
            playerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"];
            player = GameObject.Find("Player" + playerID.ToString()).GetComponent<Player>();
            locationPre = player.P_GetLocation();
        }
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            roundPre = gameManager.M_GetRound();
        }
        if (mapEventCanvas == null)
        {
            mapEventCanvas = GameObject.Find("MapEventCanvas").gameObject;
            if (cannotFetchItemPanel == null)
            {
                cannotFetchItemPanel = mapEventCanvas.transform.Find("CannotFetchItemPanel").gameObject;
            }

        }
        if (storeCanvas == null)
        {
            storeCanvas = GameObject.Find("StoreCanvas").gameObject;
            if (storePanel == null)
            {
                storePanel = storeCanvas.transform.Find("StorePanel").gameObject;
                for (int i = 0; i < storeItemNum; i++)
                {
                    storeItemObj[i] = storePanel.transform.Find("StoreItemGroup" + (i + 1).ToString()).gameObject;
                }

            }
        }
        if (bagCanvas == null)
        {
            bagCanvas = GameObject.Find("BagCanvas").gameObject;
            if (bagPanel == null)
            {
                bagPanel = bagCanvas.transform.Find("BagPanel").gameObject;
                for (int i = 0; i < bagItemNum; i++)
                {
                    bagItemObj[i] = bagPanel.transform.Find("BagItemGroup" + (i + 1).ToString()).gameObject;
                }
                bagItemObj[bagItemNum] = bagPanel.transform.Find("BagItemGroupExtra").gameObject;

            }
        }

        if (itemInfoCanvas == null)
        {
            itemInfoCanvas = GameObject.Find("ItemInfoCanvas").gameObject;
            itemInfoPanel = itemInfoCanvas.transform.Find("ItemInfoPanel").gameObject;
        }
        if (confirmCanvas == null)
        {
            confirmCanvas = GameObject.Find("ConfirmCanvas").gameObject;
            confirmText = confirmCanvas.transform.GetChild(0).GetChild(0).Find("ConfirmText").gameObject;
            confirmBackGroundPanel = confirmCanvas.transform.Find("ConfirmBackGroundPanel").gameObject;
        }
    }
    private void CreateOuterGrass()
    {
        GameObject temp;
        for (int i = 1; i < mapSize; i++)
        {
            for (int j = 1; j < mapSize; j++)
            {
                temp = Instantiate(outerGrassPrefab, transform.position + new Vector3(-i, 0, j), Quaternion.identity);
                temp.transform.parent = GameObject.Find("Map").transform;
                if (i > mapSize / 3)
                {
                    Destroy(temp);
                }
                temp = Instantiate(outerGrassPrefab, transform.position + new Vector3(i, 0, -j), Quaternion.identity);
                temp.transform.parent = GameObject.Find("Map").transform;
                if (j > mapSize / 3)
                {
                    Destroy(temp);
                }
            }
        }
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                temp = Instantiate(outerGrassPrefab, transform.position + new Vector3(-i, 0, -j), Quaternion.identity);
                temp.transform.parent = GameObject.Find("Map").transform;
                if (i == 0 && j == 0 || j > mapSize / 3 || i > mapSize / 3)
                {
                    Destroy(temp);
                }
                temp = Instantiate(outerGrassPrefab, transform.position + new Vector3(-i, 0, mapSize + j), Quaternion.identity);
                temp.transform.parent = GameObject.Find("Map").transform;
                if (i == 0 && j == 0 || j > mapSize / 3 || i > mapSize / 3)
                {
                    Destroy(temp);
                }
                temp = Instantiate(outerGrassPrefab, transform.position + new Vector3(mapSize + i, 0, -j), Quaternion.identity);
                temp.transform.parent = GameObject.Find("Map").transform;
                if (i == 1 && j == 0 || j > mapSize / 3 || i > mapSize / 3)
                {
                    Destroy(temp);
                }
                temp = Instantiate(outerGrassPrefab, transform.position + new Vector3(mapSize + i, 0, j), Quaternion.identity);
                temp.transform.parent = GameObject.Find("Map").transform;
                if (i == 0 && j == 0 || i > mapSize / 3)
                {
                    Destroy(temp);
                }
                temp = Instantiate(outerGrassPrefab, transform.position + new Vector3(i, 0, mapSize + j), Quaternion.identity);
                temp.transform.parent = GameObject.Find("Map").transform;
                if (i == 0 && j == 1 || j > mapSize / 3)
                {
                    Destroy(temp);
                }
                temp = Instantiate(outerGrassPrefab, transform.position + new Vector3(mapSize + i, 0, mapSize + j), Quaternion.identity);
                temp.transform.parent = GameObject.Find("Map").transform;
                if (j > mapSize / 3 || i > mapSize / 3)
                {
                    Destroy(temp);
                }

            }
        }
    }
    /*
    purpose:找出被包圍的格子，利用深度優先搜尋
    */
    private void FindEnclosedPosition(int[,] beEnclosedGrid, int[,] encloseGrid, ref int[,] selectedGrid)
    {
        int beEnclosedPosition = 0, enclosePosition = 1;
        int col = mapSize, row = mapSize;
        int[,] tempGrid = new int[mapSize, mapSize];


        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (beEnclosedGrid[i, j] == 1)
                {
                    tempGrid[i, j] = beEnclosedPosition;
                }
                else if (encloseGrid[i, j] == 1)
                {
                    tempGrid[i, j] = enclosePosition;
                }
            }
        }
        // top一行
        for (int i = 0; i < col; i++)
        {
            if (tempGrid[0, i] == beEnclosedPosition)
            {
                tempGrid[0, i] = -1;
                DFS(0, i, beEnclosedPosition, enclosePosition, ref tempGrid);
            }
        }
        // bottom一行
        for (int i = 0; i < col; i++)
        {
            if (tempGrid[row - 1, i] == beEnclosedPosition)
            {
                tempGrid[row - 1, i] = -1;
                DFS(row - 1, i, beEnclosedPosition, enclosePosition, ref tempGrid);
            }
        }
        //left一列
        for (int i = 1; i < row - 1; i++)
        {
            if (tempGrid[i, 0] == beEnclosedPosition)
            {
                tempGrid[i, 0] = -1;
                DFS(i, 0, beEnclosedPosition, enclosePosition, ref tempGrid);
            }
        }
        // right一列
        for (int i = 1; i < row - 1; i++)
        {
            if (tempGrid[i, col - 1] == beEnclosedPosition)
            {
                tempGrid[i, col - 1] = -1;
                DFS(i, col - 1, beEnclosedPosition, enclosePosition, ref tempGrid);
            }
        }
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (tempGrid[i, j] == beEnclosedPosition)
                {
                    selectedGrid[i, j] = 1;
                }
            }
        }

    }
    /*
    purpose:深度優先搜尋
    */
    private void DFS(int i, int j, int beEnclosedPosition, int enclosePosition, ref int[,] tempGrid)
    {
        int col = mapSize, row = mapSize;


        if (i > 1 && tempGrid[i - 1, j] == beEnclosedPosition)
        {
            tempGrid[i - 1, j] = -1;
            DFS(i - 1, j, beEnclosedPosition, enclosePosition, ref tempGrid);
        }
        if (i < row - 1 && tempGrid[i + 1, j] == beEnclosedPosition)
        {
            tempGrid[i + 1, j] = -1;
            DFS(i + 1, j, beEnclosedPosition, enclosePosition, ref tempGrid);
        }
        if (j > 1 && tempGrid[i, j - 1] == beEnclosedPosition)
        {
            tempGrid[i, j - 1] = -1;
            DFS(i, j - 1, beEnclosedPosition, enclosePosition, ref tempGrid);
        }
        if (j < col - 1 && tempGrid[i, j + 1] == beEnclosedPosition)
        {
            tempGrid[i, j + 1] = -1;
            DFS(i, j + 1, beEnclosedPosition, enclosePosition, ref tempGrid);

        }
    }
    /*
    purpose:將2d-array填滿給定的數字
    */
    private void FillArray(ref int[,] emptyGrid, int number, int col, int row)
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                emptyGrid[i, j] = number;
            }
        }
    }
    /*
    purpose:判斷是否為地圖邊界
    */
    private bool IsBoundary(int i, int j)
    {
        if (i == (mapSize - 1) || i == 0 || j == (mapSize - 1) || j == 0)
        {
            return true;
        }
        else
            return false;
    }
    /*
     判斷是否是九宮格
    */
    public bool Surrounding(int x, int z, int i, int j)
    {
        if ((x == i && z == j) || (x == i + 1 && z == j) || (x == i - 1 && z == j) || (x == i && z == j + 1) || (x == i && z == j - 1) || (x == i + 1 && z == j + 1) || (x == i + 1 && z == j - 1) || (x == i - 1 && z == j + 1) || (x == i - 1 && z == j - 1))
        {
            return true;
        }
        else return false;
    }
    /*
     關閉並重設採集panel
    */
    private void ResetFetchItemPanel()
    {
        cannotFetchItemPanel.SetActive(false);
        cannotFetchItemPanel.GetComponent<RectTransform>().position = new Vector2(0, 0);
    }
    [PunRPC]
    public void BeSearched(int[] beSearchedList)
    {
        if (beSearchedList[playerID] == 1)
        {
            gameManager.M_ShowInfo("被敵人搜索出來", 2, true);
            photonView.RPC("ShowUpUpdateView", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
        }
    }
    [PunRPC]
    public void HideUpdateView(int playerViewID)
    {
        for (int i = 0; i < 5; i++)
            PhotonView.Find(playerViewID).gameObject.transform.GetChild(i).gameObject.SetActive(false);
    }
    [PunRPC]
    public void ShowUpUpdateView(int playerViewID)
    {
        for (int i = 0; i < 5; i++)
            PhotonView.Find(playerViewID).gameObject.transform.GetChild(i).gameObject.SetActive(true);
    }

    public class PlayerInfo
    {

        public string nickName { get; set; }
        public int locationI { get; set; }
        public int locationJ { get; set; }

        public PlayerInfo(string nickName, int locationI, int locationJ)
        {
            this.nickName = nickName;
            this.locationI = locationI;
            this.locationJ = locationJ;
        }
    }
    public void SetConfirmResult(int confirmResult)
    {
        confirmBackGroundPanel.SetActive(false);
        this.confirmResult = confirmResult;
    }

    private void MoveOnMapControl()
    {
        if (Input.GetKeyDown(KeyCode.W))//往前走
        {
            if (!(onPuddle || onTrap))
                player.P_SetMoveLock(false);

            if (player.P_GetActionPoint() > 0)
                player.GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, 0);
            if (treeGrid[(int)player.P_GetLocation().x, (int)player.P_GetLocation().z + 1] == 1)
            {
                player.P_SetMoveLock(true);
            }
            if (reedGrid[(int)player.P_GetLocation().x, (int)player.P_GetLocation().z + 1] == 1 && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
            {
                int playerLocationI = (int)player.P_GetLocation().x;
                int playerLocationJ = (int)player.P_GetLocation().z;
                player.P_SetMoveLock(true);
                confirmResult = -1;
                confirmText.GetComponent<Text>().text = "是否消耗4點行動點數進行藏匿?";
                confirmBackGroundPanel.SetActive(true);
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
                StartCoroutine(WaiterForPlayerClickBtn());
                IEnumerator WaiterForPlayerClickBtn()
                {
                    while (confirmResult == -1)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    if (confirmResult == 1)
                    {
                        Debug.LogError(player.P_GetActionPoint());
                        if (player.P_GetActionPoint() <= 4)
                        {
                            gameManager.M_ShowInfo("行動點數不足", 1);
                        }
                        else
                        {
                            inReed = true;
                            player.P_SetActionPoint(player.P_GetActionPoint() - 4);
                            player.P_SetLocation(new Vector3(playerLocationI, 0, playerLocationJ + 1));
                            photonView.RPC("HideUpdateView", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
                            locationPre = player.P_GetLocation();
                        }
                    }
                }
            }
            if (waterGrid[(int)player.P_GetLocation().x, (int)player.P_GetLocation().z + 1] == 1 && !player.P_GetItemList().Contains(blueD))
            {
                player.P_SetMoveLock(true);
            }

        }
        if (Input.GetKeyDown(KeyCode.A))//往左走
        {
            if (!(onPuddle || onTrap))
                player.P_SetMoveLock(false);

            if (player.P_GetActionPoint() > 0)
                player.GetComponent<Transform>().rotation = Quaternion.Euler(0, 270, 0);

            if (treeGrid[(int)player.P_GetLocation().x - 1, (int)player.P_GetLocation().z] == 1)
            {
                player.P_SetMoveLock(true);
            }
            if (reedGrid[(int)player.P_GetLocation().x - 1, (int)player.P_GetLocation().z] == 1 && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
            {
                int playerLocationI = (int)player.P_GetLocation().x;
                int playerLocationJ = (int)player.P_GetLocation().z;
                player.P_SetMoveLock(true);
                confirmResult = -1;
                confirmText.GetComponent<Text>().text = "是否消耗4點行動點數進行藏匿?";
                confirmBackGroundPanel.SetActive(true);
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
                StartCoroutine(WaiterForPlayerClickBtn());
                IEnumerator WaiterForPlayerClickBtn()
                {
                    while (confirmResult == -1)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    if (confirmResult == 1)
                    {
                        Debug.LogError(player.P_GetActionPoint());
                        if (player.P_GetActionPoint() <= 4)
                        {
                            gameManager.M_ShowInfo("行動點數不足", 1);
                        }
                        else
                        {
                            inReed = true;
                            player.P_SetActionPoint(player.P_GetActionPoint() - 4);
                            player.P_SetLocation(new Vector3(playerLocationI - 1, 0, playerLocationJ));
                            photonView.RPC("HideUpdateView", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
                            locationPre = player.P_GetLocation();
                        }
                    }
                }
            }
            if (waterGrid[(int)player.P_GetLocation().x - 1, (int)player.P_GetLocation().z] == 1 && !player.P_GetItemList().Contains(blueD)) {
                player.P_SetMoveLock(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.S))//往後走
        {
            if (!(onPuddle || onTrap))
                player.P_SetMoveLock(false);

            if (player.P_GetActionPoint() > 0)
                player.GetComponent<Transform>().rotation = Quaternion.Euler(0, 180, 0);

            if (treeGrid[(int)player.P_GetLocation().x, (int)player.P_GetLocation().z - 1] == 1)
            {
                player.P_SetMoveLock(true);
            }
            if (reedGrid[(int)player.P_GetLocation().x, (int)player.P_GetLocation().z - 1] == 1 && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
            {
                int playerLocationI = (int)player.P_GetLocation().x;
                int playerLocationJ = (int)player.P_GetLocation().z;
                player.P_SetMoveLock(true);
                confirmResult = -1;
                confirmText.GetComponent<Text>().text = "是否消耗4點行動點數進行藏匿?";
                confirmBackGroundPanel.SetActive(true);
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
                StartCoroutine(WaiterForPlayerClickBtn());
                IEnumerator WaiterForPlayerClickBtn()
                {
                    while (confirmResult == -1)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    if (confirmResult == 1)
                    {
                        Debug.LogError(player.P_GetActionPoint());
                        if (player.P_GetActionPoint() <= 4)
                        {
                            gameManager.M_ShowInfo("行動點數不足", 1);
                        }
                        else
                        {
                            inReed = true;
                            player.P_SetActionPoint(player.P_GetActionPoint() - 4);
                            player.P_SetLocation(new Vector3(playerLocationI, 0, playerLocationJ - 1));
                            photonView.RPC("HideUpdateView", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
                            locationPre = player.P_GetLocation();
                        }
                    }
                }
            }
            if (waterGrid[(int)player.P_GetLocation().x, (int)player.P_GetLocation().z - 1] == 1 && !player.P_GetItemList().Contains(blueD))
            {
                player.P_SetMoveLock(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.D))//往右走
        {
            if (!(onPuddle || onTrap))
                player.P_SetMoveLock(false);

            if (player.P_GetActionPoint() > 0)
                player.GetComponent<Transform>().rotation = Quaternion.Euler(0, 90, 0);

            if (treeGrid[(int)player.P_GetLocation().x + 1, (int)player.P_GetLocation().z] == 1)
            {
                player.P_SetMoveLock(true);
            }
            if (reedGrid[(int)player.P_GetLocation().x + 1, (int)player.P_GetLocation().z] == 1 && player.P_GetActionPoint() != 0 && !player.P_GetMoveLock())
            {
                int playerLocationI = (int)player.P_GetLocation().x;
                int playerLocationJ = (int)player.P_GetLocation().z;
                player.P_SetMoveLock(true);
                confirmResult = -1;
                confirmText.GetComponent<Text>().text = "是否消耗4點行動點數進行藏匿?";
                confirmBackGroundPanel.SetActive(true);
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
                StartCoroutine(WaiterForPlayerClickBtn());
                IEnumerator WaiterForPlayerClickBtn()
                {
                    while (confirmResult == -1)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    if (confirmResult == 1)
                    {
                        Debug.LogError(player.P_GetActionPoint());
                        if (player.P_GetActionPoint() <= 4)
                        {
                            gameManager.M_ShowInfo("行動點數不足", 1);
                        }
                        else
                        {
                            inReed = true;
                            player.P_SetActionPoint(player.P_GetActionPoint() - 4);
                            player.P_SetLocation(new Vector3(playerLocationI + 1, 0, playerLocationJ));
                            photonView.RPC("HideUpdateView", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
                            locationPre = player.P_GetLocation();
                        }
                    }
                }
            }
            if (waterGrid[(int)player.P_GetLocation().x + 1, (int)player.P_GetLocation().z] == 1 && !player.P_GetItemList().Contains(blueD))
            {
                player.P_SetMoveLock(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            S_ChopTheTree();

        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            S_CreatePortal();

        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            S_RandomTransfer();

        }

    }
    private (int, int) GetFrontLocation(int playerLocationI, int playerLocationJ, int rotationY)
    {
        if (rotationY == 0) return (playerLocationI, playerLocationJ +1 );
        else if (rotationY == 90) return (playerLocationI + 1, playerLocationJ);
        else if (rotationY == 180) return (playerLocationI, playerLocationJ - 1);
        else return (playerLocationI - 1, playerLocationJ);
    }
    public void UpdateBagItem()
    {
        for (int i = 0; i < bagItemNum; i++)
        {
            bagItemObj[i].transform.Find("ItemImage".ToString()).gameObject.SetActive(false);
            bagItemObj[i].transform.Find("ItemImageBackground".ToString()).gameObject.SetActive(false);
            bagItemObj[i].transform.Find("BeDragCover".ToString()).gameObject.SetActive(false);
            bagItemObj[i].transform.Find("NotOpenImage".ToString()).gameObject.SetActive(false);
            if (i < player.P_GetItemList().Count)
            {
                bagItemObj[i].transform.Find("ItemImage".ToString()).gameObject.SetActive(true);
                bagItemObj[i].transform.Find("ItemImageBackground".ToString()).gameObject.SetActive(true);
                bagItemObj[i].transform.Find("BeDragCover".ToString()).gameObject.SetActive(true);
                Texture temp = (Texture2D)Resources.Load("Item/Icon/" + player.P_GetItemList()[i].GetResourceName());
                bagItemObj[i].transform.Find("ItemImage".ToString()).GetComponent<RawImage>().texture = temp;
                bagItemObj[i].transform.Find("ItemImageBackground".ToString()).GetComponent<RawImage>().texture = temp;

            }
            else if (i >= player.P_GetItemAmountMax() && i < bagItemNum)
            {
                bagItemObj[i].transform.Find("NotOpenImage".ToString()).gameObject.SetActive(true);
            }
        }
    }
    private void ChooseItemToDiscard()
    {
        gameManager.M_ShowInfo("背包空間不足", 0.01f);
        isChoosingItemToDiscard = true;
        bagPanel.SetActive(false);
        bagPanel.GetComponent<RectTransform>().anchoredPosition = chooseItemToDiscardRect;
        for (int i = 0; i < bagItemNum; i++)
        {
            bagItemObj[i].transform.Find("ItemImage".ToString()).gameObject.SetActive(false);
            bagItemObj[i].transform.Find("ItemImageBackground".ToString()).gameObject.SetActive(false);
            bagItemObj[i].transform.Find("BeDragCover".ToString()).gameObject.SetActive(false);
            bagItemObj[i].transform.Find("NotOpenImage".ToString()).gameObject.SetActive(false);
            if (i < player.P_GetItemList().Count - 1)
            {
                bagItemObj[i].transform.Find("ItemImage".ToString()).gameObject.SetActive(true);
                bagItemObj[i].transform.Find("ItemImageBackground".ToString()).gameObject.SetActive(true);
                bagItemObj[i].transform.Find("BeDragCover".ToString()).gameObject.SetActive(true);
                Texture temp = (Texture2D)Resources.Load("Item/Icon/" + player.P_GetItemList()[i].GetResourceName());
                bagItemObj[i].transform.Find("ItemImage".ToString()).GetComponent<RawImage>().texture = temp;
                bagItemObj[i].transform.Find("ItemImageBackground".ToString()).GetComponent<RawImage>().texture = temp;

            }
            else if (i >= player.P_GetItemAmountMax() && i < bagItemNum)
            {
                bagItemObj[i].transform.Find("NotOpenImage".ToString()).gameObject.SetActive(true);
            }
            bagPanel.transform.Find("CloseBtn").gameObject.SetActive(false);
            bagPanel.transform.Find("BagText").gameObject.SetActive(false);
            bagPanel.transform.Find("DiscardText").gameObject.SetActive(true);
            bagItemObj[bagItemNum].SetActive(true);
            Texture extraTemp = (Texture2D)Resources.Load("Item/Icon/" + player.P_GetItemList()[player.P_GetItemList().Count - 1].GetResourceName());
            bagItemObj[bagItemNum].transform.Find("ItemImage".ToString()).GetComponent<RawImage>().texture = extraTemp;

        }
        bagPanel.SetActive(true);
    }
    public void DiscardItem(GameObject itemImage)
    {
        string resourceName = itemImage.GetComponent<RawImage>().texture.name.ToString();
        string itemName = null;
        foreach (Item i in itemList){
            if(i.GetResourceName() == resourceName)
            {
                itemName = i.GetName();
            }
        }
        if (isArbitrarilyDiscard)
        {
            if (itemImage.transform.parent.name.Substring(12) == "Extra")
            {
                List<Item> temp = player.P_GetItemList();
                temp.RemoveAt(player.P_GetItemList().Count - 1);
                player.P_SetItemList(temp);
            }
            else
            {
                int removeNum = int.Parse(itemImage.transform.parent.name.Substring(12));
                List<Item> temp = player.P_GetItemList();
                temp.RemoveAt(removeNum - 1);
                player.P_SetItemList(temp);
            }
            CloseItemInfo();
            S_ResetCursor();
            BagItem.Click = false;
            BagBeClick.Click = false;
        }
        else
        {
            confirmResult = -1;
            confirmText.GetComponent<Text>().text = "是否丟棄道具 : " + itemName + " ?";
            confirmBackGroundPanel.SetActive(true);
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            StartCoroutine(WaiterForPlayerClickBtn());
            IEnumerator WaiterForPlayerClickBtn()
            {
                while (confirmResult == -1)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                if (confirmResult == 1)
                {
                    if (itemImage.transform.parent.name.Substring(12) == "Extra")
                    {
                        List<Item> temp = player.P_GetItemList();
                        temp.RemoveAt(player.P_GetItemList().Count - 1);
                        player.P_SetItemList(temp);
                    }
                    else
                    {
                        int removeNum = int.Parse(itemImage.transform.parent.name.Substring(12));
                        List<Item> temp = player.P_GetItemList();
                        temp.RemoveAt(removeNum - 1);
                        player.P_SetItemList(temp);
                    }
                    isChoosingItemToDiscard = false;
                    bagPanel.SetActive(false);
                    bagPanel.transform.Find("CloseBtn").gameObject.SetActive(true);
                    bagPanel.transform.Find("BagText").gameObject.SetActive(true);
                    bagPanel.transform.Find("DiscardText").gameObject.SetActive(false);
                    bagItemObj[bagItemNum].SetActive(false);
                    bagPanel.GetComponent<RectTransform>().anchoredPosition = initBagRect;
                    bagPanel.SetActive(true);
                    CloseItemInfo();
                    S_ResetCursor();
                    BagItem.Click = false;
                    BagBeClick.Click = false;
                }
            }
        }      
    }
    public void S_ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
    public void ArbitrarilyDisCard()
    {
        isArbitrarilyDiscard = true;
        bagPanel.transform.Find("DiscardBtn").gameObject.SetActive(false);
        bagPanel.transform.Find("CancelDiscardBtn").gameObject.SetActive(true);
    }
    public void PauseArbitrarilyDisCard()
    {
        isArbitrarilyDiscard = false;
        bagPanel.transform.Find("DiscardBtn").gameObject.SetActive(true);
        bagPanel.transform.Find("CancelDiscardBtn").gameObject.SetActive(false);
    }

}