using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

public class ToolGridEditor : MonoBehaviour
{
    private int rows;
    private int cols;
    public GameObject tileSprite;
    private Camera cam;

    private int currentRows;
    private int currentColumns;

    public List<Color> tileColors;
    public GameObject tileButtonPanel;
    public GameObject holeButtonPanel;

    private int[,] grid;
    private GameObject[,] gridObject;

    private Color colorChange;
    private bool isHole;
    private int id;
    private Image currentButtonSelect;

    public List<Color> Colors;
    private int mode = 0; //  0 editmode,  1 play mode,  2 randommode

    public TMP_InputField WidthInputField;
    public TMP_InputField HeightInputField;

    [Header("Copy đường dẫn của file text và dán vào đây")]
    public string filePath;

    private void Start()
    {
        cam = Camera.main;
        ChangeButtonColor();
        GenerateTileOnStart();
    }


    // Input: Click On Tile Or Hole Button
    // Output: Save button color to "colorChange", button id to "id", button status(normal tile , hold) to "isHole", hightlight slected button
    public void TileAndHoleButtonClick()
    {
        if (currentButtonSelect != null)
        {
            currentButtonSelect.color = Colors[0];
        }

        GameObject clickButton = EventSystem.current.currentSelectedGameObject;
        currentButtonSelect = clickButton.GetComponent<Image>();
        currentButtonSelect.color = Colors[1];
        colorChange = clickButton.transform.GetChild(0).GetComponent<Image>().color;

        id = tileColors.IndexOf(colorChange);
        isHole = clickButton.name[0] == 'H';
    }

    private void Update()
    {
        if (mode == 0)
        {
            DrawTile();
        }
        else if (mode == 1)
        {
            ChoseSnake();
            SnakeDrag();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            // Không quan trọng
            DeBugNumberAray(grid);
        }

    }

    // Turn On PlayMode;
    public void PlayModeOn_Button()
    {
        if (mode == 0)
        {
            CloneGrid();
        }
        mode = 1;
        PlayMode();
    }

    // Turn On EditMode;
    public void EditModeOn_Button()
    {
        if (mode != 0)
        {
            ReGetValueOfGrid();
        }
        mode = 0;
    }


    private int[,] cloneGird;
    void CloneGrid()
    {
        cloneGird = (int[,])grid.Clone();
    }

    void ReGetValueOfGrid()
    {
        grid = (int[,])cloneGird.Clone();
        LoadGridVisual();
    }


    // Input: Click on Export button
    // Output: Create filePath on ScreenCapture folder, call function SaveArrayToFile
    public void ExportBtnClick()
    {
        var time = DateTime.Now.ToString("dd_MM_yyyy (HH:mm:ss)");
        string filePath = "Assets/Data/arrayData" + time + ".txt";

        SaveArrayToFile(filePath, grid);

        Debug.Log("Dữ liệu đã được lưu vào tệp văn bản.");
    }

    // Input: Click on Import, "filePath"
    // Output: Check if the path exists or not,  if path exists, call function LoadArrayFromFile, LoadGridVisual
    public void LoadFileTextToGrid()
    {
        if (File.Exists(filePath))
        {
            grid = LoadArrayFromFile(filePath);
            LoadGridVisual();
        }
        else
        {
            Debug.LogError("Không tìm thấy tệp văn bản.");
        }
    }

    // Input: tileColors, tileButtonPanel, holeButtonPanel
    // Output: Đổi màu của button
    void ChangeButtonColor()
    {
        List<Image> tileButton = tileButtonPanel.GetComponentsInChildren<Image>().ToList();
        List<Image> listTileButtonOut = (from element in tileButton

                                         where element.GetComponent<Button>() == null

                                         select (Image)element).ToList();
        tileButton = new List<Image>(listTileButtonOut);

        for (int i = 0; i < tileButton.Count; i++)
        {
            tileButton[i].color = tileColors[i];
        }


        List<Image> holeButton = holeButtonPanel.GetComponentsInChildren<Image>().ToList();

        List<Image> listHoleButtonOut = (from element in holeButton

                                         where element.GetComponent<Button>() == null

                                         select (Image)element).ToList();
        holeButton = new List<Image>(listHoleButtonOut);
        for (int i = 0; i < holeButton.Count; i++)
        {
            holeButton[i].color = tileColors[i + 2];
        }
    }



