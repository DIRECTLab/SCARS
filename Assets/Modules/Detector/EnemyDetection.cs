using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyDetect
{
    private static int idGiver = 0;

    public static int GetId()
    {
        return ++idGiver;
    }
}

public class EnemyDetection : MonoBehaviour
{
    private DangerDetector robot;

    public int myId { get; private set; }

    [SerializeField]
    public float maxDetectionDistance;

    [SerializeField]
    private LayerMask robotAndObstacleLayers;

    private Vector3 hitPoint;
    private Vector3 detectionRay;

    [Header("Debug")]
    [SerializeField]
    private bool showScanLaser;
    [SerializeField]
    private bool showHitPoint;

    void OnDrawGizmos()
    {
        if (showScanLaser)
        {
            Gizmos.DrawRay(transform.position, detectionRay);
        }
        if (showHitPoint)
        {
            Gizmos.DrawWireSphere(hitPoint, 0.25f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        myId = EnemyDetect.GetId();
        robot = GameObject.FindGameObjectWithTag("robot").GetComponentInChildren<DangerDetector>();

        if (!robot)
        {
            throw new System.Exception("An enemy couldn't find the robot, make sure it is placed and has the tag 'robot'");
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        detectionRay = (robot.transform.position - transform.position).normalized * maxDetectionDistance;

        if (Physics.Raycast(transform.position, detectionRay, out hit, maxDetectionDistance, robotAndObstacleLayers))
        {
            hitPoint = hit.point;
            if (hit.transform.CompareTag(robot.transform.tag))
            {
                robot.MarkDetected(this);
            }
            else
            {
                robot.MarkNotDetected(this);
            }
        }
        else
        {
            robot.MarkNotDetected(this);
        }
    }
}
