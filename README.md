# ntugp
NTU game programming project

## Updates:
-191121 ver:
  -回合制改善
- 191118 ver:
  - 回合制雛形:
    - 由MasterClient掌控每個回合間的轉換(由GameManager.cs:37 Update()&M_RoundUpdate()處理)
    - M_TeamRoundUpdate()中呼叫RPC函式TeamRoundUpdate()更新LocalPlayer的teamRound值，LocalPLayer再各自判斷自己的隊伍是否為本次teamRound的值，再做出相對應的動作（攻or守）
  - 分組與搭擋：
    - 初始化時隨機分為team1 & team2。team1先攻後守，team2先守後攻。
    - player awake時便會被assign p_team& p_teamMate值（自己的組別值 與 自己的搭擋id值），p_team值會被應用在回合判斷上。
  - 角色：
    - 解決移動同步化的問題。
    - 加上角色移動動畫（步行、停止、面向行進方向）。

- 191103 ver:
  - 連機功能: 玩家登入、進入遊戲大廳、開房間、加入房間、開始遊戲、選角色、選完進入遊戲畫面。
  - 進入遊戲畫面後獲得player id。
  - 大廳內房間清單設定：房間內最多只能有四位，滿四人時該房間在大廳會顯示為full。若房間已經開始遊戲，該房間會在大廳隱藏。
  - 目前最低遊戲可行人數設為1人。（為了測試用途，正式版會改為4人）
  - 連線固定連到rue伺服器（俄羅斯）。

- 191017 ver:
  - 每次遊戲開始時，隨機產生地圖與角色生成點。
  - ＊待修改：禁止角色走出界部分可以再寫更精緻。
  - 新增Map的member function S_Terrain(i, j)給game manager取得該座標地形。
  - ＊待修改：地形尚未完全。

- 191009 ver:
  - Assets/Scripts/Player.cs: 
    - wardLocation使用List<Vector3>型態儲存。
  - 地圖設定:
    - 左下角座標為(0, 0, 0), 右下角(0, 0, 25), 左上角(25, 0, 0), 右上角(25, 0, 25)。
    - size: 25x25。
    - 邊界整齊不突出。
- 191006 ver:
  - 角色可以擲骰子，透過GameManager修改action point之後，進行移動。
  - 加入角色移動動畫、跟拍攝影機Main Camera、隨身光源Point Light。
  - disable default map中的光源（Directional Light, Side Light, 跟發光物件屬性）。
 
## Imported Packages: 
- UniVRM-0.53.0_6b07.unitypackage
  功能: 使3d建模人物可以相容於Unity
  import後會產生一個VRM的資料夾
- Standard Assets
- Unity Chan: 可以使用於VRM模型的動畫Package
- Tile Map Package: 地圖Package
- Unity Network Lobby Package

## Assets:
- VRM資料夾：
  有四個角色的vrm檔(andrea, brandon, celine, dean)
  另外，UniVRM資料夾中有Scripts可以針對角色動作
- Art資料夾：
  - Fonts
  - Images
