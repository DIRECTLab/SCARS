using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DangerScanner : MonoBehaviour
{
    private int[,] dangers;

    [SerializeField]
    private float metersBetweenPoints;

    [SerializeField]
    private string enemyTag;

    [SerializeField]
    private Vector3 originCorner;

    [SerializeField]
    private Vector3 farCorner;

    [SerializeField]
    private float floorCheckOffset;

    [SerializeField]
    private float displayOffset;

    [SerializeField]
    private LayerMask obstacleLayer;

    [SerializeField]
    private Gradient dangerGradient;

    [SerializeField]
    private float circleSize;

    private int maxDanger = 0;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 distance = farCorner - originCorner;

        int xCount = (int)(distance.x / metersBetweenPoints);
        int zCount = (int)(distance.z / metersBetweenPoints);

        dangers = new int[xCount, zCount];

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        for (int z = 0; z < zCount; z++)
        {
            for (int x = 0; x < xCount; x++)
            {
                Vector3 point = new Vector3(x * metersBetweenPoints, floorCheckOffset, z * metersBetweenPoints);


                int count = 0;
                foreach (GameObject enemy in enemies)
                {
                    Vector3 detectionRay = (enemy.transform.position - point).normalized;
                    EnemyDetection detector = enemy.GetComponent<EnemyDetection>();
                    RaycastHit hit;
                    float maxDetectionDistance = Math.Min(Vector3.Distance(point, enemy.transform.position), detector.maxDetectionDistance);
                    if (Vector3.Distance(point, enemy.transform.position) < detector.maxDetectionDistance && Physics.Raycast(enemy.transform.position, detectionRay, out hit, maxDetectionDistance, obstacleLayer)) // if there is no obstacles in the way
                    {
                        count++;
                    }
                }
                dangers[x, z] = count;

                if (count > maxDanger){
                    maxDanger = count;
                }
            }
        }
    }

    Color ColorFromGradient (int dangerAmount)
    {
        if (maxDanger == 0)
        {
            return dangerGradient.Evaluate(0);
        }
        return dangerGradient.Evaluate((float)dangerAmount / (float) maxDanger);
    }

    void OnDrawGizmosSelected()
    {
        if (dangers != null)
        {
            for (int x = 0; x < dangers.GetLength(0); x++)
            {
                for (int z = 0; z < dangers.GetLength(1); z++)
                {
                    int dangerAmount = dangers[x, z];
                    Gizmos.color = ColorFromGradient(dangerAmount);
                    Gizmos.DrawSphere(new Vector3(originCorner.x + x * metersBetweenPoints, displayOffset, originCorner.z + z * metersBetweenPoints), circleSize);
                }
            }
        }
    }
}
