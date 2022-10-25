using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

public class GridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int GridSize = 100;
    public float tileSize = 1f;
    public float waterLevel = 0.4f;
    Cell[,] grid;
    
    [Header("Noise Settings")]
    public float NoiseScale, IslandSize;
    [Range(1, 20)] public int NoiseOctaves;
    [Range(0, 99999999)] public int Seed;


    void Start()
    {
       GenerateGrid();
       GenerateGridSprites();
    }
    public void OnValidate()
    {
        // delay the grid generation until the editor is done
        EditorApplication.delayCall += () =>
        {
           if (grid != null)
            {
                Debug.Log("Grid Updated");
                GenerateGrid();
                GenerateGridSprites();
            }
        };
    }

    void DestroyGrid()
    {
        // avoid 10gb of ram usage on every change
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void GenerateGrid()
    {
        // destroy the old grid
        DestroyGrid(); 

        // create a new grid
        grid = new Cell[GridSize, GridSize];
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                // get noise value 
                float noise = Noisefunction(x, y, GetSeed());
                // create & fill cell data
                Cell cell = new Cell();
                cell.isWater = noise < waterLevel;
                cell.noiseValue = noise;
                // add cell to grid
                grid[x, y] = cell;
                
            }
        }

        // center the camera instead
        Camera.main.transform.position = new Vector3(GridSize / 2, GridSize / 2, -10);

        Debug.Log("Grid Generated");
    }

    void SetCellSprite(int x, int y)
    {
        // load resources
        GameObject waterTileReference = (GameObject)Instantiate(Resources.Load("WaterTile"));
        GameObject landTileReference = (GameObject)Instantiate(Resources.Load("LandTile"));
        GameObject grassCornerTile = (GameObject)Instantiate(Resources.Load("GrassCornerTile"));
        GameObject grassEdgeTile = (GameObject)Instantiate(Resources.Load("GrassEdge"));

        GameObject groundTile = null;
        GameObject waterTile = null;
        float posX = x * tileSize;
        float posY = y * tileSize;
        
        // allways spawn water tiles
        waterTile = Instantiate(waterTileReference, new Vector3(posX, posY, 0), Quaternion.identity);
        // set layout order & parent
        waterTile.GetComponent<SpriteRenderer>().sortingOrder = 0;
        waterTile.transform.parent = transform;

        // if not water
        if (!grid[x, y].isWater) {
            if(IsOnLandEdge(x, y))
            {
                if(IsOnLandCorner(x, y))
                {
                    // spawn grass corner
                    float rotation = CalculateTileCornerRotation(x, y);
                    groundTile = Instantiate(
                        grassCornerTile,
                        new Vector3(posX, posY, 0),
                        Quaternion.Euler(0, 0, rotation) // rotate the tile
                    );
                }
                else
                {
                    // spawn grass edge
                    float rotation = CalculateTileEdgeRotation(x, y);
                    groundTile = Instantiate(
                        grassEdgeTile,
                        new Vector3(posX, posY, 0),
                        Quaternion.Euler(0, 0, rotation) // rotate the tile
                    );
                }
            }
            else
            {
                // spawn land tile
                groundTile = Instantiate(landTileReference, new Vector3(posX, posY, 0), Quaternion.identity);

            }
            // set layout order & parent
            groundTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
            groundTile.transform.parent = transform;
        }

        // destroy resources
        Destroy(waterTileReference);
        Destroy(landTileReference);
        Destroy(grassEdgeTile);
        Destroy(grassCornerTile);
    }

    bool IsOnLandCorner(int x, int y)
    {
        if (IsOnLandEdge(x, y))
        {
            // a corner is a cell that has at least 2 water cells around it
            int waterCells = 0;
            if (grid[x + 1, y].isWater) waterCells++;
            if (grid[x - 1, y].isWater) waterCells++;
            if (grid[x, y + 1].isWater) waterCells++;
            if (grid[x, y - 1].isWater) waterCells++;
            if (waterCells >= 2) return true;

        }
        return false;
    }

    float CalculateTileCornerRotation(int x, int y)
    {
        // calculate the rotation of the corner tile
        // the rotation is based on the direction of the water cells around the corner
        bool top = grid[x, y + 1].isWater;
        bool bottom = grid[x, y - 1].isWater;
        bool right = grid[x + 1, y].isWater;
        bool left = grid[x - 1, y].isWater;

        int rotation = 0;
        if (left && top) rotation = 0;
        if (left && bottom) rotation = 90;
        if (right && bottom) rotation = 180;
        if (right && top) rotation = 270;

        return rotation;
    }

    float CalculateTileEdgeRotation(int x, int y)
    {
        // the rotation is based on the direction of the water cells around the edge
        int rotation = 0;
        if (grid[x, y + 1].isWater) rotation = 0;
        if (grid[x - 1, y].isWater) rotation = 90;
        if (grid[x, y - 1].isWater) rotation = 180;
        if (grid[x + 1, y].isWater) rotation = 270;

        return rotation;
    }
    
    public void GenerateGridSprites() {
        // iterate grid
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {   
                // set sprite for cell
                SetCellSprite(x, y);
            }
        }

       
    }

    public bool IsOnLandEdge(int x, int y)
    {
        int[,] directions = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };
        for (int i = 0; i < 4; i++)
        {
            int newX = x + directions[i, 0];
            int newY = y + directions[i, 1];
            if (IsInGrid(newX, newY))
            {
                if (grid[newX, newY].isWater)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    public bool IsInGrid(int x, int y)
    {
        return x >= 0 && x < GridSize && y >= 0 && y < GridSize;
    }

    public Cell[,] GetNeighbours(int x, int y)
    {
        Cell[,] neighbours = new Cell[3, 3];
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int checkX = x + i;
                int checkY = y + j;
                if (checkX >= 0 && checkX < GridSize && checkY >= 0 && checkY < GridSize)
                {
                    Cell cell = grid[checkX, checkY];
                    if(cell != null)
                    {
                        neighbours[i + 1, j + 1] = cell;
                    }
                }
            }
        }
        return neighbours;
    }

    private float Noisefunction(float x, float y, Vector2 Origin)
    {

        float a = 0, noisesize = NoiseScale, opacity = 1;

        for (int octaves = 0; octaves < NoiseOctaves; octaves++)
        {
            float xVal = (x / (noisesize * GridSize)) + Origin.x;
            float yVal = (y / (noisesize * GridSize)) - Origin.y;
            float z = noise.snoise(new float2(xVal, yVal));
            a += Mathf.InverseLerp(0, 1, z) / opacity;

            noisesize /= 2f;
            opacity *= 2f;
        }

        return a -= FallOffMap(x, y, GridSize, IslandSize);
    }

    private float FallOffMap(float x, float y, int size, float islandSize)
    {
        float gradient = 1;

        gradient /= (x * y) / (GridSize * GridSize) * (1 - (x / GridSize)) * (1 - (y / GridSize));
        gradient -= 16;
        gradient /= islandSize;


        return gradient;
    }

     private Vector2 GetSeed()
    {
        return new Vector2(Mathf.Sqrt(Seed), Mathf.Sqrt(Seed));
    }
    
}
