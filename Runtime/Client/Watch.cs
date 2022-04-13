// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using UnityEngine;


namespace Psix {

using Interaction;

/**
 * Smartwatch interface.
 *
 * Provides methods and callbacks related to connecting to Port 6 XR Controller
 * smartwatch app.
 *
 */
public class Watch {

    private GattClient client;

    // Sensor service
    private string SensorServiceUUID = "4b574af0-72d7-45d2-a1bb-23cd0ec20c57";
    private string gyroUUID = "4b574af1-72d7-45d2-a1bb-23cd0ec20c57";
    private string accUUID  = "4b574af2-72d7-45d2-a1bb-23cd0ec20c57";
    private string gravUUID = "4b574af3-72d7-45d2-a1bb-23cd0ec20c57";
    private string quatUUID = "4b574af4-72d7-45d2-a1bb-23cd0ec20c57";

    // Feedback service
    private string FeedbackServiceUUID = "42926760-277c-4298-acfe-226b8d1c8c88";
    private string HapticsUUID = "42926761-277c-4298-acfe-226b8d1c8c88";

    // Interaction service
    private string InteractionServiceUUID = "008e74d0-7bb3-4ac5-8baf-e5e372cced76";
    private string GestureUUID = "008e74d1-7bb3-4ac5-8baf-e5e372cced76";
    private string TouchUUID = "008e74d2-7bb3-4ac5-8baf-e5e372cced76";
    private string PhysicalUUID = "008e74d3-7bb3-4ac5-8baf-e5e372cced76";

    // Disconnect service
    private string DisconnectServiceUUID = "e23625a0-a6b6-4aa5-a1ad-b9c5d9158363";
    private string DisconnectUUID = "e23625a1-a6b6-4aa5-a1ad-b9c5d9158363";

    /**
     * Constructor.
     *
     * @param name The nametag of the watch that should be connected to.
     */
    public Watch(string name)
    {
        client = new GattClient(name);

        client.SubscribeToCharacteristic(DisconnectServiceUUID, DisconnectUUID,
            (data) => { Disconnect(); }
        );

        client.SubscribeToCharacteristic(SensorServiceUUID, gyroUUID, gyroCallback);
        client.SubscribeToCharacteristic(SensorServiceUUID, accUUID, accCallback);
        client.SubscribeToCharacteristic(SensorServiceUUID, gravUUID, gravityCallback);
        client.SubscribeToCharacteristic(SensorServiceUUID, quatUUID, quatCallback);

        client.SubscribeToCharacteristic(InteractionServiceUUID, GestureUUID, gestureCallback);
        client.SubscribeToCharacteristic(InteractionServiceUUID, TouchUUID, touchCallback);
        client.SubscribeToCharacteristic(InteractionServiceUUID, PhysicalUUID, motionCallback);
    }

    /**
     * Connect to the watch running Port 6 XR Controller app.
     *
     * @param onConnected Action that is called once the connection is established.
     * @param onDisconnected Action that is called when the connection is severed.
     * @param onTimeout Action that is called if no matching device is found.
     */
    public bool Connect(
        Action? onConnected = null,
        Action? onDisconnected = null,
        Action? onTimeout = null,
        int timeoutInterval = 20000)
    {
        return client.Connect(
            onConnected: () => { connectAction(onConnected); },
            onDisconnected: () => { disconnectAction(onDisconnected); },
            onTimeout: () => { timeoutAction(onTimeout); },
            timeout: timeoutInterval
        );
    }

    /**
     * Disconnect a connected watch.
     */
    public void Disconnect()
    {
        client.Disconnect();
    }

    public bool IsConnected { get; private set; } = false;

    /**
     * Trigger a one-shot haptic feedback effect on the watch.
     *
     * @param length The duration of the effect in milliseconds.
     * @param amplitude The strength of the effect, between 0.0 and 1.0.
     */
    public void Vibrate(int length, float amplitude)
    {
        int clampedLength = Mathf.Clamp(length, 0, 5000);
        float clampedAmplitude = Mathf.Clamp(amplitude, 0.0f, 1.0f);
        byte byteAmplitude = Convert.ToByte(Math.Round(255 * clampedAmplitude));

        List<byte> data = BitConverter.GetBytes(clampedLength).Reverse().ToList();
        data.Insert(0, 0); // One-shot effect
        data.Add(byteAmplitude);

        client.SendBytes(data.ToArray(), FeedbackServiceUUID, HapticsUUID);
    }

    /**
     * Cancel an ongoing haptic effect that was triggered earlier.
     */
    public void CancelVibration()
    {
        client.SendBytes(new byte[] {0xff}, FeedbackServiceUUID, HapticsUUID);
    }

    /// Angular velocity of the watch in its own coordinate system, degrees per second.
    /// Returns a zero vector if no watch is connected.
    public Vector3 AngularVelocity { get; private set; } = Vector3.zero;
    public Action<Vector3> OnAngularVelocityUpdated = (data) => { return; };

