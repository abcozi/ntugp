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
    private int[,] grassGrid, waterGrid, emptyGrid, isolatedGrid, portalGrid, puddleGrid, trapGrid, treeGrid, reedGrid, barrenGrid, storeGrid;
    private string[,] itemGrid;
    private List<GameObject> purpleItemList, blueItemList;
    private int[,] S_Terrain;
    private int[,] portalPairAuto, portalPairFromPlayer;
    private GameObject[,] gridObj, gridItemObj;
    private int mapSize = 15, portalNumAuto, allPortalNumFromPlayer = 0;
    private int playerID = 0;
    private Player player;
    private GameManager gameManager;
    private GameObject mapEventCanvas, canFetchItemPanel, cannotFetchItemPanel;
    private RectTransform canFetchItemPanelRt, cannotFetchItemPanelRt;
    private int fetchItemI, fetchItemJ; 
    private Dictionary<string, int> barrenRecoverRound, portalRecoverRound;
    private int roundPre;
    private Vector3 locationPre;
    public GameObject grassPrefab, trapPrefab, waterPrefab, portalPrefab, puddlePrefab, treePrefab, reedPrefab, barrenPrefab, outerGrassPrefab, storePrefab;
    public GameObject purpleAPrefab, purpleBPrefab, purpleCPrefab, purpleDPrefab, blueAPrefab, blueBPrefab, blueCPrefab;
    private PhotonView photonView;
    private bool myRound;
    [SerializeField]
    private Transform cam;

    //barren = 0, grass = 1, puddle = 2, reed = 3, trap = 4, tree = 5, portal = 6,  water = 7, store = 8, isolated grass = 100


    //initialization
    void Start()
    {
        Random.InitState(Global.seed);
        //initialize each variable
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
        FillArray(ref emptyGrid, 1, mapSize, mapSize);
        itemGrid = new string[mapSize, mapSize];

        purpleItemList = new List<GameObject> { purpleAPrefab, purpleBPrefab, purpleCPrefab, purpleDPrefab};
        blueItemList = new List<GameObject> { blueAPrefab, blueBPrefab, blueCPrefab };

        gridObj = new GameObject[mapSize, mapSize];
        gridItemObj = new GameObject[mapSize, mapSize];

        barrenRecoverRound = new Dictionary<string, int>();
        portalRecoverRound = new Dictionary<string, int>();

        //生成各種地形與道具
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

            bool fetchItemPannelclose = CanFetchCloseBtn.GetIfClicked() || CannotFetchCloseBtn.GetIfClicked();
            bool fetchItemBtnBeClicked = FetchItemBtn.GetIfClicked();
            bool pannelBeClicked = CanFetchItemPanel.GetIfClicked() || CannotFetchItemPanel.GetIfClicked();
            
            //關閉採集畫面
            if (fetchItemPannelclose)
            {
                ResetFetchItemPanel();
                CanFetchCloseBtn.SetClicked(false);
                CannotFetchCloseBtn.SetClicked(false);
            }
            else if (fetchItemBtnBeClicked)//點擊採集按鈕
            {
                if(grassGrid[fetchItemI, fetchItemJ] == 1)
                {
                    S_FetchItem(fetchItemI, fetchItemJ);
                }
                if(trapGrid[fetchItemI, fetchItemJ] == 1)
                {
                    S_StepOnTheTrap(fetchItemI, fetchItemJ);
                }
                FetchItemBtn.SetClicked(false);
                ResetFetchItemPanel();
            }
            else if (pannelBeClicked)//點擊土地資訊pannel
            {
                ResetFetchItemPanel();
            }
           else if (Physics.Raycast(ray, out hit, 100))//點擊panel以外
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
                        if (hit.collider.gameObject.tag == "ItemOnMap")
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
                            ResetFetchItemPanel();
                            
                            canFetchItemPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(mousePos.x, mousePos.y);
                            canFetchItemPanel.SetActive(true);
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
            roundPre = gameManager.M_GetRound();
        }

        if (locationPre != player.P_GetLocation())
        {
            ResetFetchItemPanel();
            locationPre = player.P_GetLocation();
        }

        int playerLocationI = (int)player.P_GetLocation().x, playerLocationJ = (int)player.P_GetLocation().z;
        if (trapGrid[playerLocationI, playerLocationJ] == 1)
        {
            S_StepOnTheTrap(playerLocationI, playerLocationJ);
        }

       

        if(portalGrid[playerLocationI, playerLocationJ] == 1 && !portalRecoverRound.ContainsKey(playerLocationI.ToString() + "," + playerLocationJ.ToString()))
        {
            int transferI = S_GetTransferLocation(playerLocationI, playerLocationJ).Item1, transferJ = S_GetTransferLocation(playerLocationI, playerLocationJ).Item2;
            if (transferI != -1 && transferJ != -1)
            {

                portalRecoverRound.Add(playerLocationI.ToString() + "," + playerLocationJ.ToString(), 5);
                portalRecoverRound.Add(transferI.ToString() + "," + transferJ.ToString(), 5);
                player.P_SetLocation(new Vector3(transferI, 0, transferJ));
            }
            
        }
    }


    /*
     purpose:取得i row j col的地形
     */
    public int S_GetTerrain(int i, int j)
    {
        Debug.Log(S_Terrain[i, j]);
        return (S_Terrain[i, j]);
    }
    /*
     purpose:踩到陷阱, i row j col 陷阱被觸發
    */ 
    public void S_StepOnTheTrap(int i, int j)
    {
        if(trapGrid[i, j] == 1)
        {
            photonView.RPC("UpdateStepOnTheTrapMap", RpcTarget.All, i, j);
        }
    }
    [PunRPC]
    private void UpdateStepOnTheTrapMap(int i, int j)
    {
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(trapPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
    }
    /*
     purpose:採集, i row j col 生成採集後的土壤
    */

    public void S_FetchItem(int i, int j)
    {
        StartCoroutine(Fetch());
        IEnumerator Fetch()
        {
            yield return new WaitForSeconds(5);
            photonView.RPC("UpdateFetchBarrenMap", RpcTarget.All, i, j);
        }
        
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
    }

    public void S_GetItem(int i, int j)
    {
        photonView.RPC("UpdateItemOnMap", RpcTarget.All, i, j);
    }
    [PunRPC]
    private void UpdateItemOnMap(int i, int j)
    {
        itemGrid[i, j] = null;
        Destroy(gridItemObj[i, j]);
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
    public void S_ChopTheTree(int i, int j)
    {
        treeGrid[i, j] = 0;
        grassGrid[i, j] = 1;
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
    }
    /*
     purpose:建造傳送門
     */
    public void S_CreatePortal(int i, int j, int playerId)
    {
        grassGrid[i, j] = 0;
        portalGrid[i, j] = 1;
        Destroy(gridObj[i, j]);
        gridObj[i, j] = Instantiate(portalPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
        gridObj[i, j].transform.parent = GameObject.Find("Map").transform;


        bool newPair = true, single = true;
        for(int x = 0; x < allPortalNumFromPlayer; x++)
        {
            if(portalPairFromPlayer[x, 0] == playerId && portalPairFromPlayer[x, 1] != -1 && portalPairFromPlayer[x, 2] != -1 && portalPairFromPlayer[x, 3] == -1 && portalPairFromPlayer[x, 4] == -1)
            {
                portalPairFromPlayer[x, 3] = i;
                portalPairFromPlayer[x, 4] = j;
                for (int y = 0; y < allPortalNumFromPlayer; y++)
                {
                    if(portalPairFromPlayer[y, 3] == portalPairFromPlayer[x, 1] && portalPairFromPlayer[y, 4] == portalPairFromPlayer[x, 2])
                    {
                        portalPairFromPlayer[y, 1] = i;
                        portalPairFromPlayer[y, 2] = j;
                        newPair = false;
                    }
                }
                if (newPair)
                {
                    portalPairFromPlayer[allPortalNumFromPlayer, 0] = playerId;
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
            portalPairFromPlayer[allPortalNumFromPlayer, 0] = playerId;
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
        for(int x = 0; x < portalNumAuto; x++)
        {
            if(portalPairAuto[x, 0] == i && portalPairAuto[x, 1] == j){
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
        while(portalNumAuto % 2 != 0)
        {
            portalNumAuto = Random.Range(2, 5);
        }
        portalPairAuto = new int[portalNumAuto, 4];

        for(int x = 0; x < portalNumAuto; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                portalPairAuto[x, y] = -1;
            }
        }

        for(int x = 0; x < portalNumAuto; x++)
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
    //purpose:建造泥巴
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
        int itemPossibility = mapSize / 3;

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                randomResult = Random.Range(0, 100);
                if (grassGrid[i, j] == 1 && (randomResult < itemPossibility))
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

                    barrenRecoverRound.Add(i.ToString() +"," + j.ToString(), 5);
                }
            }
        }
    }
    private void CreateStore()
    {
        int i, j, storeNum;

        storeNum = Random.Range(1, 5);

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
                Destroy(gridObj[i, j]);
                int rotateRandom = Random.Range(0, surroundings.Count);
                int rotate = (int)surroundings[rotateRandom];
                gridObj[i, j] = Instantiate(storePrefab, transform.position + new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, rotate * 90, 0)));
                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;

            }


        }
    }
    /*
    //purpose:隨機生成道具
    //*/
    private void CreateItem()
    {
        int randomResult;
        int itemPossibility = mapSize / 3;
        int purplePossibility = 10, bluePossibility = 20;

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                randomResult = Random.Range(0, 100);
                if((grassGrid[i, j] == 1 || trapGrid[i, j] == 1) && randomResult <= purplePossibility)
                {
                    randomResult = Random.Range(0, purpleItemList.Count);
                    gridItemObj[i, j] = Instantiate(purpleItemList[randomResult], transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridItemObj[i, j].transform.parent = GameObject.Find("Map").transform;
                    itemGrid[i, j] = purpleItemList[randomResult].name;
                }
                else if ((grassGrid[i, j] == 1 || trapGrid[i, j] == 1) && randomResult > purplePossibility && randomResult < purplePossibility + bluePossibility)
                {
                    randomResult = Random.Range(0, blueItemList.Count);
                    gridItemObj[i, j] = Instantiate(blueItemList[randomResult], transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridItemObj[i, j].transform.parent = GameObject.Find("Map").transform;
                    itemGrid[i, j] = blueItemList[randomResult].name;
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
    private void GetObject()
    {
        if (photonView == null)
        {
            photonView = GetComponent<PhotonView>();
        }
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
            if (canFetchItemPanel == null)
            {
                canFetchItemPanel = mapEventCanvas.transform.Find("CanFetchItemPanel").gameObject;
                canFetchItemPanelRt = canFetchItemPanel.GetComponent<RectTransform>();
            }
            if (cannotFetchItemPanel == null)
            {
                cannotFetchItemPanel = mapEventCanvas.transform.Find("CannotFetchItemPanel").gameObject;
                cannotFetchItemPanelRt = cannotFetchItemPanel.GetComponent<RectTransform>();
            }
        }

    }
    private void CreateOuterGrass()
    {
        GameObject temp;
        for(int i = 1; i < mapSize; i++)
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
    private bool Surrounding(int x, int z, int i, int j)
    {
        if ((x == i && z == j) || (x == i + 1 && z == j) || (x == i - 1 && z == j) || (x == i && z == j + 1) || (x == i && z == j - 1) || (x == i + 1 && z == j + 1) || (x == i + 1 && z == j - 1) || (x == i - 1 && z == j +1 ) || (x == i - 1 && z == j - 1))
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
        canFetchItemPanel.SetActive(false);
        cannotFetchItemPanel.SetActive(false);
        canFetchItemPanelRt.position = new Vector2(0, 0);
        cannotFetchItemPanelRt.position = new Vector2(0, 0);
    }
    




}