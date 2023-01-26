using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCoordinate
{
    public int tileZIndex;
    public int tileXIndex;
    public int coordinateZIndex;
    public int coordinateXIndex;

    public TileCoordinate(int tileZIndex, int tileXIndex, int coordinateZIndex, int coordinateXIndex)
    {
        this.tileZIndex = tileZIndex;
        this.tileXIndex = tileXIndex;
        this.coordinateZIndex = coordinateZIndex;
        this.coordinateXIndex = coordinateXIndex;
    }
}

public class LevelData
{
    private int tileDepthInVertices, tileWidthInVertices;

    public TileData[,] tilesData;

    public LevelData(int tileDepthInVertices, int tileWidthInVertices, int levelDepthInTiles, int levelWidthInTiles)
    {
        tilesData = new TileData[tileDepthInVertices * levelDepthInTiles, tileWidthInVertices * levelWidthInTiles];

        this.tileDepthInVertices = tileDepthInVertices;
        this.tileWidthInVertices = tileWidthInVertices;
    }

    public void AddTileData(TileData tileData, int tileZIndex, int tileXIndex)
    {
        tilesData[tileZIndex, tileXIndex] = tileData;
    }

    public TileCoordinate ConvertToTileCoordinate(int zIndex, int xIndex)
    {
        int tileZIndex = (int)Mathf.Floor ((float)zIndex / (float)this.tileDepthInVertices);
        int tileXIndex = (int)Mathf.Floor ((float)xIndex / (float)this.tileWidthInVertices);

        int coordinateZIndex = this.tileDepthInVertices - (zIndex % this.tileDepthInVertices) - 1;
        int coordinateXIndex = this.tileWidthInVertices - (xIndex % this.tileDepthInVertices) - 1;

        TileCoordinate tileCoordinate = new TileCoordinate(tileZIndex, tileXIndex, coordinateZIndex, coordinateXIndex);
        return tileCoordinate;
    }
}

public class LevelGeneration : MonoBehaviour
{

    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;

    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private TreeGeneration treeGeneration;

    [SerializeField]
    private string dangerSenseLayerName;

    public LevelData levelData;

    // Start is called before the first frame update
    void Start()
    {
        float seed = Random.Range(-8000, 8000);
        GenerateMap(seed);   
    }

    void GenerateMap(float seed)
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        // Calculate number of vertices of the tile in each axis
        Vector3[] tileMeshVertices = tilePrefab.GetComponent<MeshFilter>().sharedMesh.vertices;
        int tileDepthInVertices = (int)Mathf.Sqrt(tileMeshVertices.Length);
        int tileWidthInVertices = tileDepthInVertices;

        float distanceBetweenVertices = (float)tileDepth / (float)tileDepthInVertices;

        this.levelData = new LevelData(tileDepthInVertices, tileWidthInVertices, mapDepthInTiles, mapWidthInTiles);

        tilePrefab.layer = LayerMask.NameToLayer(dangerSenseLayerName);

        for (int xTileIndex = 0; xTileIndex < mapWidthInTiles; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < mapDepthInTiles; zTileIndex++)
            {
                Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + xTileIndex * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + zTileIndex * tileDepth);

                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);

                TileData tileData = tile.GetComponent<TileGeneration>().GenerateTile(seed);
                
                levelData.AddTileData(tileData, zTileIndex, xTileIndex);
            }
        }

        treeGeneration.GenerateTrees(this.mapDepthInTiles * tileDepthInVertices, this.mapWidthInTiles * tileWidthInVertices, distanceBetweenVertices, levelData, tileWidth, tileDepth, seed);
    }
}
