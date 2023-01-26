using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LidarData
{
    public float angleMin; // start angle of scan [radians]
    public float angleMax; // end angle of the scan [radians]
    public float angleIncrement; // angular distance between measurements [radians]
    public float timeIncrement; // time between measurements [seconds] (used with a moving scanner to interpolate position of 3d points)
    public float rangeMin; // minimum range value [meters]
    public float rangeMax; // maximum range value [meters]

    public float scanTime; // time between scans [seconds]
    public float[] ranges; // any value not in range (rangeMin < value < rangeMax) is marked as -1

}

// Lidar values are based on LaserScane.msg from ROS. Intensities are not included since that is hard to simulate

// Angles are currently in radians
public class LidarScript : MonoBehaviour, ISocketValue
{

    [Header("Layer Info")]
    public LayerMask layersSeenByLidar;
    
    [Header("Configuration")]

    public float angleMin; // start angle of scan [radians]
    public float angleMax; // end angle of the scan [radians]
    public float angleIncrement; // angular distance between measurements [radians]
    public float timeIncrement; // time between measurements [seconds] (used with a moving scanner to interpolate position of 3d points)
    public float rangeMin; // minimum range value [meters]
    public float rangeMax; // maximum range value [meters]

    public float scanTime; // time between scans [seconds]

    private float currentScanTime = 0;

    private int currentIndex = 0;


    // Output values
    public float[] ranges { get; private set; } // any value not in range (rangeMin < value < rangeMax) is marked as -1


    [Header("Debug")]
    public bool showLidar = true;
    private bool hitSuccess = false;

    [SerializeField]
    private bool showLidarHitPoints = true;
    private float currentDistance = 10;

    private Vector3[] hitPoints;

    // Start is called before the first frame update
    void Start()
    {
        int numOfReadings = (int)((rangeMax - rangeMin) / angleIncrement);
        ranges = new float[numOfReadings];
        hitPoints = new Vector3[numOfReadings];
        for (int i = 0; i < ranges.Length; i++)
        {
            ranges[i] = -1; // set all to -1 to start
        }
    }

    void OnDrawGizmos()
    {
        if (hitSuccess)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        if (showLidar)
        {
            Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * currentDistance);
        }

        if (showLidarHitPoints)
        {
            foreach (Vector3 hitPoint in hitPoints)
            {
                if (hitPoint != Vector3.zero)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(hitPoint, Vector3.up);
                }
            }
        }
    }

    void FixedUpdate()
    {
        currentScanTime += Time.deltaTime;

        while (currentScanTime > timeIncrement && timeIncrement != 0) // skip this if the increment is 0 or else this crashes :(
        {
            // Rotate this object so that it's forward is where it should be for the measurement
            Vector3 currentRotation = transform.localEulerAngles;
            currentRotation.y = currentIndex * Mathf.Rad2Deg * angleIncrement;
            transform.localEulerAngles = currentRotation;
            RaycastHit hit;
            // We hit something
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, rangeMax, layersSeenByLidar))
            {
                ranges[currentIndex] = hit.distance;
                hitPoints[currentIndex] = hit.point;
                currentDistance = hit.distance;

            }
            // Exceeded max distance or didn't hit anything
            else
            {
                hitPoints[currentIndex] = Vector3.zero;
                ranges[currentIndex] = -1;
                currentDistance = rangeMax;
            }
            currentIndex += 1;
            currentIndex %= ranges.Length;
            currentScanTime -= timeIncrement;
        }
    }

    // Returns LiDAR data according to 
    public Tuple<string, string, string> GetCurrentMessage()
    {
        LidarData lidarData = new LidarData()
        {
            angleMin = this.angleMin,
            angleMax = this.angleMax,
            angleIncrement = this.angleIncrement,
            timeIncrement = this.timeIncrement,
            rangeMin = this.rangeMin,
            rangeMax = this.rangeMax,
            scanTime = this.scanTime,
            ranges = this.ranges,
        };

        string data = JsonUtility.ToJson(lidarData);

        return Tuple.Create("Lidar", data, "json-string");
    }
}
