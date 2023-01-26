using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimRobot : MonoBehaviour, ISocketValue
{
    public TextAsset[] jsonFiles; // this should be LoS, Proximity, Distance (THE ORDER IS IMPORTANT)

    public float robotSpeed;
    private Rigidbody rb;
    private Vector3[] points;

    private Vector3 startPosition;

    private int currentGoal = 0;

    private bool completed = false;

    public DangerDetector dangerDetector;
    public EnemySpawner spawner;

    private int currentFile = 0;
    public int currentIteration = 1; // the sim number we're on
    private int maxIterations = 101;
    private int currentObserver = 0;
    private int[] observerCounts = new int[5]
    {
        2, 4, 6, 8, 10
    };

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawner.SpawnEnemies(currentIteration, observerCounts[currentObserver]);
        startPosition = rb.position;
        Path path = JsonUtility.FromJson<Path>(jsonFiles[0].text);
        List<Vector3> paths = new List<Vector3>();

        HashSet<string> alreadyExists = new HashSet<string>();

        foreach (string item in path.traverseLocations)
        {
            if (!alreadyExists.Contains(item))
            {
                PathItem pathItem = JsonUtility.FromJson<PathItem>(item);
                alreadyExists.Add(item);
                paths.Add(new Vector3(pathItem.point.x, 0.1f, pathItem.point.y));
            }
        }

        Vector3[] linePoints = paths.ToArray();

        points = linePoints;
    }

    IEnumerator RestartAfterTimer()
    {
        yield return new WaitForSeconds(2.0f);

        if (currentIteration >= maxIterations) // done with this iteration, next number of things
        {
            currentIteration = 0;
            currentObserver++;
        }

        if (currentObserver >= observerCounts.Length) // done with this cost function, now we should go to new file
        {
            currentObserver = 0;
            currentFile++;
            if (currentFile >= jsonFiles.Length)
            {
                Destroy(gameObject); // we're done
            }
            else
            {
                Path path = JsonUtility.FromJson<Path>(jsonFiles[currentFile].text);
                List<Vector3> paths = new List<Vector3>();

                HashSet<string> alreadyExists = new HashSet<string>();

                foreach (string item in path.traverseLocations)
                {
                    if (!alreadyExists.Contains(item))
                    {
                        PathItem pathItem = JsonUtility.FromJson<PathItem>(item);
                        alreadyExists.Add(item);
                        paths.Add(new Vector3(pathItem.point.x, 0.1f, pathItem.point.y));
                    }
                }

                Vector3[] linePoints = paths.ToArray();

                points = linePoints;
            }

        }

        currentIteration++;
        rb.isKinematic = true;
        rb.detectCollisions = false;
        transform.position = startPosition;
        dangerDetector.ResetDanger();
        spawner.SpawnEnemies(currentIteration, observerCounts[currentObserver]);
        currentGoal = 0;
        completed = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

    void Update()
    {
        if (currentGoal >= points.Length - 1)
        {
            if (!completed)
            {

                StartCoroutine(RestartAfterTimer());
                completed = true;
            }
        }
        else
        {
            if (Vector3.Distance(rb.transform.position, points[currentGoal]) < 0.5f)
            {
                currentGoal++;
            }

            Vector3 direction = (points[currentGoal] - transform.position);
            direction.Normalize();

            rb.MovePosition(transform.position + direction * Time.deltaTime * robotSpeed);
        }
    }

    public Tuple<string, string, string> GetCurrentMessage()
    {
        if (completed){
            return Tuple.Create("Completed", "true", "boolean");
        }
        else{
            return Tuple.Create("Completed", "false", "boolean");
        }
    }
}
