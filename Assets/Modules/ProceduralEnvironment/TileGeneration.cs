using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainType 
{
    public string name;
    public float height;
    public Color color;
}

public class TileData {
    public float[,] heightMap;
    public float[,] dangerMap;
    public TerrainType[,] chosenHeightTerrainTypes;
    public TerrainType[,] chosenDangerTerrainTypes;
    public Mesh mesh;

    public TileData(float[,] heightMap, float[,] dangerMap, TerrainType[,] chosenHeightTerrainTypes, TerrainType[,] chosenDangerTerrainTypes, Mesh mesh)
    {
        this.heightMap = heightMap;
        this.dangerMap = dangerMap;
        this.chosenHeightTerrainTypes = chosenHeightTerrainTypes;
        this.chosenDangerTerrainTypes = chosenDangerTerrainTypes;
        this.mesh = mesh;
    }
}

enum VisualizationMode { Height, Danger, Environment }

public class TileGeneration : MonoBehaviour
{

    [SerializeField]
    NoiseMap noiseMapGeneration;

    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private float mapScale;

    [SerializeField]
    private float heightMultiplier;

    [SerializeField]
    private AnimationCurve heightCurve;

    [SerializeField]
    private TerrainType[] heightTerrainTypes;

    [SerializeField]
    private Wave[] waves;

    [SerializeField]
    private TerrainType[] dangerTerrainTypes;

    [SerializeField]
    private AnimationCurve dangerCurve;

    [SerializeField]
    private Wave[] dangerWaves;

    [SerializeField]
    private VisualizationMode visualizationMode;

    [SerializeField]
    private Texture2D environmentTexture;

    public TileData tileData { get; private set; }


    private void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                float height = heightMap[zIndex, xIndex];

                Vector3 vertex = meshVertices[vertexIndex];

                meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);

                vertexIndex++;
            }
        }


        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

    public TileData GenerateTile(float seed)
    {
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        // Get offsets of this tile
        float offsetX = -this.gameObject.transform.position.x;
        float offsetZ = -this.gameObject.transform.position.z;

        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        // Calculate offsets based on tile position
        float[,] heightMap = this.noiseMapGeneration.GenerateNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, this.waves, seed);

        float[,] dangerMap = this.noiseMapGeneration.GenerateNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, this.dangerWaves, seed);
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                dangerMap[zIndex, xIndex] -= this.dangerCurve.Evaluate(heightMap[zIndex, xIndex]) * heightMap[zIndex, xIndex];
            }
        }

        // Generate heightMap using the noise
        TerrainType[,] chosenHeightTypes = new TerrainType[tileDepth, tileWidth];
        Texture2D heightTexture = BuildTexture(heightMap, this.heightTerrainTypes, chosenHeightTypes);
        UpdateMeshVertices(heightMap);

        // Generate danger map using the noise
        TerrainType[,] chosenDangerTypes = new TerrainType[tileDepth, tileWidth];
        Texture2D dangerTexture = BuildTexture(dangerMap, this.dangerTerrainTypes, chosenDangerTypes);

        switch (this.visualizationMode)
        {
            case VisualizationMode.Height:
                this.tileRenderer.material.mainTexture = heightTexture;
                break;
            case VisualizationMode.Danger:
                this.tileRenderer.material.mainTexture = dangerTexture;
                break;
            case VisualizationMode.Environment:
                this.tileRenderer.material.mainTexture = environmentTexture;
                break;
        }

        TileData tileData = new TileData(heightMap, dangerMap, chosenHeightTypes, chosenDangerTypes, this.meshFilter.mesh);

        this.tileData = tileData;

        return tileData;
    }

    private Texture2D BuildTexture(float[,] heightMap, TerrainType[] terrainTypes, TerrainType[,] chosenTypes)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];

                TerrainType terrainType = ChooseTerrainType(height, terrainTypes);

                chosenTypes[zIndex, xIndex] = terrainType;

                colorMap[colorIndex] = terrainType.color;
            }
        }

        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    private TerrainType ChooseTerrainType(float height, TerrainType[] terrainTypes) {
        foreach (TerrainType terrainType in terrainTypes)
        {
            if (height < terrainType.height)
            {
                return terrainType;
            }
        }
        return terrainTypes[terrainTypes.Length - 1];
    }
}
