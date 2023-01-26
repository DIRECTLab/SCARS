using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LoSLevelGen : MonoBehaviour
{

    [Header("Prefabs")]

    [SerializeField]
    private GameObject robotPrefab; // prefab of the robot to spawn

    [SerializeField]
    private GameObject enemyDetectorPrefab; // prefab to spawn for the 'enemy'

    [SerializeField]
    private GameObject gridSectionPrefab; // the plane that will be generated for the map size

    [SerializeField]
    private GameObject[] obstaclePrefabs; // list of objects this will spawn as generated objects

    [Header("Enemy Settings")]

    [SerializeField]
    private int numberOfDetectors; // the # of 'enemies' on the map

    [Header("Obstacle Settings")]

    [SerializeField]
    private float placementThreshold; // if the given part of the noise is greater than this, it will be placed (if not touching neighbor)

    [SerializeField]
    private float chanceOfPlacementGivenValid; // the chance this object will be placed, even if the threshold is met. This is to keep from having solid blocks of obstacle

    [SerializeField]
    private float minDistanceBetweenObstacles; // the minimum viable distance between obstacles. This is needed for the loop that actually places them

    [SerializeField]
    private float maxOffsetFromPosition; // max offset an object can have from it's chosen position (this can help it not look like a grid)

    [Header("Map Generation Settings")]

    [SerializeField]
    private int mapGridSizeX; // the # of planes that should be created

    [SerializeField]
    private int mapGridSizeZ;

    [SerializeField]
    private float mapSeed;

    [SerializeField]
    private bool useRandomSeed; // set to true for random generation each time

    [SerializeField]
    private NoiseMap noiseGenerationMap; // the noise generator for this

    [SerializeField]
    private Wave[] noiseWaves;

    [SerializeField]
    private float mapSize; // changes the zoom on the perlin noise

    [SerializeField]
    private LayerMask obstacleLayer;

    [SerializeField]
    private float safeRadius; // radius which enemies cannot spawn by robot

    [SerializeField]
    private LayerMask enemyLayer;

    [SerializeField]
    private GameObject goalPrefab;

    public int currentIteration { get; private set; } // used by the map scanner

    private List<GameObject> generatedObjects = new List<GameObject>(); // list of generated objects so we can do cleanup between rounds
    // Start is called before the first frame update
    void Start()
    {
        SetupEnvironment();
        currentIteration = 0;
    }

    private void ResetEnvironment()
    {
        currentIteration++;
        while (this.generatedObjects.Count > 0)
        {
            GameObject toRemove = this.generatedObjects[0];
            this.generatedObjects.RemoveAt(0);
            Destroy(toRemove);
        }

        if (useRandomSeed)
        {
            this.mapSeed = UnityEngine.Random.Range((float.MinValue / 2), (float.MaxValue / 2));
        }
        else
        {
            UnityEngine.Random.InitState((int)(mapSeed));
        }
    }
    // Removes all generated objects and resets the simulation
    public void SetupEnvironment()
    {
        ResetEnvironment();
        GenerateTerrain();
        GenerateEnemies();
        GenerateRobot();

        GenerateGoal();
    }

    /**
    Returns the size of the map (width, depth, origin)
    */
    public Tuple<float, float, Vector3> GetMapSize()
    {
        Vector3 tileSize = this.gridSectionPrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;
        Vector3 farCorner = new Vector3(this.gameObject.transform.position.x + (mapGridSizeX) * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + (mapGridSizeZ) * tileDepth);

        return Tuple.Create((float)(mapGridSizeX * tileWidth), (float)(mapGridSizeZ * tileDepth), transform.position);
        
    }

    private void GenerateGoal()
    {
        Vector3 tileSize = this.gridSectionPrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;
        Vector3 farCorner = new Vector3(this.gameObject.transform.position.x + (mapGridSizeX - 1) * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + (mapGridSizeZ - 1) * tileDepth);

        RaycastHit[] hits = Physics.SphereCastAll(farCorner, 1, Vector3.down, 2, obstacleLayer, QueryTriggerInteraction.Ignore);

        Debug.Log("Hit " + hits.Length + " objects");

        foreach (RaycastHit hit in hits) 
        {
            Debug.Log(hit.transform.name);
            generatedObjects.Remove(hit.transform.gameObject);
            Destroy(hit.transform.gameObject);
        }

        RaycastHit[] enemyHits = Physics.SphereCastAll(farCorner, safeRadius, Vector3.down, 2, enemyLayer, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in enemyHits)
        {
            Debug.Log(hit.transform.name);
            generatedObjects.Remove(hit.transform.gameObject);
            Destroy(hit.transform.gameObject);
        }

        GameObject createdGoal = Instantiate(goalPrefab, farCorner, Quaternion.identity);
        generatedObjects.Add(createdGoal);

    }

    private void GenerateRobot()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1, Vector3.down, 2, obstacleLayer, QueryTriggerInteraction.Ignore);

        Debug.Log("Hit " + hits.Length + " objects");

        foreach (RaycastHit hit in hits) 
        {
            Debug.Log(hit.transform.name);
            generatedObjects.Remove(hit.transform.gameObject);
            Destroy(hit.transform.gameObject);
        }

        RaycastHit[] enemyHits = Physics.SphereCastAll(transform.position, safeRadius, Vector3.down, 2, enemyLayer, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in enemyHits) 
        {
            Debug.Log(hit.transform.name);
            generatedObjects.Remove(hit.transform.gameObject);
            Destroy(hit.transform.gameObject);
        }



        GameObject robot = Instantiate(robotPrefab, new Vector3(transform.position.x, transform.position.y + 0.4150001f, transform.position.z), Quaternion.identity);
        generatedObjects.Add(robot);
    }

    private void GenerateEnemies()
    {
        Vector3 tileSize = this.gridSectionPrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;
        Vector3 farCorner = new Vector3(this.gameObject.transform.position.x + mapGridSizeX * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + mapGridSizeZ * tileDepth);

        for (int i = 0; i < numberOfDetectors; i++)
        {
            Vector3 placementPosition = new Vector3(UnityEngine.Random.Range(transform.position.x, farCorner.x), transform.position.y, UnityEngine.Random.Range(transform.position.z, farCorner.z));

            GameObject detector = Instantiate(enemyDetectorPrefab, placementPosition, Quaternion.identity);

            float lookDirection = UnityEngine.Random.Range(0, 360);

            Vector3 lookDirectionEuler = detector.transform.eulerAngles;
            lookDirectionEuler.y = lookDirection;
            detector.transform.eulerAngles = lookDirectionEuler;
            generatedObjects.Add(detector);
        }

    }

    private void GenerateTerrain()
    {
        Vector3 tileSize = this.gridSectionPrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        for (int xTileIndex = 0; xTileIndex < mapGridSizeX; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < mapGridSizeZ; zTileIndex++)
            {
                Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + xTileIndex * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + zTileIndex * tileDepth);

                GameObject tile = Instantiate(gridSectionPrefab, tilePosition, Quaternion.identity);
                generatedObjects.Add(tile);
            }
        }

        Vector3 farCorner = new Vector3(this.gameObject.transform.position.x + (mapGridSizeX - 1) * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + (mapGridSizeZ - 1) * tileDepth);

        int potentialObstaclesX = (int)((farCorner.x - transform.position.x) / minDistanceBetweenObstacles); // this tells us that there is potentially n number of obstacles for each row
        int potentialObstalcesZ = (int)((farCorner.z - transform.position.z) / minDistanceBetweenObstacles); // this tells us that there is potentially n number of rows

        float[,] densityMap = this.noiseGenerationMap.GenerateNoiseMap(potentialObstalcesZ, potentialObstaclesX, this.mapSize, this.mapSeed, this.mapSeed, this.noiseWaves, this.mapSeed);

        for (int x = 0; x < potentialObstaclesX; x++)
        {
            for (int z = 0; z < potentialObstalcesZ; z++)
            {
                if (densityMap[z, x] > placementThreshold)
                {
                    float secondCheck = UnityEngine.Random.value;
                    if (secondCheck < placementThreshold)
                    {
                        Vector3 placementPosition = new Vector3((transform.position.x + x * minDistanceBetweenObstacles) + UnityEngine.Random.Range(-maxOffsetFromPosition, maxOffsetFromPosition), transform.position.y, (transform.position.z + z * minDistanceBetweenObstacles) + UnityEngine.Random.Range(-maxOffsetFromPosition, maxOffsetFromPosition));
                        int index = (int)(UnityEngine.Random.value * this.obstaclePrefabs.Length);
                        GameObject obstacle = Instantiate(this.obstaclePrefabs[index], placementPosition, Quaternion.identity);

                        generatedObjects.Add(obstacle);
                    }
                }
            }
        }
    }
}
