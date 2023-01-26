using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using System;

public interface ISocketValue
{
    // This should return the name of the attribute, and the JSON respresentation of it's data, as well as the format
    public Tuple<string, string, string> GetCurrentMessage();
}

// This is all the game objects that need a subscription to the server
public interface ISocketSubscriber
{
    public string[] GetRequestedSubscriptions();

    public void HandleSocketMessage(WebSocketMessage msg);
}

[Serializable]
public class WebSocketMessage
{
    public string purpose;
    public string type;
    public string data;
    public string format;

    public override string ToString(){
        return purpose + " " + type + " " + data + " " + format;
    }
}

public class WebsocketWorker : MonoBehaviour
{
    private WebSocket ws;

    [SerializeField]
    private GameObject[] socketMemberGameobjects; // These should have an ISocketValue component on it
    private List<ISocketValue> socketValueList = new List<ISocketValue>();

    [SerializeField]
    private GameObject[] socketSubscriberGameobjects; // These should have an ISocketSubscriber component on it
    private Dictionary<string, List<ISocketSubscriber>> socketSubscribers = new Dictionary<string, List<ISocketSubscriber>>();

    private void Start()
    {
        ws = new WebSocket("ws://localhost:8080");
        ws.Connect();
        ws.OnMessage += OnMessageReceived;


        foreach (GameObject obj in socketMemberGameobjects)
        {
            ISocketValue[] socketValues = obj.GetComponents<ISocketValue>();
            foreach (ISocketValue socketValue in socketValues)
            {
                socketValueList.Add(socketValue);
            }
            if (socketValues.Length == 0)
            {
                Debug.LogWarning(obj.name + " was put in the socket's value list, but does not contain an ISocketValue component. It should have one added, or be removed from the list");
            }
        }

        foreach (GameObject obj in socketSubscriberGameobjects)
        {
            ISocketSubscriber[] socketSubscriberGameObjects = obj.GetComponents<ISocketSubscriber>();
            foreach (ISocketSubscriber socketSubscriber in socketSubscriberGameObjects)
            {
                string[] requestedSubscriptions = socketSubscriber.GetRequestedSubscriptions();

                foreach (string subscription in requestedSubscriptions)
                {
                    if (!socketSubscribers.ContainsKey(subscription))
                    {
                        socketSubscribers[subscription] = new List<ISocketSubscriber>();
                    }
                    socketSubscribers[subscription].Add(socketSubscriber);
                    WebSocketMessage webSocketMessage = new WebSocketMessage()
                {
                    purpose = "SUBSCRIBE",
                    type = subscription,
                };
                string jsonMessage = JsonUtility.ToJson(webSocketMessage);
                ws.Send(jsonMessage);
                }
            }
            
            if (socketSubscriberGameObjects.Length == 0)
            {
                Debug.LogWarning(obj.name + " was put in the socket's subscriber list, but does not contain an ISocketSubscriber component. It should have one added, or be removed from the list");
            }
        }

    }

    private void Update()
    {
        if (ws == null)
        {
            return;
        }

        foreach (ISocketValue socketValue in socketValueList)
        {
            Tuple<string, string, string> message = socketValue.GetCurrentMessage();
            WebSocketMessage webSocketMessage = new WebSocketMessage()
            {
                purpose = "PUSH",
                type = message.Item1,
                data = message.Item2,
                format = message.Item3,
            };
            string jsonMessage = JsonUtility.ToJson(webSocketMessage);
            ws.Send(jsonMessage);
        }
    }

    private void OnMessageReceived(object s, MessageEventArgs e)
    {
        WebSocket sender = (WebSocket)s;

        WebSocketMessage message = JsonUtility.FromJson<WebSocketMessage>(e.Data);

        if (message == null)
        {
            Debug.LogWarning("Received a message in an invalid format");
        }

        if (socketSubscribers.ContainsKey(message.type))
        {
            foreach (ISocketSubscriber subscriber in socketSubscribers[message.type])
            {
                subscriber.HandleSocketMessage(message);
            }
        }
    }
}
