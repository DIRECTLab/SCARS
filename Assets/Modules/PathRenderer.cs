using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PathItem
{
    public Vector3 point;
}

[Serializable]
public class Path
{
    public string[] traverseLocations;
}

public class PathRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public TextAsset jsonFile;

    public float secondsBetweenPointAdd;

    private float currentTime = 0;
    private int currentCount = 1;

    private bool shouldUpdate = true;

    private Vector3[] points;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Path path = JsonUtility.FromJson<Path>(jsonFile.text);
        List<Vector3> paths = new List<Vector3>();

        HashSet<string> alreadyExists = new HashSet<string>();

        foreach (string item in path.traverseLocations)
        {
            Debug.Log(item);
            if (!alreadyExists.Contains(item))
            {
                PathItem pathItem = JsonUtility.FromJson<PathItem>(item);
                Debug.Log(pathItem.point);
                alreadyExists.Add(item);
                paths.Add(new Vector3(pathItem.point.x, 0.1f, pathItem.point.y));
            }
        }

        Vector3[] linePoints = paths.ToArray();

        points = linePoints;
        
        Debug.Log(linePoints);

        lineRenderer.positionCount = linePoints.Length;
        lineRenderer.SetPositions(linePoints);
    }

    void Update()
    {
        currentTime += Time.deltaTime;

        while (currentTime > secondsBetweenPointAdd && secondsBetweenPointAdd > 0)
        {
            currentTime -= secondsBetweenPointAdd;
            currentCount++;
        }

        if (currentCount > points.Length)
        {
            currentCount = points.Length;
        }

        if (shouldUpdate)
        {
            Vector3[] currentPoints = new Vector3[currentCount];
            Array.Copy(points, 0, currentPoints, 0, currentCount);

            lineRenderer.positionCount = currentCount;
            lineRenderer.SetPositions(currentPoints);

            if (currentCount == points.Length)
            {
                shouldUpdate = false;
            }
        }
    }
}
