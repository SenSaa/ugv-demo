using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core;

namespace Sensors
{
    public class LidarSensor : MonoBehaviour
    {
        [SerializeField] string topicName = "/scan";
        [SerializeField] int numRays = 360;
        [SerializeField] float range = 20f;
        [SerializeField] float publishHz = 10f;

        private ROSConnection ros;
        private float lastPublish;

        void Start()
        {
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<LaserScanMsg>(topicName);
        }

        void Update()
        {
            // Rate limiti to publishing frequency (10Hz)
            if (Time.time - lastPublish < (1f / publishHz)) return;
            lastPublish = Time.time;

            PublishLaserScan();
        }
        
        // Lidar simulation
        void PublishLaserScan()
        {
            var msg = new LaserScanMsg();
            // 1) Create timestamp
            uint seconds = (uint)Clock.time;
            uint nanoseconds = (uint)((Clock.time - seconds) * Clock.k_NanoSecondsInSeconds);
            msg.header.stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg((int)seconds, nanoseconds);
            msg.header.frame_id = "lidar_link"; // IMPORTANT

            // 2) Configure scan parameters
            msg.angle_min = -Mathf.PI;
            msg.angle_max = Mathf.PI;
            msg.angle_increment = (msg.angle_max - msg.angle_min) / numRays;
            msg.range_min = 0.1f;
            msg.range_max = range;

            // 3) Perform raycasts
            float[] ranges = new float[numRays];
            for (int i = 0; i < numRays; i++)
            {
                // Calculate ray direction
                float angle = msg.angle_min + i * msg.angle_increment;
                Vector3 dir =
                    Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f) *
                    transform.forward;

                // Cast ray and record distance
                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, range))
                {
                    ranges[i] = hit.distance;

                    Debug.DrawRay(transform.position, dir * hit.distance, Color.red, 0.016f);
                }
                else
                    ranges[i] = float.PositiveInfinity; // No hit detected
            }

            // 4) Publish scan data
            msg.ranges = ranges;
            ros.Publish(topicName, msg);
        }
    }
}
