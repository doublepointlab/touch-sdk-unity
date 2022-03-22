// Copyright (C) 2022 Port 6 Oy <hello@port6.io> – All rights reserved

using System.Linq;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

using Psix;
using Psix.Interaction;

[System.Serializable]
public class PsixGestureEvent : UnityEvent<Gesture>
{
}

[System.Serializable]
public class PsixTouchEvent : UnityEvent<TouchEventArgs>
{
}

[System.Serializable]
public class PsixMotionEvent : UnityEvent<MotionEventArgs>
{
}

public class WatchManager : MonoBehaviour
{

    /// Invoked when a gesture is detected by the watch.
    public PsixGestureEvent m_GestureEvent;

    /// Invoked when a touch screen event is detected by the watch.
    public PsixTouchEvent m_TouchEvent;

    /// Invoked when a motion event (such as a physical button click or rotary input rotation)
    /// is detected by the watch.
    public PsixMotionEvent m_MotionEvent;

    /// Invoked when a connection to the watch is established.
    public UnityEvent m_ConnectEvent;

    /// Invoked when the connection to the watch is severed.
    public UnityEvent m_DisconnectEvent;

    /// Invoked when an attempt to connect to a watch times out.
    public UnityEvent m_TimeoutEvent;

    /// Angular velocity of the watch in its own coordinate system, degrees per second.
    /// Returns a zero vector if no watch is connected.
    public Vector3 AngularVelocity { get { return watch.AngularVelocity; } }

    /// Acceleration of the watch in its own coordinate system, meters per second squared.
    /// Returns a zero vector if no watch is connected.
    public Vector3 Acceleration { get { return watch.Acceleration; } }

    /// Estimated direction of gravity in the coordinate system of the watch,
    /// meters per second squared. Returns a zero vector if no watch is connected.
    public Vector3 Gravity { get { return watch.Gravity; } }

    /// Absolute orientation quaternion of watch in a reference coordinate system.
    /// Quaternion {x*sin(t/2), y*sin(t/2), z*sin(t/2), cos(t/2)} corresponds
    /// to a rotation of watch from the reference position around the unit vector
    /// axis {x, y, z}, such that the directions {1, 0, 0}, {0, 1, 0}, and {0, 0, 1}
    /// correspond to the "magnetic" East, magnetic North, and upwards directions,
    /// respectively. Returns {0, 0, 0, 1} if no watch is connected.
    public Quaternion Orientation { get {return watch.Orientation; } }

    /// Indicates whether the touch screen of the watch is being touched.
    public bool IsTouching { get; private set; }

    /// The screen coordinates of the last detected touch event in screen pixel coordinates.
    public Vector2 TouchCoordinates { get; private set; } = Vector2.zero;

    /// The number of clockwise steps that the watch rotary input has taken since the
    /// start of this script's life cycle.
    public int RotaryPosition { get; private set; } = 0;

    /// The number of taps detected by the watch since the start of this script's life cycle.
    public int TapCount { get; private set; } = 0;

    /// Indicates whether a connection to a watch is active.
    public bool IsConnected {get { return watch.IsConnected; } }

    /// Name tag of the watch to connect to.
    [Tooltip("Name of the watch to connect to. Leave empty to connect to any compatible device within range.")]
    [SerializeField]
    private string watchName = "";

    private Watch watch;

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

            TouchCoordinates = touchEventArgs.coords;
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

        Connect();
    }

    /**
     * Trigger a one-shot haptic feedback effect on the watch.
     *
     * @param length The duration of the effect in milliseconds.
     * @param amplitude The strength of the effect, between 0.0 and 1.0.
     */
    public void Vibrate(int length = 10, float amplitude = 0.5f)
    {
        watch.Vibrate(length, amplitude);
    }

    /**
     * Try to discover and connect to a watch. Return true if watch discovery was
     * initiated successfully and false if something went wrong. Times out after
     * 20 seconds by default if no watch is found.
     *
     * @param timeout Timeout interval in milliseconds.
     */
    public bool Connect(int timeout = 20000)
    {
        return watch.Connect(
            onConnected: () => { m_ConnectEvent?.Invoke(); },
            onDisconnected: () => { m_DisconnectEvent?.Invoke(); },
            onTimeout: () => { m_TimeoutEvent?.Invoke(); },
            timeoutInterval: timeout
        );
    }

    /**
     * Disconnect the watch.
     */
    public void Disconnect()
    {
        watch.Disconnect();
    }
}
