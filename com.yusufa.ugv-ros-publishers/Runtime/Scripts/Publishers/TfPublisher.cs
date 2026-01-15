using UnityEngine;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Tf2;
using RosMessageTypes.Geometry;
using Unity.Robotics.Core;

public class TfPublisher : MonoBehaviour
{
    [SerializeField] string topicName = "/tf";
    [SerializeField] string rootFrameId = "odom";
    [SerializeField] string baseFrameId = "base_link";

    [Header("Sensor Transforms (children of base_link)")]
    public List<Transform> sensorTransforms = new List<Transform>();

    private ROSConnection ros;
    private double lastPublishTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TFMessageMsg>(topicName);
    }

    // Publishing loop running on physics update.
    void FixedUpdate()
    {
        // Publish at 50 Hz
        if (Clock.time - lastPublishTime >= 0.02f) // 50 Hz
        {
            PublishTF();
            lastPublishTime = Clock.time;
        }
    }

    // TF Tree Construction
    void PublishTF()
    {
        var tfList = new List<TransformStampedMsg>();

        // 1) Publish odom -> base_link (robot's world pose)
        tfList.Add(CreateTF(
            rootFrameId,
            baseFrameId,
            transform.position.To<FLU>(), // Convert Unity coords to ROS (FLU)
            transform.rotation.To<FLU>()
        ));

        // 2) Publish base_link -> sensors (relative transforms)
        foreach (var sensor in sensorTransforms)
        {
            if (sensor == null) continue;

            tfList.Add(CreateTF(
                baseFrameId,
                sensor.name, // Use GameObject name as frame_id
                sensor.localPosition.To<FLU>(), // Local position relative to base_link
                sensor.localRotation.To<FLU>() // Local rotation relative to base_link
            ));
        }

        // 3) Publish all transforms as single message
        ros.Publish(topicName, new TFMessageMsg(tfList.ToArray()));
    }

    // Helper for creating transform msg
    TransformStampedMsg CreateTF(string parent, string child, Vector3<FLU> pos, Quaternion<FLU> rot)
    {
        // Create timestamp
        uint seconds = (uint)Clock.time;
        uint nanoseconds = (uint)((Clock.time - seconds) * Clock.k_NanoSecondsInSeconds);
        var t = new RosMessageTypes.BuiltinInterfaces.TimeMsg((int)seconds, nanoseconds);

        // Build transform message
        return new TransformStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg { frame_id = parent, stamp = t },
            child_frame_id = child,
            transform = new TransformMsg
            {
                translation = new Vector3Msg(pos.x, pos.y, pos.z),
                rotation = new QuaternionMsg(rot.x, rot.y, rot.z, rot.w)
            }
        };
    }
}
