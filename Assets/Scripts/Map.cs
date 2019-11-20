using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Map : MonoBehaviour
{
    //每個grid:2維array存各個地形或道具的分布
    //gridObj:初始化(生成)prefab的2維array
    //mapSize:地圖邊長
    private int[,] grassGrid, waterGrid, itemGrid, emptyGrid, isolatedGrid, bridgeGrid, portalGrid, puddleGrid, trapGrid, treeGrid, reedGrid, barrenGrid;
    private int[,] S_Terrain;
    private int[,] portalPair;
    private GameObject[,] gridObj;
    private int mapSize = 15, portalNum;
    public GameObject grassPrefab, trapPrefab, waterPrefab, starPrefab, bridgePrefab, portalPrefab, puddlePrefab, treePrefab, reedPrefab, barrenPrefab, outerGrassPrefab;
    //barren = 0, grass = 1, puddle = 2, reed = 3, trap = 4, tree = 5, portal = 6,  water = 7, isolated grass = 100
    //initialization
    void Start()
    {
        //initialize each variable
        grassGrid = new int[mapSize, mapSize];
        waterGrid = new int[mapSize, mapSize];
        itemGrid = new int[mapSize, mapSize];
        emptyGrid = new int[mapSize, mapSize];
        isolatedGrid = new int[mapSize, mapSize];
        bridgeGrid = new int[mapSize, mapSize];
        portalGrid = new int[mapSize, mapSize];
        puddleGrid = new int[mapSize, mapSize];
        trapGrid = new int[mapSize, mapSize];
        treeGrid = new int[mapSize, mapSize];
        reedGrid = new int[mapSize, mapSize];
        barrenGrid = new int[mapSize, mapSize];
        FillArray(ref emptyGrid, 1, mapSize, mapSize);

        gridObj = new GameObject[mapSize, mapSize];

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
        //CreateBridge();
        //CreateItem();
        SaveToTerrain();
        CreateOuterGrass();

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
     purpose:取得i row j col傳送門所傳送的地點
     */
    public (int, int) S_GetTransferLocation(int i, int j)
    {
        for(int x = 0; x < portalNum; x++)
        {
            if(portalPair[x, 0] == i && portalPair[x, 1] == j){
                return (portalPair[x, 2], portalPair[x, 3]);
            }
        }
        return (0, 0);
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

        portalNum = Random.Range(2, 5);
        while(portalNum % 2 != 0)
        {
            portalNum = Random.Range(2, 5);
        }
        portalPair = new int[portalNum, 4];
        for(int x = 0; x < portalNum; x++)
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
                portalPair[x, 0] = i;
                portalPair[x, 1] = j;
                portalPair[x + 1, 2] = i;
                portalPair[x + 1, 3] = j;
            }
            else
            {
                portalPair[x, 0] = i;
                portalPair[x, 1] = j;
                portalPair[x - 1, 2] = i;
                portalPair[x - 1, 3] = j;
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
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(trapPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
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
                }
            }
        }
    }
    ///*
    //purpose:建造橋，選出被孤立的grass，且周圍4格被water包圍，任意選兩個方向在water上造橋，直到遇到grass則停止
    //*/
    //private void CreateBridge()
    //{
    //    for (int i = 0; i < mapSize; i++)
    //    {
    //        for (int j = 0; j < mapSize; j++)
    //        {
    //            if (isolatedGrid[i, j] == 1 && waterGrid[i - 1, j] == 1 && waterGrid[i + 1, j] == 1 && waterGrid[i, j - 1] == 1 && waterGrid[i, j + 1] == 1)
    //            {

    //                HashSet<int> randomNumbers = new HashSet<int>();
    //                randomNumbers.Add(Random.Range(0, 4));
    //                while (randomNumbers.Count < 2)
    //                {
    //                    randomNumbers.Add(Random.Range(0, 4));
    //                }

    //                isolatedGrid[i, j] = 0;
    //                grassGrid[i, j] = 0;
    //                bridgeGrid[i, j] = 1;
    //                waterGrid[i, j] = 0;
    //                Destroy(gridObj[i, j]);
    //                gridObj[i, j] = Instantiate(waterPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
    //                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
    //                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
    //                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;

    //                foreach (int x in randomNumbers)
    //                {
    //                    if (x == 0)
    //                    {
    //                        CreateBridgeAbove(waterGrid, i, j, 0);
    //                    }
    //                    else if (x == 1)
    //                    {
    //                        CreateBridgeLeft(waterGrid, i, j, 0);
    //                    }
    //                    else if (x == 2)
    //                    {
    //                        CreateBridgeBelow(waterGrid, i, j, 0);
    //                    }
    //                    else if (x == 3)
    //                    {
    //                        CreateBridgeRight(waterGrid, i, j, 0);
    //                    }
    //                }



    //            }
    //        }
    //    }
    //}
    ///*
    //purpose:隨機生成道具
    //*/
    //private void CreateItem()
    //{
    //    int randomResult;
    //    int itemPossibility = mapSize / 3;

    //    for (int i = 0; i < mapSize; i++)
    //    {
    //        for (int j = 0; j < mapSize; j++)
    //        {
    //            randomResult = Random.Range(0, 100);
    //            if ((grassGrid[i, j] == 1 || bridgeGrid[i, j] == 1) && randomResult < itemPossibility && !IsBoundary(i, j) && isolatedGrid[i, j] != 1)
    //            {
    //                gridObj[i, j] = Instantiate(trapPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
    //                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
    //            }
    //            if ((grassGrid[i, j] == 1 || bridgeGrid[i, j] == 1) && randomResult >= itemPossibility && randomResult < 3 * itemPossibility && !IsBoundary(i, j) && isolatedGrid[i, j] != 1)
    //            {
    //                //gridObj[i, j] = Instantiate(starPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
    //                //gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
    //            }
    //        }
    //    }
    //}
    /*
    purpose:將產生的地圖存入terrian 2d-array以供存取
    */
    //barren = 0, grass = 1, puddle, reed = 3, trap = 4, tree = 5, portal = 6,  water = 7, isolated grass = 100
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

            }
        }
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                print(S_Terrain[i, j]);
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
    purpose:向格子上方造橋
    */
    private void CreateBridgeAbove(int[,] tempGrid, int i, int j, int count)
    {
        if (tempGrid[i + 1, j] != 1)
        {
            bridgeGrid[i, j] = 1;
            waterGrid[i, j] = 0;
            gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        }
        else
        {
            if (count == 0)
            {
                CreateBridgeAbove(tempGrid, i + 1, j, count + 1);
            }
            else
            {
                bridgeGrid[i, j] = 1;
                waterGrid[i, j] = 0;
                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                CreateBridgeAbove(tempGrid, i + 1, j, count + 1);
            }

        }
    }
    /*
    purpose:向格子下方造橋
    */
    private void CreateBridgeBelow(int[,] tempGrid, int i, int j, int count)
    {
        if (tempGrid[i - 1, j] != 1)
        {
            bridgeGrid[i, j] = 1;
            waterGrid[i, j] = 0;
            gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        }
        else
        {
            if (count == 0)
            {
                CreateBridgeBelow(tempGrid, i - 1, j, count + 1);
            }
            else
            {
                bridgeGrid[i, j] = 1;
                waterGrid[i, j] = 0;
                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                CreateBridgeBelow(tempGrid, i - 1, j, count + 1);
            }

        }
    }
    /*
    purpose:向格子左方造橋
    */
    private void CreateBridgeLeft(int[,] tempGrid, int i, int j, int count)
    {
        if (tempGrid[i, j - 1] != 1)
        {
            bridgeGrid[i, j] = 1;
            waterGrid[i, j] = 0;
            gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        }
        else
        {
            if (count == 0)
            {
                CreateBridgeLeft(tempGrid, i, j - 1, count + 1);
            }
            else
            {
                bridgeGrid[i, j] = 1;
                waterGrid[i, j] = 0;
                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                CreateBridgeLeft(tempGrid, i, j - 1, count + 1);
            }

        }
    }
    /*
    purpose:向格子右方造橋
    */
    private void CreateBridgeRight(int[,] tempGrid, int i, int j, int count)
    {
        if (tempGrid[i, j + 1] != 1)
        {
            bridgeGrid[i, j] = 1;
            waterGrid[i, j] = 0;
            gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
        }
        else
        {
            if (count == 0)
            {
                CreateBridgeRight(tempGrid, i, j + 1, count + 1);
            }
            else
            {
                bridgeGrid[i, j] = 1;
                waterGrid[i, j] = 0;
                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                gridObj[i, j].transform.parent = GameObject.Find("Map").transform;
                CreateBridgeAbove(tempGrid, i, j + 1, count + 1);
            }

        }
    }

    //update is called once per frame
    void Update()
    {

    }


}