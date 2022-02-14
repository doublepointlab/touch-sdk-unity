using System.Linq;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

using Psix;

[System.Serializable]
public class GestureEvent : UnityEvent<Psix.Interaction.Gesture>
{
}

[System.Serializable]
public class TouchEvent : UnityEvent<Psix.Interaction.TouchEventArgs>
{
}

[System.Serializable]
public class MotionEvent : UnityEvent<Psix.Interaction.MotionEventArgs>
{
}

public class WatchManager : MonoBehaviour
{

    public GestureEvent m_GestureEvent;
    public TouchEvent m_TouchEvent;
    public MotionEvent m_MotionEvent;

    public UnityEvent m_ConnectEvent;
    public UnityEvent m_DisconnectEvent;
    public UnityEvent m_TimeoutEvent;


    public Vector3 Acceleration { get; private set; }
    public Vector3 AngularVelocity { get; private set; }
    public Vector3 Gravity { get; private set; }
    public Quaternion Orientation { get; private set; }

    public bool IsTouching { get; private set; }

    public int RotaryPosition { get; private set; } = 0;
    public int TapCount { get; private set; } = 0;

    [SerializeField]
    private string watchName = "NHYP";

    private Watch watch;

    public bool IsConnected {get { return watch.IsConnected; } }

    void Start()
    {

        watch = new Watch(watchName);

        watch.OnGesture = (gesture) => {
            m_GestureEvent?.Invoke(gesture);
            TapCount++;
        };
        watch.OnTouchEvent = (touchEventArgs) => {

            m_TouchEvent?.Invoke(touchEventArgs);

            if (touchEventArgs.type == Psix.Interaction.TouchType.On)
            {
                IsTouching = true;
            } else if (touchEventArgs.type == Psix.Interaction.TouchType.Off)
            {
                IsTouching = false;
            }
        };
        watch.OnMotionEvent = (motionEventArgs) => {

            m_MotionEvent?.Invoke(motionEventArgs);

            if (motionEventArgs.info == Psix.Interaction.MotionInfo.Clockwise)
            {
                RotaryPosition++;
            } else if (motionEventArgs.info == Psix.Interaction.MotionInfo.CounterClockwise)
            {
                RotaryPosition--;
            }
        };

        watch.Connect(
            onConnected: () => { m_ConnectEvent?.Invoke(); },
            onDisconnected: () => { m_DisconnectEvent?.Invoke(); },
            onTimeout: () => { m_TimeoutEvent?.Invoke(); }
        );
    }

    void Update()
    {
        float[] quat = watch.GetOrientation();
        Orientation = new Quaternion(quat[0], quat[1], quat[2], quat[3]);
        float[] accel = watch.GetAcceleration();
        Acceleration = new Vector3(accel[0], accel[1], accel[2]);
        float[] gyro = watch.GetRotationalVelocity();
        AngularVelocity = new Vector3(gyro[0], gyro[1], gyro[2]);
        float[] grav = watch.GetGravity();
        Gravity = new Vector3(grav[0], grav[1], grav[2]);
    }

    public void Vibrate(int length = 10, float amplitude = 0.5f)
    {
        watch.Vibrate(length, amplitude);
    }
}
