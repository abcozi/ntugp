using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Map : MonoBehaviour
{
	//每個grid:2維array存各個地形或道具的分布
	//gridObj:初始化(生成)prefab的2維array
	//mapSize:地圖邊長
    public int [,] grassGrid, waterGrid, itemGrid, emptyGrid, isolatedGrid, bridgeGrid;
    public GameObject[,] gridObj;
    public int mapSize = Global.mapSize;
    public GameObject grassPrefab, trapPrefab, waterPrefab, starPrefab, bridgePrefab;
    //empty = 0, grass = 1, water = 2, isolated grass = 100
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
        FillArray(ref emptyGrid, 1, mapSize, mapSize);

        gridObj = new GameObject[mapSize, mapSize];

        //生成各種地形與道具
        CreateGrass();
        CreateWater();
        FillGrass();
        CreateWater();
        FillWater();
        CreateBridge();
        FindIsolatedGrass();
        CreateBridge();
        CreateItem();

    }
    /*
    purpose:建造grass地形，隨機產生
    生成位置在Vector3(i, 0, j)，i與j的範圍是0~mapSize
    */
    public void CreateGrass()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (Random.Range(0, 100) < 60)
                {
                    grassGrid[i, j] = 1;
                    emptyGrid[i, j] = 0;
                }
                if (grassGrid[i, j] == 1)
                {
                	//生成prefab
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                }
            }
        }
    }
    /*
    purpose:建造water地形，先用FindEnclosedPosition函數找出被grass包圍的empty地形
    把找到的生成water地形
    */
    public void CreateWater()
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
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                }

            }
        }
    }
    /*
    purpose:由於建造grass可能不連續，容易導致有些grass被孤立，走不到，因此需要填補起來
    方式:如果empty周圍3個都是grass，也將empty的生成grass
    如果empty周圍2個都是grass，也將empty的生成grass
    */
    public void FillGrass()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                int grass_count = 0;
                if (grassGrid[i, j] == 0 && !IsBoundary(i, j))
                {
                    if (grassGrid[i - 1, j] == 1)
                    {
                        grass_count++;
                    }
                    if (grassGrid[i + 1, j] == 1)
                    {
                        grass_count++;
                    }
                    if (grassGrid[i, j - 1] == 1)
                    {
                        grass_count++;
                    }
                    if (grassGrid[i, j + 1] == 1)
                    {
                        grass_count++;
                    }
                }
                if (grass_count == 3 && emptyGrid[i, j] == 1)
                {
                    grassGrid[i, j] = 1;
                    emptyGrid[i, j] = 0;
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                }
            }
        }
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                int grass_count = 0;
                if (grassGrid[i, j] == 0 && !IsBoundary(i, j))
                {
                    if ( grassGrid[i - 1, j] == 1)
                    {
                        grass_count++;
                    }
                    if (grassGrid[i + 1, j] == 1)
                    {
                        grass_count++;
                    }
                    if (grassGrid[i, j - 1] == 1)
                    {
                        grass_count++;
                    }
                    if (grassGrid[i, j + 1] == 1)
                    {
                        grass_count++;
                    }
                }
                if (grass_count == 2 && emptyGrid[i, j] == 1)
                {
                    grassGrid[i, j] = 1;
                    emptyGrid[i, j] = 0;
                    gridObj[i, j] = Instantiate(grassPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                }
            }
        }
    }
    /*
    purpose:由於建造的water可能太分散，因此把grass周圍3個為water的grass改成water
    讓湖泊變大
    */
    public void FillWater()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                int water_count = 0;
                if (waterGrid[i, j] == 0 && !IsBoundary(i, j))
                {
                    if (waterGrid[i - 1, j] == 1)
                    {
                        water_count++;
                    }
                    if (waterGrid[i + 1, j] == 1)
                    {
                        water_count++;
                    }
                    if (waterGrid[i, j - 1] == 1)
                    {
                        water_count++;
                    }
                    if (waterGrid[i, j + 1] == 1)
                    {
                        water_count++;
                    }
                }
                if (water_count == 3 && grassGrid[i, j] == 1)
                {
                    waterGrid[i, j] = 1;
                    grassGrid[i, j] = 0;
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(waterPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                }
            }
        }
    }
    /*
    purpose:找出被孤立，走不過去的grass，利用FindEnclosedPosition函數
    */
    public void FindIsolatedGrass()
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
    purpose:建造橋，選出被孤立的grass，且周圍4格被water包圍，任意選兩個方向在water上造橋，直到遇到grass則停止
    */
    public void CreateBridge()
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if(isolatedGrid[i, j] == 1 && waterGrid[i - 1, j] == 1 && waterGrid[i + 1, j] == 1 && waterGrid[i, j - 1] == 1 && waterGrid[i, j + 1] == 1)
                {

                    HashSet<int> random_numbers = new HashSet<int>();
                    random_numbers.Add(Random.Range(0, 4));
                    while(random_numbers.Count < 2)
                    {
                        random_numbers.Add(Random.Range(0, 4));
                    }

                    isolatedGrid[i, j] = 0;
                    grassGrid[i, j] = 0;
                    bridgeGrid[i, j] = 1;
                    waterGrid[i, j] = 0;
                    Destroy(gridObj[i, j]);
                    gridObj[i, j] = Instantiate(waterPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                    gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;

                    foreach (int x in random_numbers)
                    {
                        if(x == 0)
                        {
                            CreateBridgeAbove(waterGrid, i, j, 0);
                        }
                        else if(x == 1)
                        {
                            CreateBridgeLeft(waterGrid, i, j, 0);
                        }
                        else if (x == 2)
                        {
                            CreateBridgeBelow(waterGrid, i, j, 0);
                        }
                        else if (x == 3)
                        {
                            CreateBridgeRight(waterGrid, i, j, 0);
                        }
                    }
                    
                    
                    
                }
            }
        }
    }
    /*
    purpose:隨機生成道具
    */
    public void CreateItem()
    {
        int random_result;
        int item_posibility = mapSize / 3;

        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                random_result = Random.Range(0, 100);
                if ((grassGrid[i, j] == 1 || bridgeGrid[i, j] == 1) && random_result < item_posibility && !IsBoundary(i, j) && isolatedGrid[i, j] != 1)
                {
                    gridObj[i, j] = Instantiate(trapPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                }
                if ((grassGrid[i, j] == 1 || bridgeGrid[i, j] == 1) && random_result >= item_posibility && random_result < 3 * item_posibility && !IsBoundary(i, j) && isolatedGrid[i, j] != 1)
                {
                    //gridObj[i, j] = Instantiate(starPrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                    gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                }
            }
        }
    }
    /*
    purpose:找出被包圍的格子，利用深度優先搜尋
    */
    public void FindEnclosedPosition(int[,] be_enclosed_grid, int[,] enclose_grid, ref int[,] selected_grid)
    {
        int be_enclosed_position = 0, enclose_position = 1;
        int col = mapSize, row = mapSize;
        int[,] temp_grid = new int[mapSize, mapSize];


        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (be_enclosed_grid[i, j] == 1)
                {
                    temp_grid[i, j] = be_enclosed_position;
                }
                else if (enclose_grid[i, j] == 1)
                {
                    temp_grid[i, j] = enclose_position;
                }
            }
        }
        // top一行
        for (int i = 0; i < col; i++)
        {
            if (temp_grid[0, i] == be_enclosed_position)
            {
                temp_grid[0, i] = -1;
                DFS(0, i, be_enclosed_position, enclose_position, ref temp_grid);
            }
        }
        // bottom一行
        for (int i = 0; i < col; i++)
        {
            if (temp_grid[row - 1, i] == be_enclosed_position)
            {
                temp_grid[row - 1, i] = -1;
                DFS(row - 1, i, be_enclosed_position, enclose_position, ref temp_grid);
            }
        }
        //left一列
        for (int i = 1; i < row - 1; i++)
        {
            if (temp_grid[i, 0] == be_enclosed_position)
            {
                temp_grid[i, 0] = -1;
                DFS(i, 0, be_enclosed_position, enclose_position, ref temp_grid);
            }
        }
        // right一列
        for (int i = 1; i < row - 1; i++)
        {
            if (temp_grid[i, col - 1] == be_enclosed_position)
            {
                temp_grid[i, col - 1] = -1;
                DFS(i, col - 1, be_enclosed_position, enclose_position, ref temp_grid);
            }
        }
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (temp_grid[i, j] == be_enclosed_position)
                {
                    selected_grid[i, j] = 1;
                }
            }
        }

    }
	/*
    purpose:深度優先搜尋
    */
    public void DFS(int i, int j, int be_enclosed_position, int enclose_position, ref int[,] temp_grid)
    {
        int col = mapSize, row = mapSize;


        if (i > 1 && temp_grid[i - 1, j] == be_enclosed_position)
        {
            temp_grid[i - 1, j] = -1;
            DFS(i - 1, j, be_enclosed_position, enclose_position, ref temp_grid);
        }
        if (i < row - 1 && temp_grid[i + 1, j] == be_enclosed_position)
        {
            temp_grid[i + 1, j] = -1;
            DFS(i + 1, j, be_enclosed_position, enclose_position, ref temp_grid);
        }
        if (j > 1 && temp_grid[i, j - 1] == be_enclosed_position)
        {
            temp_grid[i, j - 1] = -1;
            DFS(i, j - 1, be_enclosed_position, enclose_position, ref temp_grid);
        }
        if (j < col - 1 && temp_grid[i, j + 1] == be_enclosed_position)
        {
            temp_grid[i, j + 1] = -1;
            DFS(i, j + 1, be_enclosed_position, enclose_position, ref temp_grid);

        }
    }
    /*
    purpose:將2d-array填滿給定的數字
    */
    public void FillArray(ref int[,] emptyGrid, int number, int col, int row)
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
    public bool IsBoundary(int i, int j)
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
    public void CreateBridgeAbove(int[,] temp_grid, int i, int j, int count)
    {
        if (temp_grid[i + 1, j] != 1)
        {
            bridgeGrid[i, j] = 1;
            waterGrid[i, j] = 0;
            gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
        }
        else
        {
            if(count == 0)
            {
                CreateBridgeAbove(temp_grid, i + 1, j, count + 1);
            }
            else
            {
                bridgeGrid[i, j] = 1;
                waterGrid[i, j] = 0;
                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                CreateBridgeAbove(temp_grid, i + 1, j, count + 1);
            }

        }
    }
    /*
    purpose:向格子下方造橋
    */
    public void CreateBridgeBelow(int[,] temp_grid, int i, int j, int count)
    {
        if (temp_grid[i - 1, j] != 1)
        {
            bridgeGrid[i, j] = 1;
            waterGrid[i, j] = 0;
            gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
        }
        else
        {
            if (count == 0)
            {
                CreateBridgeBelow(temp_grid, i - 1, j, count + 1);
            }
            else
            {
                bridgeGrid[i, j] = 1;
                waterGrid[i, j] = 0;
                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                CreateBridgeBelow(temp_grid, i - 1, j, count + 1);
            }

        }
    }
    /*
    purpose:向格子左方造橋
    */
    public void CreateBridgeLeft(int[,] temp_grid, int i, int j, int count)
    {
        if (temp_grid[i, j - 1] != 1)
        {
            bridgeGrid[i, j] = 1;
            waterGrid[i, j] = 0;
            gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
        }
        else
        {
            if (count == 0)
            {
                CreateBridgeLeft(temp_grid, i, j - 1, count + 1);
            }
            else
            {
                bridgeGrid[i, j] = 1;
                waterGrid[i, j] = 0;
                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                CreateBridgeLeft(temp_grid, i, j - 1, count + 1);
            }

        }
    }
    /*
    purpose:向格子右方造橋
    */
    public void CreateBridgeRight(int[,] temp_grid, int i, int j, int count)
    {
        if (temp_grid[i, j + 1] != 1)
        {
            bridgeGrid[i, j] = 1;
            waterGrid[i, j] = 0;
            gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
            gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
        }
        else
        {
            if (count == 0)
            {
                CreateBridgeRight(temp_grid, i, j + 1, count + 1);
            }
            else
            {
                bridgeGrid[i, j] = 1;
                waterGrid[i, j] = 0;
                gridObj[i, j] = Instantiate(bridgePrefab, transform.position + new Vector3(i, 0, j), Quaternion.identity);
                gridObj[i, j].transform.parent = GameObject.Find("Environment").transform;
                CreateBridgeAbove(temp_grid, i, j + 1, count + 1);
            }

        }
    }
    
    //update is called once per frame
    void Update()
    {

    }


}