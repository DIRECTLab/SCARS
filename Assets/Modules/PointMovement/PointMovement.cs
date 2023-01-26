using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Position
{
    public Vector3 point;
}

/**
    Allows you to move the robot by telling it a 'goto' position as well as an orientation
*/
public class PointMovement : MonoBehaviour, ISocketSubscriber
{

    [SerializeField]
    private float turnSpeed;

    [SerializeField]
    private float rotationTolerance;

    [SerializeField]
    private float walkSpeed;

    [SerializeField]
    private float positionTolerance;

    [SerializeField]
    private LocationData locationData;

    [SerializeField]
    private bool turnAndWalkSameTime = true;

    [SerializeField]
    private float minDistanceToObstacle = 0.10f;

    [SerializeField]
    private LayerMask obstacleLayer;

    private Rigidbody rb;

    public Vector3 goalPosition;

    private float yOffset;

    private bool shouldTurn = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        goalPosition = transform.position;
        yOffset = transform.position.y;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(goalPosition, Vector3.up * 2);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPos = new Vector3(transform.position.x, yOffset, transform.position.z);
        if (Vector3.Distance(currentPos, goalPosition) > positionTolerance)
        {
            Vector3 targetDir = goalPosition - currentPos;
            float angle = Vector3.Angle(targetDir, transform.forward);
            if (angle > rotationTolerance)
            {
                shouldTurn = true;
            }
            if (shouldTurn)
            {
                rb.velocity = Vector3.zero;
                Vector3 cross = Vector3.Cross(transform.forward, targetDir);

                float dotProduct = Vector3.Dot(cross, transform.up);

                Quaternion deltaRotate = Quaternion.Euler(Vector3.up * turnSpeed * (dotProduct > 0 ? 1 : -1) * Time.deltaTime);
                rb.MoveRotation(rb.rotation * deltaRotate);
            }
            if ((!shouldTurn || turnAndWalkSameTime) && angle < 15f)
            {
                Vector3 direction = transform.forward;
                direction.y = 0;
                if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, minDistanceToObstacle, obstacleLayer))
                {
                    rb.velocity = direction * walkSpeed;
                }
                else
                {
                    rb.velocity = Vector3.zero;
                }

            }
            else
            {
                rb.velocity = Vector3.zero;
            }
            if (shouldTurn && angle < rotationTolerance * 0.8f)
            {
                shouldTurn = false;
            }
        }
        else
        {
            rb.velocity = Vector3.zero;
            shouldTurn = false;
        }
    }

    public void HandleSocketMessage(WebSocketMessage message)
    {
        Position currentMessage = JsonUtility.FromJson<Position>(message.data);
        goalPosition = new Vector3(currentMessage.point.x, yOffset, currentMessage.point.y);
        Debug.Log("Recieved a goal position: " + goalPosition);
    }
    public string[] GetRequestedSubscriptions()
    {
        return new string[]{"PointMovement"};
    }
}