    // Input: Mouse position
    // Output: call function ChangeTileColor
    private bool onMouseDrag = false;
    private GameObject lastHighlightedObject;

    void DrawTile()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 100))
        {
            ClearHighlightedObject();
            return; // để kết thúc ngay lập tức hàm khi raycast không va chạm
        }

        GameObject currentObject = hit.transform.gameObject;

        if (currentObject != lastHighlightedObject)
        {
            ClearHighlightedObject();
            currentObject.transform.GetChild(1).gameObject.SetActive(true);
            lastHighlightedObject = currentObject;
        }

        if (Input.GetMouseButton(0))
        {
            onMouseDrag = true;
            if (onMouseDrag)
            {
                ChangeTileColor(currentObject, colorChange, isHole, id);
            }
        }
        else
        {
            onMouseDrag = false;
        }
    }

    void ClearHighlightedObject()
    {
        if (lastHighlightedObject != null)
        {
            lastHighlightedObject.transform.GetChild(1).gameObject.SetActive(false);
            lastHighlightedObject = null;
        }
    }


    //Input: Tile selected, "colorChange", "isHole", "id"
    //Output: Change tile color, change value on "grid", show hole if tile is Hole
    void ChangeTileColor(GameObject tile, Color color, bool isHole, int id)
    {
        // Hoán đổi giá trị id giữa màu đen và màu trắng
        if (id == 1)
        {
            id = 0;
        }
        else if (id == 0)
        {
            id = 1;
        }

        Transform tileTransform = tile.transform;
        Transform childTransform = tileTransform.GetChild(0).gameObject.transform.GetChild(0);

        if (isHole)
        {
            // Hiển thị đối tượng hố và thiết lập màu cho nó
            tileTransform.GetChild(0).gameObject.SetActive(true);
            childTransform.GetComponent<SpriteRenderer>().color = color;
            grid[(int)(currentRows - tileTransform.position.y), (int)tileTransform.position.x] = 100 + id;
        }
        else
        {
            // Ẩn đối tượng hố và thiết lập màu cho ô đất
            tileTransform.GetChild(0).gameObject.SetActive(false);
            tile.GetComponent<SpriteRenderer>().color = color;
            grid[(int)(currentRows - tileTransform.position.y), (int)tileTransform.position.x] = id;
        }
    }


    // Input: WidthInputField, HeightInputField (nhập số trên màn hình)
    // Output: Click ExportSize để gọi hàm GenerateTile
    public void GenerateTileButton()
    {
        rows = int.Parse(WidthInputField.text);
        cols = int.Parse(HeightInputField.text);
        GenerateTile();
    }

    public void GenerateTileOnStart()
    {
        GenerateTileButton();
    }

    // Input: WidthInputField, HeightInputField
    // Output: Generate ra các tile
    void GenerateTile()
    {
        if (transform.childCount != 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            transform.DetachChildren();
        }

        if (gameObject.transform.childCount == 0)
        {
            currentRows = rows;
            currentColumns = cols;
            gridObject = new GameObject[currentRows, currentColumns];
            grid = new int[currentRows, currentColumns];
            for (int i = 0; i < currentRows; i++)
            {
                for (int j = 0; j < currentColumns; j++)
                {
                    grid[i, j] = 1; // Set Default Tile is Block Tile

                    var tile = Instantiate(tileSprite, new Vector3(j, currentRows - i, 0), Quaternion.identity);
                    gridObject[i, j] = tile;
                    tile.transform.SetParent(gameObject.transform);
                }
            }

        }

        ChangCameraView();
    }

    // Input: Kích thước của mảng gird
    // Output: Scale camera để các tile không bị lệch ra khỏi màn hình
    void ChangCameraView()
    {
        float minSize = 0;
        if (currentRows > minSize)
        {
            minSize = currentRows;
        }

        if (currentColumns > minSize)
        {
            minSize = currentColumns;
        }

        cam.transform.position = new Vector3((minSize / 10) - 0.5f, (minSize + 1) / 2, -10);
        cam.orthographicSize = (minSize + 1) / 2;
    }


    // This is code PlayMode

    [Serializable]
    public struct Tile
    {
        public int x;
        public int y;
        public GameObject ob;
        public int tileID;

        public Tile(int x, int y, GameObject ob, int tileID)
        {
            this.x = x;
            this.y = y;
            this.ob = ob;
            this.tileID = tileID;
        }
    }

    [Serializable]
    public struct Snake
    {
        public List<Tile> allTile;
        public Dictionary<int, List<Tile>> adjacentTile;
        public int snakeID;

        public Snake(List<Tile> allTile, Dictionary<int, List<Tile>> adjacentTile, int snakeID)
        {
            this.allTile = allTile;
            this.adjacentTile = adjacentTile;
            this.snakeID = snakeID;
        }

        // Input: bool Head(tile được chọn là head hay tail), giá trị x và y mới, gameObject được gán mới
        // Output: Thay đổi các giá trị từng tile của snake
        public void SnakeMove(bool head, int x, int y, GameObject gameobject)
        {
            int startIndex = (head) ? 0 : allTile.Count - 1;
            int endIndex = (head) ? allTile.Count : -1;
            int step = (head) ? 1 : -1;

            for (int i = startIndex; i != endIndex; i += step)
            {
                var tempx = allTile[i].x;
                var tempy = allTile[i].y;
                var tempob = allTile[i].ob;
                var id = allTile[i].tileID;

                allTile[i] = new Tile(x, y, gameobject, id);

                x = tempx;
                y = tempy;
                gameobject = tempob;
            }
        }

        public void SnakeBack(List<int[]> tempTileLocate)
        {
            for (int i = 0; i < allTile.Count; i++)
            {
                var gameobject = allTile[i].ob;
                var id = allTile[i].tileID;
                allTile[i] = new Tile(tempTileLocate[i][0], tempTileLocate[i][1], gameobject, id);
            }
        }
    }

    private Dictionary<int, Snake> allIdOfSnakes = new Dictionary<int, Snake>();
    public List<Snake> allSnakes;

    void PlayMode()
    {
        allIdOfSnakes.Clear();
        allSnakes.Clear();
        LoadSnakeFromMatrix();
    }

    // Input: mảng 2 chiều grid
    // Output: list lưu trữ tất cả snake có trong grid: allSnakes
    void LoadSnakeFromMatrix()
    {
        for (int row = 0; row < currentRows; row++)
        {
            for (int column = 0; column < currentColumns; column++)
            {
                int currentGridValue = grid[row, column];

                if (currentGridValue != 0 && currentGridValue != 1 && currentGridValue.ToString().Length != 3)
                {
                    if (allIdOfSnakes.ContainsKey(currentGridValue))
                    {
                        var ob = gridObject[row, column];
                        allIdOfSnakes[currentGridValue].allTile.Add(new Tile(row, column, ob, 100));
                    }
                    else
                    {
                        List<Tile> allTile = new List<Tile> { new Tile(row, column, gridObject[row, column], 100) };
                        Dictionary<int, List<Tile>> adjacentTile = new Dictionary<int, List<Tile>>();
                        allIdOfSnakes[currentGridValue] = new Snake(allTile, adjacentTile, currentGridValue);
                    }
                }
            }
        }

        foreach (var snake in allIdOfSnakes)
        {
            allSnakes.Add(snake.Value);
        }

        SortSnake();
    }


    // Input: allSnakes
    // Output: gọi hàm LoadAdjacentTileOfEachTile
    void SortSnake()
    {
        LoadAdjacentTileOfEachTile();
    }

    //Input: allSnakes;
    //Output: với mỗi snake trong allSnakes, với mỗi tile trong snake kiểm tra các tile liền kề và thêm vào adjacentTile
    void LoadAdjacentTileOfEachTile()
    {
        // Cho từng con rắn trong danh sách
        for (int i = 0; i < allSnakes.Count; i++)
        {
            // Lấy thông tin của con rắn hiện tại
            var snake = allSnakes[i];
            var allTile = snake.allTile;
            var adjacentTile = snake.adjacentTile;

            // Duyệt qua tất cả các ô của con rắn
            for (int j = 0; j < allTile.Count; j++)
            {
                // Lấy thông tin của ô bắt đầu
                var startTile = allTile[j];

                // Duyệt qua tất cả các ô khác trong con rắn
                for (int k = 0; k < allTile.Count; k++)
                {
                    // Bỏ qua ô hiện tại
                    if (j != k)
                    {
                        // Lấy thông tin của ô kết thúc
                        var endTile = allTile[k];

                        // Tính khoảng cách giữa hai ô
                        var distance = Math.Sqrt(Math.Pow(startTile.x - endTile.x, 2) +
                                                 Math.Pow(startTile.y - endTile.y, 2));

                        // Nếu khoảng cách bằng 1, đánh dấu ô kết thúc là ô lân cận của ô bắt đầu
                        if (distance == 1)
                        {
                            if (!adjacentTile.TryAdd(j, new List<Tile> { endTile }))
                            {
                                adjacentTile[j].Add(endTile);
                            }
                        }
                    }
                }
            }
        }

        MarkedIdOfTile();
    }

    //Input: adjacentTile;
    //Output: Các tile được đánh số thứ tự (id)
    void MarkedIdOfTile()
    {
        for (int i = 0; i < allSnakes.Count; i++)
        {
            // Lấy thông tin của con rắn hiện tại
            var snake = allSnakes[i];
            var allTile = snake.allTile;
            var adjacentTile = snake.adjacentTile;

            // Duyệt qua tất cả các ô lân cận của từng ô trong con rắn
            foreach (var pair in adjacentTile)
            {
                // Nếu ô chỉ có một ô lân cận, đánh dấu ô hiện tại có ID là 0
                if (pair.Value.Count == 1)
                {
                    var tempTile = allTile[pair.Key];
                    allTile[pair.Key] = new Tile(tempTile.x, tempTile.y, tempTile.ob, 0);
                    LoadHeadAndTileOfSnake(snake, 0, pair.Key);
                    break;
                }
            }
        }

        SortTileOfSnakeByTileID();
    }

    // Hàm đệ quy, đánh số thứ tự cho đến hết các tile của snake
    void LoadHeadAndTileOfSnake(Snake snake, int tileID, int index)
    {
        tileID++;

        // Nếu chưa đánh dấu hết các ô trong con rắn
        if (tileID < snake.allTile.Count)
        {
            // Duyệt qua tất cả các ô lân cận của từng ô trong con rắn
            foreach (var pair in snake.adjacentTile)
            {
                // Duyệt qua tất cả các ô lân cận
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    // Nếu tìm thấy ô lân cận có tọa độ giống với ô hiện tại
                    if (pair.Value[i].x == snake.allTile[index].x && pair.Value[i].y == snake.allTile[index].y)
                    {
                        var tempTile = snake.allTile[pair.Key];

                        // Nếu ô lân cận có ID là 100, đánh dấu ID của ô lân cận và tiếp tục đệ quy
                        if (tempTile.tileID == 100)
                        {
                            snake.allTile[pair.Key] = new Tile(tempTile.x, tempTile.y, tempTile.ob, tileID);
                            LoadHeadAndTileOfSnake(snake, tileID, pair.Key);
                            break;
                        }
                    }
                }
            }
        }
    }

    // Input: allSnakes
    // Output: Sắp sếp lại tile trong mỗi snake theo số thứ tự
    void SortTileOfSnakeByTileID()
    {
        // Duyệt qua tất cả các con rắn trong danh sách
        for (int i = 0; i < allSnakes.Count(); i++)
        {
            var snake = allSnakes[i];

            // Sắp xếp các ô trong con rắn theo ID
            var sortedTiles = snake.allTile.OrderBy(t => t.tileID).ToList();

            // Gán lại danh sách các ô đã sắp xếp cho con rắn
            allSnakes[i] = new Snake(sortedTiles, snake.adjacentTile, snake.snakeID);
        }
    }

    private int snakeIndex = 100;

    private bool isSnakeHead = true;

    // Input: Mouse position
    // Ouput: Kiểm tra chuột có đang trỏ vào snake nào không, nếu có lưu lại vị trí của snake trong list allSnakes 
    void ChoseSnake()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool rayCastDown = Physics.Raycast(ray, out hit, 100);

        if (Input.GetMouseButtonDown(0) && rayCastDown)
        {
            var color = hit.transform.gameObject.GetComponent<SpriteRenderer>().color;

            int index = tileColors.FindIndex(c => c == color);

            var snake = allSnakes.FirstOrDefault(s => s.snakeID == index);

            snakeIndex = allSnakes.IndexOf(snake);

            CheckSnakeIsChosen(snakeIndex, hit.transform.gameObject);
        }

        if (Input.GetMouseButtonUp(0))
        {
            snakeIndex = 1000;
        }
    }

    // Input: vị trí của snake trong list allSnakes, tile đang được chọn
    // Output: Kiểm tra tile được chọn là head hay tail, lưu vào bool isSnakeHead
    void CheckSnakeIsChosen(int index, GameObject ob)
    {
        CheckSnakeTileIsChosen_HeadOrTail(allSnakes[index], ob);
    }

    void CheckSnakeTileIsChosen_HeadOrTail(Snake snake, GameObject ob)
    {
        int lastIndex = snake.allTile.Count - 1;

        //new Vector3( column,  currentRows - row, 0)
        for (int i = 0; i < snake.allTile.Count; i++)
        {
            if (snake.allTile[i].ob == ob)
            {
                isSnakeHead = snake.allTile[i].tileID == 0;
            }
        }
    }


    // This is Drag Code
    private readonly Vector2 mXAxis = new Vector2(1, 0);
    private readonly Vector2 mYAxis = new Vector2(0, 1);
    private const float mAngleRange = 30;

    private const float mMinSwipeDist = 80.0f;

    private Vector2 mStartPosition;

    // Input: MousePosition
    // Output: Kiêm tra đang kéo snake theo hướng nào
    void SnakeDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mStartPosition = new Vector2(Input.mousePosition.x,
                Input.mousePosition.y);
        }

        Vector2 endPosition = new Vector2(Input.mousePosition.x,
            Input.mousePosition.y);
        Vector2 swipeVector = endPosition - mStartPosition;

        if (swipeVector.magnitude > mMinSwipeDist)
        {
            mStartPosition = endPosition;
            swipeVector.Normalize();

            float angleOfSwipe = Vector2.Dot(swipeVector, mXAxis);
            angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;

            if (angleOfSwipe < mAngleRange)
            {
                OnSwipeRight();
            }
            else if ((180.0f - angleOfSwipe) < mAngleRange)
            {
                OnSwipeLeft();
            }
            else
            {
                angleOfSwipe = Vector2.Dot(swipeVector, mYAxis);
                angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                if (angleOfSwipe < mAngleRange)
                {
                    OnSwipeTop();
                }
                else if ((180.0f - angleOfSwipe) < mAngleRange)
                {
                    OnSwipeBottom();
                }
            }
        }
    }

    void OnSwipeRight()
    {
        //Debug.Log("Right");
        if (snakeIndex != 1000)
        {
            SnakeMove(0, 1);
        }

    }

    void OnSwipeLeft()
    {
        //Debug.Log("Left");
        if (snakeIndex != 1000)
        {
            SnakeMove(0, -1);
        }
    }

    void OnSwipeTop()
    {
        //Debug.Log("Top");
        if (snakeIndex != 1000)
        {
            SnakeMove(-1, 0);
        }
    }

    void OnSwipeBottom()
    {
        //Debug.Log("Bottom");
        if (snakeIndex != 1000)
        {
            SnakeMove(1, 0);
        }
    }

    // Input: x tăng thêm và y tăng thêm
    // Output: Di chuyển snake
    void SnakeMove(int x, int y)
    {
        int currentX;
        int currentY;

        if (isSnakeHead)
        {
            currentX = allSnakes[snakeIndex].allTile[0].x;
            currentY = allSnakes[snakeIndex].allTile[0].y;
        }
        else
        {
            currentX = allSnakes[snakeIndex].allTile[allSnakes[snakeIndex].allTile.Count - 1].x;
            currentY = allSnakes[snakeIndex].allTile[allSnakes[snakeIndex].allTile.Count - 1].y;
        }

        if (CheckCanMove(x, y, currentX, currentY, allSnakes[snakeIndex].snakeID))
        {
            ChangeValueOfTheMatrixBeforeSnakeMove(allSnakes[snakeIndex]);
            allSnakes[snakeIndex].SnakeMove(isSnakeHead, currentX + x, currentY + y,
                gridObject[currentX + x, currentY + y]);
            ChangeValueOfTheMatrixAfterSnakeMove(allSnakes[snakeIndex]);
        }
    }

    // Input: x tăng thêm ,y tăng thêm, x hiện tại, y hiện tại , id của snake
    // Output: kiểm tra có di chuyển được hay không
    //         kiểm tra nếu ô tiếp theo là lỗ thì xoá snake khỏi mảng
    bool CheckCanMove(int x, int y, int currentX, int currentY, int snakeID)
    {
        int targetX = currentX + x;
        int targetY = currentY + y;

        if (targetX < 0 || targetX >= currentRows || targetY < 0 || targetY >= currentColumns)
        {
            return false;
        }

        if (grid[targetX, targetY] == 0)
        {
            return true;
        }

        if (grid[targetX, targetY].ToString().Length == 3)
        {
            var num_1 = Int32.Parse(grid[targetX, targetY].ToString()[1].ToString());
            var num_2 = Int32.Parse(grid[targetX, targetY].ToString()[2].ToString());
            if ((num_1 * 10 + num_2) == snakeID)
            {
                Debug.Log("SnakeInHole");
                ChangeValueOfTheMatrixBeforeSnakeMove(allSnakes[snakeIndex]);
                allSnakes.Remove(allSnakes[snakeIndex]);
                snakeIndex = 100;
                return false;
            }
        }

        return false;
    }

    // Input: snake
    // Output: Đổi màu thành trắng và đổi giá trị của grid = 0;
    void ChangeValueOfTheMatrixBeforeSnakeMove(Snake snake)
    {
        for (int i = 0; i < snake.allTile.Count; i++)
        {
            grid[snake.allTile[i].x, snake.allTile[i].y] = 0;
            ChangeTileColor(gridObject[snake.allTile[i].x, snake.allTile[i].y], tileColors[1]);
        }
    }

    // Input: snake
    // Output: Đổi màu và đổi giá trị của grid tương ứng với các tile của snake
    void ChangeValueOfTheMatrixAfterSnakeMove(Snake snake)
    {
        for (int i = 0; i < snake.allTile.Count; i++)
        {
            grid[snake.allTile[i].x, snake.allTile[i].y] = snake.snakeID;
            ChangeTileColor(gridObject[snake.allTile[i].x, snake.allTile[i].y], tileColors[snake.snakeID]);
        }
    }

    // Input: tile, color
    // Output: Đổi màu của tile (Khi di chuyển)
    void ChangeTileColor(GameObject tile, Color color)
    {
        tile.transform.GetChild(0).gameObject.SetActive(false);
        tile.GetComponent<SpriteRenderer>().color = color;
    }

    // Input: đường dẫn tới thư mục được sẽ lưu file txt, grid
    // Output: Chuyển đổi grid thành file txt và lưu vào folder ScreenCapture
    void SaveArrayToFile(string filePath, int[,] arrayToSave)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < arrayToSave.GetLength(0); i++)
            {
                for (int j = 0; j < arrayToSave.GetLength(1); j++)
                {
                    writer.Write(arrayToSave[i, j]);

                    if (j < arrayToSave.GetLength(1) - 1)
                    {
                        writer.Write(",");
                    }
                }

                writer.WriteLine();
            }
        }
    }

    // Input: Đường dẫn file text
    // Output: Đọc file text, load các giá trị vào mảng 2 chiều gird
    private int[,] LoadArrayFromFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);


        int numRows = lines.Length;
        int numCols = lines[0].Split(',').Length;


        grid = new int[numRows, numCols];


        for (int i = 0; i < numRows; i++)
        {
            string[] values = lines[i].Split(',');

            for (int j = 0; j < numCols; j++)
            {

                int.TryParse(values[j], out grid[i, j]);
            }
        }

        return grid;
    }

    // Input: Gird
    // Output: Generate tile ra màn hình
    public void LoadGridVisual()
    {
        if (transform.childCount != 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            transform.DetachChildren();
        }

        if (gameObject.transform.childCount == 0)
        {
            currentRows = grid.GetLength(0);
            currentColumns = grid.GetLength(1);

            gridObject = new GameObject[currentRows, currentColumns];
            for (int i = 0; i < currentRows; i++)
            {
                for (int j = 0; j < currentColumns; j++)
                {
                    var tile = Instantiate(tileSprite, new Vector3(j, currentRows - i, 0), Quaternion.identity);
                    gridObject[i, j] = tile;
                    tile.transform.SetParent(gameObject.transform);

                    if (grid[i, j].ToString().Length != 3)
                    {

                        ChangeTileColor(tile, false, grid[i, j]);
                    }
                    else
                    {
                        ChangeTileColor(tile, true,
                            Int32.Parse(grid[i, j].ToString()[1].ToString()) * 10 +
                            Int32.Parse(grid[i, j].ToString()[2].ToString()));
                    }
                }
            }
        }

        ChangCameraView();
    }

    // Input: tile, isHole(là tile bình thường hay hole), id của tile
    // Output: Đổi màu theo id, đổi hình dạng của tile theo isHole
    void ChangeTileColor(GameObject tile, bool isHole, int id)
    {
        // hoan doi id cua mau den va mau trang
        if (id == 1)
        {
            id = 0;
        }
        else if (id == 0)
        {
            id = 1;
        }

        var color = tileColors[id];
        if (isHole)
        {
            tile.transform.GetChild(0).gameObject.SetActive(true);
            tile.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
        }
        else
        {
            tile.transform.GetChild(0).gameObject.SetActive(false);
            tile.GetComponent<SpriteRenderer>().color = color;
        }

    }

    void DeBugNumberAray(int[,] number)
    {
        // Debug.Log("       " + number[0, 0] + "       " + number[0, 1] + "       " + number[0, 2] + "       " + number[0, 3] + "       " + number[0, 4]);
        // Debug.Log("       " + number[1, 0] + "       " + number[1, 1] + "       " + number[1, 2] + "       " + number[1, 3] + "       " + number[1, 4]);
        // Debug.Log("       " + number[2, 0] + "       " + number[2, 1] + "       " + number[2, 2] + "       " + number[2, 3] + "       " + number[2, 4]);
        // Debug.Log("       " + number[3, 0] + "       " + number[3, 1] + "       " + number[3, 2] + "       " + number[3, 3] + "       " + number[3, 4]);
        // Debug.Log("       " + number[4, 0] + "       " + number[4, 1] + "       " + number[4, 2] + "       " + number[4, 3] + "       " + number[4, 4]);
        Debug.Log("       " + number[0, 0] + "       " + number[0, 1] + "       " + number[0, 2] + "       " +
                  number[0, 3] + "       " + number[0, 4] + "       " + number[0, 5] + "       " + number[0, 6] +
                  "       " + number[0, 7] + "       " + number[0, 8] + "       " + number[0, 9]);
        Debug.Log("       " + number[1, 0] + "       " + number[1, 1] + "       " + number[1, 2] + "       " +
                  number[1, 3] + "       " + number[1, 4] + "       " + number[1, 5] + "       " + number[1, 6] +
                  "       " + number[1, 7] + "       " + number[1, 8] + "       " + number[1, 9]);
        Debug.Log("       " + number[2, 0] + "       " + number[2, 1] + "       " + number[2, 2] + "       " +
                  number[2, 3] + "       " + number[2, 4] + "       " + number[2, 5] + "       " + number[2, 6] +
                  "       " + number[2, 7] + "       " + number[2, 8] + "       " + number[2, 9]);
        Debug.Log("       " + number[3, 0] + "       " + number[3, 1] + "       " + number[3, 2] + "       " +
                  number[3, 3] + "       " + number[3, 4] + "       " + number[3, 5] + "       " + number[3, 6] +
                  "       " + number[3, 7] + "       " + number[3, 8] + "       " + number[3, 9]);
        Debug.Log("       " + number[4, 0] + "       " + number[4, 1] + "       " + number[4, 2] + "       " +
                  number[4, 3] + "       " + number[4, 4] + "       " + number[4, 5] + "       " + number[4, 6] +
                  "       " + number[4, 7] + "       " + number[4, 8] + "       " + number[4, 9]);
        Debug.Log("       " + number[5, 0] + "       " + number[5, 1] + "       " + number[5, 2] + "       " +
                  number[5, 3] + "       " + number[5, 4] + "       " + number[5, 5] + "       " + number[5, 6] +
                  "       " + number[5, 7] + "       " + number[5, 8] + "       " + number[5, 9]);
        Debug.Log("       " + number[6, 0] + "       " + number[6, 1] + "       " + number[6, 2] + "       " +
                  number[6, 3] + "       " + number[6, 4] + "       " + number[6, 5] + "       " + number[6, 6] +
                  "       " + number[6, 7] + "       " + number[6, 8] + "       " + number[6, 9]);
        Debug.Log("       " + number[7, 0] + "       " + number[7, 1] + "       " + number[7, 2] + "       " +
                  number[7, 3] + "       " + number[7, 4] + "       " + number[7, 5] + "       " + number[7, 6] +
                  "       " + number[7, 7] + "       " + number[7, 8] + "       " + number[7, 9]);
        Debug.Log("       " + number[8, 0] + "       " + number[8, 1] + "       " + number[8, 2] + "       " +
                  number[8, 3] + "       " + number[8, 4] + "       " + number[8, 5] + "       " + number[8, 6] +
                  "       " + number[8, 7] + "       " + number[8, 8] + "       " + number[8, 9]);
        Debug.Log("       " + number[9, 0] + "       " + number[9, 1] + "       " + number[9, 2] + "       " +
                  number[9, 3] + "       " + number[9, 4] + "       " + number[9, 5] + "       " + number[9, 6] +
                  "       " + number[9, 7] + "       " + number[9, 8] + "       " + number[9, 9]);
    }

}