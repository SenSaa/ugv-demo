using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class ImuPublisher : MonoBehaviour
{
    public string topicName = "imu/data";
    public ArticulationBody chassis;
    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImuMsg>(topicName);
    }

    void FixedUpdate()
    {
        var msg = new ImuMsg();
        msg.header.frame_id = "base_link";

        uint seconds = (uint)Clock.time;
        uint nanoseconds = (uint)((Clock.time - seconds) * Clock.k_NanoSecondsInSeconds);
        msg.header.stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg((int)seconds, nanoseconds);

        // Rotation (Unity to ROS FLU)
        msg.orientation = transform.rotation.To<FLU>();

        // Angular Velocity (rad/s)
        Vector3 localAngularVel = transform.InverseTransformDirection(chassis.angularVelocity);
        msg.angular_velocity = localAngularVel.To<FLU>();

        // Linear Acceleration (m/s^2) 
        msg.linear_acceleration = (transform.InverseTransformDirection(chassis.velocity) / Time.fixedDeltaTime).To<FLU>();

        ros.Publish(topicName, msg);
    }
}