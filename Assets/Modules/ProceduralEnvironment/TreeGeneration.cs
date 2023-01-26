using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGeneration : MonoBehaviour
{
    [SerializeField]
    private NoiseMap noiseMapGeneration;

    [SerializeField]
    private Wave[] waves;

    [SerializeField]
    private float levelScale;

    [SerializeField]
    private float neighborRadius;

    [SerializeField]
    private float maxOffset;

    [SerializeField]
    private GameObject[] treePrefabs;
    public void GenerateTrees(int levelDepth, int levelWidth, float distanceBetweenVertices, LevelData levelData, int widthOfTile, int depthOfTile, float seed)
    {
        float[,] treeMap = this.noiseMapGeneration.GenerateNoiseMap(levelDepth, levelWidth, levelScale, 0, 0, this.waves, seed);

        float levelSizeX = levelWidth * distanceBetweenVertices;
        float levelSizeY = levelDepth * distanceBetweenVertices;

        for (int zIndex = 0; zIndex < levelDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < levelWidth; xIndex++)
            {
                TileCoordinate tileCoordinate = levelData.ConvertToTileCoordinate(zIndex, xIndex);
                TileData tileData = levelData.tilesData[tileCoordinate.tileZIndex, tileCoordinate.tileXIndex];
                int tileWidth = tileData.heightMap.GetLength(1);

                // Calculate mesh vertex index
                Vector3[] meshVertices = tileData.mesh.vertices;
                int vertexIndex = tileCoordinate.coordinateZIndex * tileWidth + tileCoordinate.coordinateXIndex;


                // Get Terrain type
                TerrainType terrainType = tileData.chosenHeightTerrainTypes[tileCoordinate.coordinateZIndex, tileCoordinate.coordinateXIndex];
                // If this is water, it shouldn't get it

                if (terrainType.name != "Water")
                {
                    float treeValue = treeMap[zIndex, xIndex];

                    int neighborZBegin = (int)Mathf.Max(0, zIndex - this.neighborRadius);
                    int neighborZEnd = (int)Mathf.Min(levelDepth - 1, zIndex + this.neighborRadius);
                    int neighborXBegin = (int)Mathf.Max(0, xIndex - this.neighborRadius);
                    int neighborXEnd = (int)Mathf.Min(levelWidth-1, xIndex + this.neighborRadius);

                    float maxValue = 0f;
                    for (int neighborZ = neighborZBegin; neighborZ <= neighborZEnd; neighborZ++)
                    {
                        for (int neighborX = neighborXBegin; neighborX <= neighborXEnd; neighborX++)
                        {
                            float neighborValue = treeMap[neighborZ, neighborX];
                            if (neighborValue > maxValue)
                            {
                                maxValue = neighborValue;
                            }
                        }
                    }

                    if (treeValue == maxValue)
                    {
                        Vector3 treePosition = new Vector3((xIndex)*distanceBetweenVertices, meshVertices[vertexIndex].y, (zIndex)*distanceBetweenVertices);
                        treePosition.x -= (widthOfTile / 2) + Random.Range(0, maxOffset);
                        treePosition.z -= (depthOfTile / 2) + Random.Range(0, maxOffset);
                        int index = (int)(Random.value * this.treePrefabs.Length);
                        GameObject tree = Instantiate(this.treePrefabs[index], treePosition, Quaternion.identity);
                        tree.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        Vector3 rotation = tree.transform.localEulerAngles;
                        rotation.y = Random.Range(0, 360);
                        tree.transform.localEulerAngles = rotation;
                    }
                }

            }
        }
    }
}
