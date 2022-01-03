using System;
using System.Linq;
using System.Threading;

namespace Psix {
public class Watch {

    private GattClient client;

    // Sensor service
    private string SensorServiceUUID = "4b574af0-72d7-45d2-a1bb-23cd0ec20c57";
    public string gyroUUID = "4b574af1-72d7-45d2-a1bb-23cd0ec20c57";
    public string accUUID  = "4b574af2-72d7-45d2-a1bb-23cd0ec20c57";
    public string gravUUID = "4b574af3-72d7-45d2-a1bb-23cd0ec20c57";
    public string quatUUID = "4b574af4-72d7-45d2-a1bb-23cd0ec20c57";

    // Feedback service
    private string FeedbackServiceUUID = "42926760-277c-4298-acfe-226b8d1c8c88";
    public string HapticsUUID = "42926761-277c-4298-acfe-226b8d1c8c88";

    // Interaction service
    private string InteractionServiceUUID = "008e74d0-7bb3-4ac5-8baf-e5e372cced76";
    private string GestureUUID = "008e74d1-7bb3-4ac5-8baf-e5e372cced76";
    private string TouchUUID = "008e74d2-7bb3-4ac5-8baf-e5e372cced76";
    private string PhysicalUUID = "008e74d3-7bb3-4ac5-8baf-e5e372cced76";

    public Watch(string name)
    {
        client = new GattClient(name);

        client.SubscribeToCharacteristic(SensorServiceUUID, accUUID, accCallback);
        client.SubscribeToCharacteristic(SensorServiceUUID, gyroUUID, gyroCallback);
        client.SubscribeToCharacteristic(SensorServiceUUID, gravUUID, gravityCallback);
        client.SubscribeToCharacteristic(SensorServiceUUID, quatUUID, quatCallback);

        client.SubscribeToCharacteristic(InteractionServiceUUID, GestureUUID, gestureCallback);
        client.SubscribeToCharacteristic(InteractionServiceUUID, TouchUUID, touchCallback);
        client.SubscribeToCharacteristic(InteractionServiceUUID, PhysicalUUID, physicalCallback);
    }

    public void Connect()
    {
        client.Connect();
    }

    public void Disconnect()
    {
        client.Disconnect();
    }

    private float[] gyro = new float[] {0, 0, 0};
    private float[] acceleration = new float[] {0, 0, 0};
    private float[] gravity = new float[] {0, 0, 0};
    private float[] quat = new float[] {1, 0, 0, 0};

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
    public Action<int> OnGesture = (type) => { return; };
    public Action<int, float[]> OnTouchEvent = (type, coords) => { return; };
    public Action<int, int> OnMotionEvent = (type, info) => { return; };

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
        OnGesture(Convert.ToInt32(data[0]));
    }

    private void touchCallback(byte[] data)
    {
        OnTouchEvent(Convert.ToInt32(data[0]), getFloatArray(data.Skip(1).ToArray()));
    }

    private void physicalCallback(byte[] data)
    {
        OnMotionEvent(Convert.ToInt32(data[0]), Convert.ToInt32(data[1]));
    }

    public void TriggerHaptics(int Length, int Amplitude)
    {
        byte[] data = { 0, 0 };
        try {
            data = new byte[]{ Convert.ToByte(Length), Convert.ToByte(Amplitude) };
        }
        catch (OverflowException) {}

        client.SendBytes(data, FeedbackServiceUUID, HapticsUUID);
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
