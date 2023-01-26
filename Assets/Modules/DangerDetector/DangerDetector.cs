using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DangerPair
{
    public int id;
    public float amount;
}

[Serializable]
public class DangerData 
{
    public float amount;
    public DangerPair[] uniqueObservers;
    public float totalDistance;
}

public class DangerDetector : MonoBehaviour, ISocketValue
{

    private Dictionary<int, EnemyDetection> seenByEnemies = new Dictionary<int, EnemyDetection>();

    private Dictionary<int, float> uniqueEnemyCounts = new Dictionary<int, float>();

    private Vector3 lastPosition;

    [SerializeField]
    private DangerData dangerData;

    void Start()
    {
        lastPosition = transform.position;
        dangerData = new DangerData();
        dangerData.amount = 0;
        dangerData.uniqueObservers = new DangerPair[0];
    }

    public void ResetDanger()
    {
        uniqueEnemyCounts = new Dictionary<int, float>();
        seenByEnemies.Clear();
        lastPosition = transform.position;
        dangerData = new DangerData();
        dangerData.uniqueObservers = new DangerPair[0];
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;

        float distanceCoveredLastFrame = Vector3.Distance(lastPosition, currentPosition);

        foreach (int key in seenByEnemies.Keys)
        {
            // uniqueEnemyCounts[key] += distanceCoveredLastFrame;
        }
        
        dangerData.amount += distanceCoveredLastFrame * seenByEnemies.Keys.Count;
        dangerData.totalDistance += distanceCoveredLastFrame;
        lastPosition = currentPosition;
    }

    public void MarkDetected(EnemyDetection detectedBy)
    {
        seenByEnemies[detectedBy.myId] = detectedBy;
        if (!uniqueEnemyCounts.ContainsKey(detectedBy.myId))
        {
            uniqueEnemyCounts[detectedBy.myId] = 0.0f;
        }
    }

    public void MarkNotDetected(EnemyDetection detectedBy)
    {
        if (seenByEnemies.ContainsKey(detectedBy.myId))
        {
            seenByEnemies.Remove(detectedBy.myId);
        }
    }

    public Tuple<string, string, string> GetCurrentMessage()
    {
        DangerPair[] dangerPairs = new DangerPair[uniqueEnemyCounts.Keys.Count];
        int i = 0;
        foreach (int key in uniqueEnemyCounts.Keys)
        {
            DangerPair pair = new DangerPair();
            pair.id = key;
            pair.amount = uniqueEnemyCounts[key];
            dangerPairs[i] = pair;
            i++;
        }
        dangerData.uniqueObservers = dangerPairs;
        return Tuple.Create("Danger", JsonUtility.ToJson(dangerData), "json-string");
    }
}
