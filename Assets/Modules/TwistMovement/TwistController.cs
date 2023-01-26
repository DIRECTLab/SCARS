using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// Unity uses xz as the horizontal place, and y as vertical, but I am translating it so xy is horizontal and z is vertical for planner
[Serializable]
public class TwistMessage
{
    public Vector3 LinearMovement;
    public Vector3 RotationalMovement;

}

[RequireComponent(typeof(Rigidbody))]
public class TwistController : MonoBehaviour, ISocketSubscriber
{


    public Vector3 LinearMovement { get; private set; }
    public Vector3 RotationalMovement { get; private set; }

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        LinearMovement = new Vector3(0f, 0f, 0f);
        RotationalMovement = new Vector3(0f, 0f, 0f);
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector3 relative = transform.TransformDirection(LinearMovement.y, LinearMovement.z, LinearMovement.x);
        Debug.Log(rb.position);
        rb.velocity = new Vector3(relative.x, 0, relative.z);
        rb.angularVelocity = new Vector3(RotationalMovement.y, RotationalMovement.z, RotationalMovement.x);
    }

    public void HandleSocketMessage(WebSocketMessage message)
    {
        TwistMessage currentMessage = JsonUtility.FromJson<TwistMessage>(message.data);
        LinearMovement = currentMessage.LinearMovement;
        RotationalMovement = currentMessage.RotationalMovement;
    }
    public string[] GetRequestedSubscriptions()
    {
        return new string[]{"TwistMessage"};
    }
}
