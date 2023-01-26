using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MapScan
{
    public bool[] scanResults;
    public float mapWidth;
    public float mapDepth;

    public int numXPoints;
    public int numYPoints;

    public float granularity;

}

public class MapScanner : MonoBehaviour, ISocketValue
{
    private LoSLevelGen levelGen; // used to get map size and starting location

    [Header("Config")]
    [SerializeField] float offsetHeight = 10f;
    [SerializeField] float metersBetweenPoints = 1f;

    [SerializeField] LayerMask obstacleLayer;

    [SerializeField] GameObject testObject;


    
    private MapScan mapScan;

    void Start()
    {

    	levelGen = GameObject.FindObjectOfType<LoSLevelGen>();
     	mapScan = new MapScan();


    }

    void OnDrawGizmosSelected()
    {
        Tuple<float, float, Vector3> mapData = levelGen.GetMapSize();
            int pointsX = (int)(mapScan.mapWidth / metersBetweenPoints);
            int pointsZ = (int)(mapScan.mapDepth / metersBetweenPoints);

            for (int x = 0; x < pointsX; x++)
            {
                for (int z = 0; z < pointsZ; z++)
                {
                    Vector3 scanLocation = new Vector3(x * metersBetweenPoints, offsetHeight, z * metersBetweenPoints);
                    if (mapScan.scanResults[(int)(x + z * mapScan.mapWidth)]){
                        Gizmos.color = Color.red;
                    }
                    else{
                        Gizmos.color = Color.green;
                    }

                    Gizmos.DrawRay(scanLocation, transform.TransformDirection(Vector3.down) * offsetHeight * 1.5f);
                }
            }
    }

    private int currentIteration = -1;

    public Tuple<string, string, string> GetCurrentMessage()
    {

        if (currentIteration != levelGen.currentIteration)
        {
            Tuple<float, float, Vector3> mapData = levelGen.GetMapSize();
            mapScan.mapWidth = mapData.Item1;
            mapScan.mapDepth = mapData.Item2;

            int pointsX = (int)(mapScan.mapWidth / metersBetweenPoints);
            mapScan.numXPoints = pointsX;
            int pointsZ = (int)(mapScan.mapDepth / metersBetweenPoints);
            mapScan.numYPoints = pointsZ;
            mapScan.granularity = metersBetweenPoints;

            mapScan.scanResults = new bool[pointsX * pointsZ];

            for (int x = 0; x < pointsX; x++)
            {
                for (int z = 0; z < pointsZ; z++)
                {
                    RaycastHit hit;
                    Vector3 scanLocation = new Vector3(x * metersBetweenPoints, offsetHeight, z * metersBetweenPoints);
                    mapScan.scanResults[(int)(x + z * mapScan.mapWidth)] = Physics.Raycast(scanLocation, transform.TransformDirection(Vector3.down), out hit, offsetHeight * 1.5f, obstacleLayer);
                }
            }

            currentIteration = levelGen.currentIteration;

            Debug.Log(JsonUtility.ToJson(mapScan));
        }

        return Tuple.Create("MapScan", JsonUtility.ToJson(mapScan), "json-string");
    }
}
