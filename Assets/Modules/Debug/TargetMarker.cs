using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TargetNodes
{
    public Vector3[] Locations;
}

public class TargetMarker : MonoBehaviour, ISocketSubscriber
{

    [SerializeField]
    private bool showLidarDebug = true;

    Vector3[] locations = new Vector3[0];

    Vector3[] lidarLocations = new Vector3[0];
    Vector3[] predictedLocation = new Vector3[0];
    
    Color[] colors = { Color.red, Color.blue, Color.green, Color.magenta, Color.cyan, Color.yellow };

    void OnDrawGizmos()
    {
        int i = 0;
        foreach (Vector3 location in locations)
        {
            Gizmos.color = colors[i % colors.Length];
            i++;
            // Gizmos.DrawRay(new Vector3(location.x, 0, location.y), Vector3.up * (i + 1) * 0.5f);
            Gizmos.DrawSphere(new Vector3(location.x, 0, location.y), .25f);

        }

        if (locations.Length >= 2){
            Vector3 target = locations[locations.Length - 2];
            Vector3 current = locations[locations.Length - 1];
            Gizmos.color = Color.black;
            Gizmos.DrawLine(new Vector3(current.x, 1, current.y), new Vector3(target.x, 1, target.y));
        }

        if (showLidarDebug)
        {
            foreach(Vector3 lidarPoint in lidarLocations)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(new Vector3(lidarPoint.x, 0, lidarPoint.y), Vector3.up);
            }
        }
	if (predictedLocation.Length > 0) 
	{
        Gizmos.color = Color.red;
	    Gizmos.DrawRay(new Vector3(predictedLocation[0].x, 0, predictedLocation[0].y), Vector3.up * 4);
	}
    }

    public void HandleSocketMessage(WebSocketMessage message)
    {
        TargetNodes currentMessage = JsonUtility.FromJson<TargetNodes>(message.data);
        if (message.type == "TargetLocations")
        {
            Debug.Log(message.data);
            locations = currentMessage.Locations;
        }
        else if (message.type == "LidarDebug")
        {
            lidarLocations = currentMessage.Locations;
        }
        else if (message.type == "PredictionDebug") 
	{
	    predictedLocation = currentMessage.Locations;
	}
    }

    public string [] GetRequestedSubscriptions()
    {
        return new string[]{"TargetLocations", "LidarDebug", "PredictionDebug"};
    }
}
