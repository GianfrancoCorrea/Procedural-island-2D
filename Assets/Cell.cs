using UnityEngine;
using System.Collections.Generic;



public class Cell
{
    public bool isWater;
    public float waterLevel;
    public CellType cellType;
    public float noiseValue;
    public Vector2Int position;
    public GameObject gameObject;
    public SpriteRenderer spriteRenderer;
    public Sprite sprite;
    public Dictionary<CellType, float> cellTypes = new Dictionary<CellType, float>();


    
    public Cell(float waterLevel, float noiseValue, Vector2Int position)
    {
        this.isWater = waterLevel > noiseValue;
        this.noiseValue = noiseValue;
        this.position = position;
        this.waterLevel = waterLevel;
        this.cellType = EvaluateCellType();
    }

    public void SetGameObject(GameObject gameObject)
    {
        this.gameObject = gameObject;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public CellType EvaluateCellType()
    {
        // use a dictionary to store the cell types
        // this way we can easily add new cell types
        cellTypes.Add(CellType.Water, waterLevel);
        cellTypes.Add(CellType.Sand, waterLevel + 0.1f);
        cellTypes.Add(CellType.Grass, waterLevel + 0.2f);
        cellTypes.Add(CellType.Forest, waterLevel + 0.3f);
        cellTypes.Add(CellType.Mountain, waterLevel + 0.4f);

        // loop through the cell types and return the first one that matches
        foreach (KeyValuePair<CellType, float> cellType in cellTypes)
        {
            if (noiseValue < cellType.Value)
            {
                return cellType.Key;
            }
        }

        return CellType.Mountain;
    }

}

public enum CellType
{
    Water,
    Sand,
    Grass,
    Forest,
    Mountain
}
