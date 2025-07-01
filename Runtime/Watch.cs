/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;
using System;
using System.Collections.Generic;
using Psix.Interaction;
using Psix;


[DefaultExecutionOrder(-100)]
public class Watch : MonoBehaviour
{
    public static Watch Instance { get; private set; }

    private IWatch watch;

    /* Full watch interface documented in IWatch */
    public void Vibrate(int length = 10, float amplitude = 0.5f)
    {
        watch.Vibrate(length, amplitude);
    }

    public void CancelVibration()
    {
        watch.CancelVibration();
    }

    public void RequestGestureDetection(Gesture gesture) {
        watch.RequestGestureDetection(gesture);
    }

    public void Connect(string name = "")
    {
        watch.Connect(name);
    }

    public void Disconnect()
    {
        watch.Disconnect();
    }

    // Receive inferred gestures
    public event Action<Gesture> OnGesture
    {
        add { watch.OnGesture += value; }
        remove { watch.OnGesture -= value; }
    }

    // Receive gesture probabilities
    public event Action<float> OnGestureProbability
    {
        add { watch.OnGestureProbability += value; }
        remove { watch.OnGestureProbability -= value; }
    }

    // Receive touch screen events
    public event Action<TouchEvent> OnTouch
    {
        add { watch.OnTouch += value; }
        remove { watch.OnTouch -= value; }
    }

    // Receive button events
    public event Action OnButton
    {
        add { watch.OnButton += value; }
        remove { watch.OnButton -= value; }
    }

    // Receive rotation events from a rotary dial
    public event Action<Direction> OnRotary
    {
        add { watch.OnRotary += value; }
        remove { watch.OnRotary -= value; }
    }

    /// Acceleration of the watch in its own frame of reference, meters per second squared
    public event Action<Vector3> OnAcceleration
    {
        add { watch.OnAcceleration += value; }
        remove { watch.OnAcceleration -= value; }
    }

    /// Angular velocity of the watch in its own frame of reference, radians per second
    public event Action<Vector3> OnAngularVelocity
    {
        add { watch.OnAngularVelocity += value; }
        remove { watch.OnAngularVelocity -= value; }
    }

    /// Absolute orientation quaternion of watch in its reference coordinate system.
    /// Quaternion {x*sin(t/2), y*sin(t/2), z*sin(t/2), cos(t/2)} corresponds
    /// to a rotation of watch from the reference position around the unit vector
    /// axis {x, y, z}, where the axes correspond to Unity coordinate system. However,
    /// the "initial" reference direction of the watch may not correspond to the reference
    /// direction within Unity.
    public event Action<Quaternion> OnOrientation
    {
        add { watch.OnOrientation += value; }
        remove { watch.OnOrientation -= value; }
    }


    /// Estimated direction of gravity in the coordinate system of the watch,
    /// meters per second squared
    public event Action<Vector3> OnGravity
    {
        add { watch.OnGravity += value; }
        remove { watch.OnGravity -= value; }
    }

    /// Changes in which hand the device is worn on
    public event Action<Hand> OnHandednessChange {
        add { watch.OnHandednessChange += value; }
        remove { watch.OnHandednessChange -= value; }
    }

    /// Changes in the set of gestures that the watch is trying to detect
    public event Action<HashSet<Gesture>> OnDetectedGesturesChange {
        add { watch.OnDetectedGesturesChange += value; }
        remove { watch.OnDetectedGesturesChange -= value; }
    }

    public event Action OnConnect
    {
        add { watch.OnConnect += value; }
        remove { watch.OnConnect -= value; }
    }

    public event Action OnDisconnect
    {
        add { watch.OnDisconnect += value; }
        remove { watch.OnDisconnect -= value; }
    }

    /* Property interface */
    // Inferred gestures
    [HideInInspector] public int TapCount = 0;
    [HideInInspector] public int ClenchCount = 0;

    // Touch screen
    [HideInInspector] public Vector2 TouchPosition = default;
    [HideInInspector] public bool IsTouched = false;

    // Button events
    [HideInInspector] public int ButtonPressCount = 0;
    [HideInInspector] public int RotaryPosition = 0;

    [HideInInspector] public Vector3 Acceleration = default;
    [HideInInspector] public Vector3 AngularVelocity = default;
    [HideInInspector] public Quaternion Orientation = Quaternion.identity;
    [HideInInspector] public Vector3 Gravity = default;

    // Which hand the watch is worn on?
    [HideInInspector] public Hand Handedness = Hand.None;

    [HideInInspector] public int BatteryPercentage { get { return watch.BatteryPercentage; } }

    // Miscellaneous information about the device
    [HideInInspector] public string AppId { get { return watch.AppId; } }
    [HideInInspector] public string AppVersion { get { return watch.AppVersion; } }
    [HideInInspector] public string DeviceName { get { return watch.DeviceName; } }
    [HideInInspector] public string Manufacturer { get { return watch.Manufacturer; } }
    [HideInInspector] public string ModelInfo { get { return watch.ModelInfo; } }
    [HideInInspector] public bool HapticsAvailable { get { return watch.HapticsAvailable; } }
    [HideInInspector] public Vector2 TouchScreenResolution { get { return watch.TouchScreenResolution; } }
    
    public Vector2 GravityCorrectedGyroDelta
    {
        get 
        {
            var deltaY = AngularVelocity.x * Gravity.normalized.y - AngularVelocity.y * Gravity.normalized.x;        
            var deltaX = AngularVelocity.y * Gravity.normalized.y + AngularVelocity.x * Gravity.normalized.x;

            return new Vector2(-deltaX, deltaY);
        }
    }

    // Set of gestures that the watch is trying to detect
    [HideInInspector] public HashSet<Gesture> DetectedGestures = new HashSet<Gesture>();

    public bool Connected
    {
        get { return watch.Connected; }
    }

    /* Called by a WatchProvider to register a watch data source */
    public void RegisterProvider(IWatch watch)
    {
        if (this.watch != null)
        {
            Debug.Log("Replacing existing watch provider");
            watch.ClearSubscriptions();
        }

        this.watch = watch;
        OnGesture += (gesture) =>
        {
            if (gesture == Gesture.PinchTap)
                TapCount++;
            else if (gesture == Gesture.Clench)
                ClenchCount++;
        };
        OnTouch += (touch) =>
        {
            if (touch.type == Psix.Interaction.TouchType.Press)
                IsTouched = true;
            else if (touch.type == Psix.Interaction.TouchType.Release)
                IsTouched = false;
            TouchPosition = touch.coords;
        };
        OnButton += () => { ButtonPressCount++; };
        OnRotary += (direction) => { if (direction == Direction.Clockwise) RotaryPosition--; else RotaryPosition++; };
        OnAcceleration += (acc) => { Acceleration = acc; };
        OnAngularVelocity += (ang) => { AngularVelocity = ang; };
        OnOrientation += (rot) => { Orientation = rot; };
        OnGravity += (grav) => { Gravity = grav; };
        OnHandednessChange += (hand) => { Handedness = hand; };
        OnDetectedGesturesChange += (gestures) => { DetectedGestures = gestures; };
    }

    private void Start()
    {
        if (watch == null)
            Debug.LogWarning("RegisterProvider has not been called: Watch probably not available");
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
}
