using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core;
using static UnityEditor.PlayerSettings;

public class GpsPublisher : MonoBehaviour
{
    public string topicName = "gps";
    private ROSConnection ros;
    // Desert origin coordinates (example: Sahara)
    public double originLat = 24.396308;
    public double originLon = 12.859733;

    void Start()
    {
        // Get ROS connection instance
        ros = ROSConnection.GetOrCreateInstance();

        // Register the publisher first
        ros.RegisterPublisher<NavSatFixMsg>(topicName);
    }

    void FixedUpdate()
    {
        var msg = new NavSatFixMsg();
        msg.header.frame_id = "gps_link";

        // Timestamp
        uint seconds = (uint)Clock.time;
        uint nanoseconds = (uint)((Clock.time - seconds) * Clock.k_NanoSecondsInSeconds);
        msg.header.stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg((int)seconds, nanoseconds);

        // Convert Unity Meters to Lat/Lon (Very basic approximation)
        // 111,111 meters is roughly 1 degree
        msg.latitude = originLat + (transform.position.z / 111111d);
        msg.longitude = originLon + (transform.position.x / (111111d * Mathf.Cos((float)originLat * Mathf.Deg2Rad)));
        msg.altitude = transform.position.y;

        ROSConnection.GetOrCreateInstance().Publish(topicName, msg);
    }
}