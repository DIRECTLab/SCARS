using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LocationDataValues
{
    public float xLocation;
    public float yLocation;
    public Vector3 linearVelocity;

    public Vector3 rotationalVelocity;

    public float heading;
}

public class LocationData : MonoBehaviour, ISocketValue
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    [SerializeField]
    TwistController twistController;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public float GetHeading()
    {
        return transform.rotation.eulerAngles.y - startRotation.eulerAngles.y;
    }

    public Tuple<string, string, string> GetCurrentMessage()
    {
        Vector3 currentPosition = transform.position - startPosition;

        LocationDataValues dataValues = new LocationDataValues();

        dataValues.xLocation = currentPosition.x;
        dataValues.yLocation = currentPosition.z; // unity treats x and z as horizontal and y as vertical, so this is just translating
        dataValues.linearVelocity = Vector3.zero;
        dataValues.rotationalVelocity = Vector3.zero;
        float offsetFromStart = transform.rotation.eulerAngles.y - startRotation.eulerAngles.y;
        dataValues.heading = offsetFromStart;

        return Tuple.Create("Location", JsonUtility.ToJson(dataValues), "json-string");
    }
}
