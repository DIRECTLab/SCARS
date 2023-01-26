using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Obstacle
{
    public Vector3 bottomLeft;
    public Vector3 bottomRight;
    public Vector3 topLeft;
    public Vector3 topRight;
}

[System.Serializable]
public class ObstacleGroup
{
    public Obstacle[] obstacles;
}

public class ObstacleMarking : MonoBehaviour, ISocketSubscriber
{
    [SerializeField]
    private bool showObstacles = true;

    private bool drawWireMesh = false; // set true to do hollow cube instead

    private Obstacle[] obstacles;

    private bool initialized = false;

    void Awake()
    {
        obstacles = new Obstacle[0];
        initialized = true;
    }



    void OnDrawGizmos()
    {
        if (showObstacles && initialized)
        {
            foreach (Obstacle obstacle in obstacles)
            {
                Gizmos.color = Color.red;

                
                Vector3[] vertices = {new Vector3(obstacle.bottomLeft.x, 2, obstacle.bottomLeft.y), new Vector3(obstacle.bottomRight.x, 2, obstacle.bottomRight.y), new Vector3(obstacle.topLeft.x, 2, obstacle.topLeft.y), new Vector3(obstacle.topRight.x, 2, obstacle.topRight.y)};
                
                Mesh mesh = new Mesh();

                mesh.vertices = vertices;
                int[] tris = new int[6]{
                    0, 2, 1, 2, 3, 1
                };

                mesh.triangles = tris;

                Vector3[] normals = new Vector3[4]
                {
                    -Vector3.forward,
                    -Vector3.forward,
                    -Vector3.forward,
                    -Vector3.forward,
                };

                mesh.normals = normals;

                Vector2[] uv = new Vector2[4]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                mesh.uv = uv;

                Gizmos.DrawMesh(mesh);
            }
        }
    }

    public void HandleSocketMessage(WebSocketMessage message)
    {
        ObstacleGroup currentMessage = JsonUtility.FromJson<ObstacleGroup>(message.data);
        obstacles = currentMessage.obstacles;
    }

    public string [] GetRequestedSubscriptions()
    {
        return new string[]{"ObstacleDebug"};
    }

}
