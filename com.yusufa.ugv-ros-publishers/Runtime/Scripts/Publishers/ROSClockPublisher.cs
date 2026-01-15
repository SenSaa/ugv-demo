using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Rosgraph; // For ClockMsg
using RosMessageTypes.BuiltinInterfaces; // For TimeMsg
using Unity.Robotics.Core; // For Clock.cs

public class ROSClockPublisher : MonoBehaviour
{
    [SerializeField] Clock.ClockMode m_ClockMode = Clock.ClockMode.UnityScaled;
    [SerializeField] double m_PublishRateHz = 100f;

    private ROSConnection m_ROS;
    private double m_LastPublishTimeSeconds;

    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;
    bool ShouldPublishMessage => Clock.time > m_LastPublishTimeSeconds + PublishPeriodSeconds;

    void Start()
    {
        Clock.Mode = m_ClockMode;
        m_ROS = ROSConnection.GetOrCreateInstance();
        m_ROS.RegisterPublisher<ClockMsg>("clock");
        m_LastPublishTimeSeconds = Clock.time;
    }

    void Update()
    {
        if (ShouldPublishMessage)
        {
            var publishTime = Clock.time;
            var clockMsg = new ClockMsg
            {
                clock = new TimeMsg
                {
                    sec = (int)publishTime,
                    nanosec = (uint)((publishTime - Math.Floor(publishTime)) * Clock.k_NanoSecondsInSeconds)
                }
            };

            m_LastPublishTimeSeconds = publishTime;
            m_ROS.Publish("clock", clockMsg);
        }
    }
}