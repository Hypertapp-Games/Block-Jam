using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public int rows;
    public int cols;
    private int[,] grid;
    private GameObject[,] gridObject;
    public GameObject tileSprite;
    public string filePath;
    private void Start()
    {
        LoadFileTextToGrid();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            GenerateTile();
        }
    }
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
            gridObject = new GameObject[rows, cols];
            grid = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    grid[i, j] = 1; // Set Default Tile is Block Tile

                    var tile = Instantiate(tileSprite, new Vector3(j, rows - i, 0), Quaternion.identity);
                    gridObject[i, j] = tile;
                    tile.transform.SetParent(gameObject.transform);
                }
            }

        }
    }
    public void LoadFileTextToGrid()
    {
        if (File.Exists(filePath))
        {
            grid = LoadArrayFromFile(filePath);
        }
        else
        {
            Debug.LogError("Không tìm thấy tệp văn bản.");
        }
    }
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
}
