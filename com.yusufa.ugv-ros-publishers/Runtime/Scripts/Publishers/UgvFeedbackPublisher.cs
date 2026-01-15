using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UgvAssignment;
using Unity.Robotics.Core;
using RosMessageTypes.Nav;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

public class UgvFeedbackPublisher : MonoBehaviour
{
    private ROSConnection ros;

    [SerializeField] private string topicName = "vehicle_feedback";
    [SerializeField] private string jointStatesTopic = "/joint_states";
    [SerializeField] private string odomTopic = "/odom";
    [SerializeField] private float publishFrequency = 0.05f; // 20Hz

    [Header("Frames")]
    [SerializeField] private string odomFrameId = "odom";
    [SerializeField] private string baseFrameId = "base_link";

    [Header("Articulation References")]
    [SerializeField] private ArticulationBody chassis;
    [SerializeField] private ArticulationBody flKnuckle;
    [SerializeField] private ArticulationBody frKnuckle;
    [SerializeField] private ArticulationBody flSuspension;
    [SerializeField] private ArticulationBody frSuspension;
    [SerializeField] private ArticulationBody rlSuspension;
    [SerializeField] private ArticulationBody rrSuspension;
    [SerializeField] private ArticulationBody flWheel;
    [SerializeField] private ArticulationBody frWheel;
    [SerializeField] private ArticulationBody rlWheel;
    [SerializeField] private ArticulationBody rrWheel;

    private float lastPublishTime;

    void Start()
    {
        // Establish ROS connection and register all publishers
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<OutputMsg>(topicName);
        ros.RegisterPublisher<RosMessageTypes.Sensor.JointStateMsg>(jointStatesTopic);
        ros.RegisterPublisher<OdometryMsg>(odomTopic);
    }

    void FixedUpdate()
    {
        if (Time.time - lastPublishTime > publishFrequency)
        {
            PublishFeedback(); // Vehicle feedback
            PublishJointStates(); // Joint state data
            PublishOdometry(); // Odometry data
            lastPublishTime = Time.time;
        }
    }

    void PublishFeedback()
    {
        // 1) Calculate velocities in local frame
        Vector3 localVel = transform.InverseTransformDirection(chassis.velocity);
        double linearVel = localVel.z;
        double angularVel = chassis.angularVelocity.y;

        // 2) Get steering angles
        double flAngle = flKnuckle.jointPosition[0];
        double frAngle = frKnuckle.jointPosition[0];

        // 3)   Determine state based on motion
        string stateString = (Mathf.Abs((float)linearVel) < 0.01f) ? "Parking" : "Manual";

        // 4) Publish custom vehicle feedback message 
        ros.Publish(topicName, new OutputMsg(stateString, linearVel, angularVel, flAngle, frAngle));
    }

    void PublishJointStates()
    {
        // 1) Create timestamp
        var msg = new RosMessageTypes.Sensor.JointStateMsg();

        uint seconds = (uint)Clock.time;
        uint nanoseconds = (uint)((Clock.time - seconds) * Clock.k_NanoSecondsInSeconds);
        msg.header.stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg((int)seconds, nanoseconds);

        msg.header.frame_id = "";

        // 2) Define joint names (order must match position/velocity arrays below)
        msg.name = new string[]
        {
            "fl_suspension", "fr_suspension", "rl_suspension", "rr_suspension",
            "fl_steer", "fr_steer",
            "fl_wheel", "fr_wheel", "rl_wheel", "rr_wheel"
        };

        // 3) Populate joint positions
        msg.position = new double[]
        {
            flSuspension.jointPosition[0], frSuspension.jointPosition[0],
            rlSuspension.jointPosition[0], rrSuspension.jointPosition[0],
            flKnuckle.jointPosition[0],    frKnuckle.jointPosition[0],
            flWheel.jointPosition[0],      frWheel.jointPosition[0],
            rlWheel.jointPosition[0],      rrWheel.jointPosition[0]
        };

        // 4) Populate joint velocities
        msg.velocity = new double[]
        {
            flSuspension.jointVelocity[0], frSuspension.jointVelocity[0],
            rlSuspension.jointVelocity[0], rrSuspension.jointVelocity[0],
            flKnuckle.jointVelocity[0],    frKnuckle.jointVelocity[0],
            flWheel.jointVelocity[0],      frWheel.jointVelocity[0],
            rlWheel.jointVelocity[0],      rrWheel.jointVelocity[0]
        };

        // Publish joint states
        ros.Publish(jointStatesTopic, msg);
    }

    // * A hacky approah here, as I couldn't get the proper time to set up proper odometry!
    // This makes it seem that the odom reflects vehicle in RVIZ.
    void PublishOdometry()
    {
        var msg = new OdometryMsg();

        // 1) Timestamp
        uint seconds = (uint)Clock.time;
        uint nanoseconds = (uint)((Clock.time - seconds) * Clock.k_NanoSecondsInSeconds);
        msg.header.stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg((int)seconds, nanoseconds);

        // 2) Set frame configuration
        // Hack:
        // Odometry expressed IN base_link, relative TO base_link
        msg.header.frame_id = "base_link";
        msg.child_frame_id = "base_link";

        // 3) Set pose (position and orientation)
        // Zero translation (arrow stays at origin)
        msg.pose.pose.position = new PointMsg(0, 0, 0);

        // Take rotation from Unity transform
        // Coordinate conversion: Unity -> ROS
        Quaternion q = transform.rotation;
        msg.pose.pose.orientation = new QuaternionMsg(-q.z, q.x, -q.y, q.w);

        // 4) Set twist (velocities) 
        // Forward speed only (makes arrow scale correctly)
        Vector3 localVel = transform.InverseTransformDirection(chassis.velocity);
        msg.twist.twist.linear = new Vector3Msg(localVel.z, 0, 0);

        // 5) Publish odom
        ros.Publish(odomTopic, msg);
    }

}
