using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UgvAssignment;

public class AckermannController : MonoBehaviour
{
    [Header("Vehicle Links")]
    public ArticulationBody chassis;
    public ArticulationBody flKnuckle, frKnuckle;
    public ArticulationBody[] driveWheels;

    [Header("Dimensions")]
    public float wheelbase = 2.7f;
    public float trackWidth = 1.6f;

    [Header("Speed Control")]
    public float kP = 500f;
    private float targetVelocityMS = 0f;
    private float targetSteerAngle = 0f;

    [Header("Keyboard Control")]
    public bool enableKeyboardOverride = true;
    public float keyboardMaxSpeed = 8f;
    public float keyboardMaxSteer = 0.785f; // ~45 degrees

    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<InputMsg>("ugv_input", OnInputReceived);
    }

    void OnInputReceived(InputMsg msg)
    {
        targetVelocityMS = (float)msg.throttle;
        targetSteerAngle = (float)msg.steering_angle;
        if (msg.brake > 0) targetVelocityMS = 0;
    }

    void FixedUpdate()
    {
        // Use ROS control commands as default values.
        float finalSpeed = targetVelocityMS;
        float finalSteer = targetSteerAngle;

        // Check for Keyboard Override
        if (enableKeyboardOverride)
        {
            // Get Inputs
            float vInput = Input.GetAxis("Vertical");
            float hInput = Input.GetAxis("Horizontal");

            // Calculate speed and steering.
            if (Mathf.Abs(vInput) > 0.05f || Mathf.Abs(hInput) > 0.05f)
            {
                finalSpeed = vInput * keyboardMaxSpeed;
                finalSteer = hInput * keyboardMaxSteer;
            }
        }

        // Apply speed and steering.
        ApplySpeedController(finalSpeed);
        ApplyAckermannSteering(finalSteer);
    }

    void ApplySpeedController(float targetSpeed)
    {
        // P-controller.
        float currentSpeed = transform.InverseTransformDirection(chassis.velocity).z;
        float error = targetSpeed - currentSpeed;
        float adjustment = error * kP;

        // Use calculated speed to drive wheels.
        foreach (var wheel in driveWheels)
        {
            var drive = wheel.xDrive;
            drive.targetVelocity += adjustment * Time.fixedDeltaTime;
            wheel.xDrive = drive;
        }
    }

    void ApplyAckermannSteering(float steerAngleRad)
    {
        if (Mathf.Abs(steerAngleRad) < 0.001f)
        {
            SetSteerAngle(flKnuckle, 0);
            SetSteerAngle(frKnuckle, 0);
            return;
        }

        // Ackermann steering
        float radius = wheelbase / Mathf.Tan(steerAngleRad);
        float leftAngle = Mathf.Atan(wheelbase / (radius - (trackWidth / 2))) * Mathf.Rad2Deg;
        float rightAngle = Mathf.Atan(wheelbase / (radius + (trackWidth / 2))) * Mathf.Rad2Deg;

        // Apply steering to steering links.
        SetSteerAngle(flKnuckle, leftAngle);
        SetSteerAngle(frKnuckle, rightAngle);
    }

    void SetSteerAngle(ArticulationBody knuckle, float angleDeg)
    {
        var drive = knuckle.xDrive;
        drive.target = angleDeg;
        knuckle.xDrive = drive;
    }
}