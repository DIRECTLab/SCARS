using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public class TestMessage
{
    public string Scene;
}

public class TestRunner : MonoBehaviour, ISocketSubscriber
{
    public string[] GetRequestedSubscriptions()
    {
        return new string[] { "TestRunner" };
    }

    public void HandleSocketMessage(WebSocketMessage message)
    {
        TestMessage currentMessage = JsonUtility.FromJson<TestMessage>(message.data);
        SceneManager.LoadScene(currentMessage.Scene);
    }
}
