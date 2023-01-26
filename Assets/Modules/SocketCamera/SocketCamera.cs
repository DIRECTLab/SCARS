using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class SocketCameraMessage
{
    public byte[] pixels;
    public string base64;
}

[RequireComponent(typeof(Camera))]
public class SocketCamera : MonoBehaviour, ISocketValue
{
    private Camera cameraComponent;
    public byte[] CameraFeed { get; private set; }

    private RenderTexture renderTexture;

    [Header("Resolution")]
    public int height;
    public int width;

    private SocketCameraMessage message = new SocketCameraMessage();


    // Start is called before the first frame update
    void Start()
    {
        cameraComponent = GetComponent<Camera>();
        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        renderTexture.Create();
    }

    private Texture2D GetTexture2D()
    {
        cameraComponent.targetTexture = renderTexture;
        cameraComponent.Render();

        RenderTexture.active = renderTexture;
        Texture2D photo = new Texture2D(width, height, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;


        cameraComponent.targetTexture = null;

        return photo;
    }

    public Tuple<string, string, string> GetCurrentMessage()
    {
        Texture2D currentImage = GetTexture2D();
        CameraFeed = currentImage.EncodeToJPG();
        string base64Encoded = Convert.ToBase64String(CameraFeed);

        Destroy(currentImage);

        message.pixels = CameraFeed;
        message.base64 = base64Encoded;

        return Tuple.Create("Camera", JsonUtility.ToJson(message), "json-string");
    }
}
