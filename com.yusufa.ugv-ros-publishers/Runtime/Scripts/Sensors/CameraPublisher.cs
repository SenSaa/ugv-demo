using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core;

public class CameraPublisher : MonoBehaviour
{
    public RenderTexture renderTexture;
    public string topicName = "/camera/image";

    private Texture2D texture2D;
    private ROSConnection ros;

    void Start()
    {
        // Create texture matching the render texture dimensions.
        texture2D = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            TextureFormat.RGB24,
            false
        );

        // Establish ROS connection and register as publisher.
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);
    }

    void LateUpdate()
    {
        if (renderTexture == null) return;

        // 1) Copy GPU texture to CPU
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(
            new Rect(0, 0, renderTexture.width, renderTexture.height),
            0, 0
        );
        texture2D.Apply();

        // 2) Extract raw RGB byte data
        byte[] rgbData = texture2D.GetRawTextureData();

        // 3) Create ROS timestamp
        uint seconds = (uint)Clock.time;
        uint nanoseconds = (uint)((Clock.time - seconds) * Clock.k_NanoSecondsInSeconds);

        // 4) Build and publish ROS ImageMsg
        ImageMsg msg = new ImageMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg
            {
                frame_id = "camera_link",
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg((int)seconds, nanoseconds)
            },
            height = (uint)renderTexture.height,
            width = (uint)renderTexture.width,
            encoding = "rgb8",
            is_bigendian = 0,
            step = (uint)(renderTexture.width * 3),
            data = rgbData
        };

        ros.Publish(topicName, msg);
    }
}
