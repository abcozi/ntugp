# ntugp
NTU game programming project

#Updates:
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
 
#Imported Packages: 
- UniVRM-0.53.0_6b07.unitypackage
  功能: 使3d建模人物可以相容於Unity
  import後會產生一個VRM的資料夾
- Standard Assets
- Unity Chan: 可以使用於VRM模型的動畫Package
- Tile Map Package: 地圖Package
- Unity Network Lobby Package

#Assets:
- VRM資料夾：
  有四個角色的vrm檔(andrea, brandon, celine, dean)
  另外，UniVRM資料夾中有Scripts可以針對角色動作
- Art資料夾：
  - Fonts
  - Images
