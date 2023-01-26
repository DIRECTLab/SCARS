using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CollisionMessage
{
    public bool atGoal;
    public bool inObstacle;
}

public class CollisionDetection : MonoBehaviour, ISocketValue
{
    private CollisionMessage collisionMessage = new CollisionMessage();

    private int collisionAmount = 0;
    
    public Tuple<string, string, string> GetCurrentMessage()
    {
        return Tuple.Create("Collision", JsonUtility.ToJson(collisionMessage), "json-string");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            collisionMessage.atGoal = true;
        }

        if (other.CompareTag("Obstacle"))
        {
            collisionMessage.inObstacle = true;
            collisionAmount++;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            collisionAmount--;
            if (collisionAmount == 0)
            {
                collisionMessage.inObstacle = false;
            }
        }
    }

}
