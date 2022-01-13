#nullable enable

using System;
using System.Linq;
using System.Threading;

using UnityEngine;


namespace Psix {

using Interaction;

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

    public void Connect(Action? onConnected = null, Action? onDisconnected = null, Action? onTimeout = null)
    {
        client.Connect(
            onConnected: () => { connectAction(onConnected); },
            onDisconnected: () => { disconnectAction(onDisconnected); },
            onTimeout: () => { timeoutAction(onTimeout); }
        );
    }

    public void Disconnect()
    {
        client.Disconnect();
    }

    public bool IsConnected { get; private set; } = false;

    public void TriggerHaptics(int Length, int Amplitude)
    {
        byte[] data = { 0, 0 };
        try {
            data = new byte[]{ Convert.ToByte(Length), Convert.ToByte(Amplitude) };
        }
        catch (OverflowException) {}

        client.SendBytes(data, FeedbackServiceUUID, HapticsUUID);
    }

    // TODO: Should these throw an exception if no device is connected?
    public float[] GetRotationalVelocity() { return gyro; }
    public float[] GetAcceleration() { return acceleration; }
    public float[] GetGravity() { return gravity; }
    public float[] GetOrientation() { return quat; }

    // User callbacks for sensor events
    public Action<float[]> OnRotationalVelocityUpdated = (data) => { return; };
    public Action<float[]> OnAccelerationUpdated = (data) => { return; };
    public Action<float[]> OnGravityUpdated = (data) => { return; };
    public Action<float[]> OnOrientationUpdated = (data) => { return; };

    // User callbacks for interaction events
    public Action<Gesture> OnGesture = (gesture) => { return; };
    public Action<TouchEvent> OnTouchEvent = (touchEvent) => { return; };
    public Action<MotionEvent> OnMotionEvent = (motionEvent) => { return; };

    // Internal sensor state
    private float[] gyro = new float[] {0, 0, 0};
    private float[] acceleration = new float[] {0, 0, 0};
    private float[] gravity = new float[] {0, 0, 0};
    private float[] quat = new float[] {1, 0, 0, 0};

    // Internal callbacks

    private void gyroCallback(byte[] data)
    {
        gyro = getFloatArray(data);
        OnRotationalVelocityUpdated(gyro);
    }

    private void accCallback(byte[] data)
    {
        acceleration = getFloatArray(data);
        OnAccelerationUpdated(acceleration);
    }

    private void gravityCallback(byte[] data)
    {
        gravity = getFloatArray(data);
        OnGravityUpdated(gravity);
    }

    private void quatCallback(byte[] data)
    {
        quat = getFloatArray(data);
        OnOrientationUpdated(quat);
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
            OnTouchEvent(new TouchEvent(
                (Interaction.TouchType)Convert.ToInt32(data[0]),
                getFloatArray(data.Skip(1).ToArray())
            ));
        }
    }

    private void motionCallback(byte[] data)
    {
        if (data.Length == 2)
        {
            OnMotionEvent(new MotionEvent(
                (Interaction.MotionType)Convert.ToInt32(data[0]),
                (Interaction.MotionInfo)Convert.ToInt32(data[1])
            ));
        }
    }

    // Internal connection lifecycle callbacks

    private void connectAction(Action? onConnected)
    {
        Debug.Log("connect action");
        IsConnected = true;
        onConnected?.Invoke();
    }

    private void disconnectAction(Action? onDisconnected)
    {
        Debug.Log("disconnect action");
        IsConnected = false;
        onDisconnected?.Invoke();
    }

    private void timeoutAction(Action? onTimeout)
    {
        Debug.Log("timeout action");
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