    /// Acceleration of the watch in its own coordinate system, meters per second squared.
    /// Returns a zero vector if no watch is connected.
    public Vector3 Acceleration { get; private set; } = Vector3.zero;
    public Action<Vector3> OnAccelerationUpdated = (data) => { return; };

    /// Estimated direction of gravity in the coordinate system of the watch,
    /// meters per second squared. Returns a zero vector if no watch is connected.
    public Vector3 Gravity { get; private set; } = Vector3.zero;
    public Action<Vector3> OnGravityUpdated = (data) => { return; };

    /// Absolute orientation quaternion of watch in a reference coordinate system.
    /// Quaternion {x*sin(t/2), y*sin(t/2), z*sin(t/2), cos(t/2)} corresponds
    /// to a rotation of watch from the reference position around the unit vector
    /// axis {x, y, z}, such that the directions {1, 0, 0}, {0, 1, 0}, and {0, 0, 1}
    /// correspond to the "magnetic" East, magnetic North, and upwards directions,
    /// respectively. Returns {0, 0, 0, 1} if no watch is connected.
    public Quaternion Orientation { get; private set; } = new Quaternion(0, 0, 0, 1);
    public Action<Quaternion> OnOrientationUpdated = (data) => { return; };

    // User callbacks for interaction events
    public Action<Gesture> OnGesture = (gesture) => { return; };
    public Action<TouchEventArgs> OnTouchEvent = (touchEvent) => { return; };
    public Action<MotionEventArgs> OnMotionEvent = (motionEvent) => { return; };


    // Internal sensor and interaction event callbacks

    private void gyroCallback(byte[] data)
    {
        float[] gyro = getFloatArray(data);
        if (gyro.Length == 3)
        {
            AngularVelocity = new Vector3(gyro[0], gyro[1], gyro[2]);
            OnAngularVelocityUpdated(AngularVelocity);
        }
    }

    private void accCallback(byte[] data)
    {
        float[] accel = getFloatArray(data);
        if (accel.Length == 3)
        {
            Acceleration = new Vector3(accel[0], accel[1], accel[2]);
            OnAccelerationUpdated(Acceleration);
        }
    }

    private void gravityCallback(byte[] data)
    {
        float[] grav = getFloatArray(data);
        if (grav.Length == 3)
        {
            Gravity = new Vector3(grav[0], grav[1], grav[2]);
            OnGravityUpdated(Gravity);
        }
    }

    private void quatCallback(byte[] data)
    {
        float[] quat = getFloatArray(data);
        if (quat.Length == 4)
        {
            Orientation = new Quaternion(quat[0], quat[1], quat[2], quat[3]);
            OnOrientationUpdated(Orientation);
        }
    }

    private void gestureCallback(byte[] data)
    {
        if (data.Length == 1)
            OnGesture((Interaction.Gesture)Convert.ToInt32(data[0]));
    }

    private void touchCallback(byte[] data)
    {
        if (data.Length == 9)
        {
            float[] touchCoords = getFloatArray(data.Skip(1).ToArray());
            OnTouchEvent(new TouchEventArgs(
                (Interaction.TouchType)Convert.ToInt32(data[0]),
                new Vector2(touchCoords[0], touchCoords[1])
            ));
        }
    }

    private void motionCallback(byte[] data)
    {
        if (data.Length == 2)
        {
            OnMotionEvent(new MotionEventArgs(
                (Interaction.MotionType)Convert.ToInt32(data[0]),
                (Interaction.MotionInfo)Convert.ToInt32(data[1])
            ));
        }
    }

    // Internal connection lifecycle callbacks

    private void connectAction(Action? onConnected)
    {
        BluetoothLEHardwareInterface.Log("connect action");
        IsConnected = true;
        onConnected?.Invoke();
    }

    private void disconnectAction(Action? onDisconnected)
    {
        BluetoothLEHardwareInterface.Log("disconnect action");
        IsConnected = false;
        AngularVelocity = Vector3.zero;
        Acceleration = Vector3.zero;
        Gravity = Vector3.zero;
        Orientation = new Quaternion(0, 0, 0, 1);
        onDisconnected?.Invoke();
    }

    private void timeoutAction(Action? onTimeout)
    {
        BluetoothLEHardwareInterface.Log("timeout action");
        IsConnected = false;
        onTimeout?.Invoke();
    }


    // Convert array of bytes to array of floats, switching the byte endianness
    private float[] getFloatArray(byte[] data)
    {
        byte[] reversed = data.Reverse().ToArray();
        float[] vec = new float[data.Length / 4];
        for (int i=vec.Length - 1; i>=0; i--)
        {
            vec[i] = System.BitConverter.ToSingle(reversed, 4*(vec.Length - 1 - i));
        }
        return vec;
    }
}
}
