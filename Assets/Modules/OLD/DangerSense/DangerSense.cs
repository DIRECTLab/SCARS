using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DangerMessage
{
    public float dangerAmount;
}

public class DangerSense : MonoBehaviour, ISocketValue
{
    [SerializeField]
    private LayerMask groundPlaneLayer; // the ground plane should be put in it's own layer so it can raycast to see where it is at (don't love this solution though...)

    [SerializeField]
    private float maxRayCastDistance = 100;

    [Header("Debug")]
    [SerializeField]
    private bool showChosenVertex;

    [SerializeField]
    private bool showRayCast;

    [SerializeField]
    private bool showHitPoint;

    private Vector3 currentVertex;
    private Vector3 hitPoint;

    private void OnDrawGizmos()
    {
        if (showChosenVertex)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentVertex, 0.25f);
        }

        if (showRayCast)
        {
            Gizmos.DrawRay(transform.position, transform.up * -maxRayCastDistance);
        }

        if (showHitPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hitPoint, 0.1f);
        }
    }

    public Tuple<string, string, string> GetCurrentMessage()
    {
        DangerMessage message = new DangerMessage();
        if (Physics.Raycast(transform.position, transform.up * -maxRayCastDistance, out RaycastHit hit))
        {
            hitPoint = hit.point;
            // We need to get the closest vertex of the collided plane
            MeshFilter mesh = hit.transform.gameObject.GetComponent<MeshFilter>();
            if (!mesh)
            {
                throw new Exception("Anything in the danger layer should have a mesh");
            }
            TileGeneration tileGen = hit.transform.gameObject.GetComponent<TileGeneration>();
            if (!tileGen)
            {
                throw new Exception("Anything in the danger layer should have a TileGeneration component (e.g. this should be a tile)");
            }

            Vector3 intersectionPoint = hit.point;
            Vector3[] vertices = mesh.mesh.vertices;

            Vector3 chosenVertex = vertices[0];
            float distanceToVertex = (vertices[0] - intersectionPoint).sqrMagnitude;
            int vertexIndex = 0;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = hit.transform.TransformPoint(vertices[i]);
                float distanceToCurrent = (v - intersectionPoint).sqrMagnitude;
                if (distanceToCurrent < distanceToVertex)
                {
                    chosenVertex = v;
                    distanceToVertex = distanceToCurrent;
                    vertexIndex = i;
                }
            }

            currentVertex = chosenVertex;

            // We need to translate this vertex back into a (z,x) coordinate to get it's danger
            int tileDepth = (int)Mathf.Sqrt(vertices.Length);

            int zCoordinate = vertexIndex / tileDepth;
            int xCooridnate = vertexIndex % tileDepth;

            message.dangerAmount = tileGen.tileData.dangerMap[zCoordinate, xCooridnate];

        }

        return Tuple.Create("dangerAmount", JsonUtility.ToJson(message), "json-string");
    }
}
