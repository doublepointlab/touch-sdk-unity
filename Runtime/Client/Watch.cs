// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using UnityEngine;


namespace Psix
{

    using Interaction;

    /**
     * Smartwatch interface.
     *
     * Provides methods and callbacks related to connecting to Port 6 XR Controller
     * smartwatch app.
     *
     */
    public class Watch
    {

        private GattClient client;

        // Protobuf service
        private string ProtobufServiceUUID = "f9d60370-5325-4c64-b874-a68c7c555bad";
        private string ProtobufUUID = "f9d60371-5325-4c64-b874-a68c7c555bad";

        /**
         * Constructor.
         *
         * @param name The nametag of the watch that should be connected to.
         */
        public Watch()
        {
            client = new GattClient();

            // Use this to detect connection
            client.SubscribeToCharacteristic(ProtobufServiceUUID, ProtobufUUID, protobufCallback);

        }

        /**
         * Connect to the watch running Port 6 XR Controller app.
         *
         * @param onConnected Action that is called once the connection is established.
         * @param onDisconnected Action that is called when the connection is severed.
         * @param onTimeout Action that is called if no matching device is found.
         */
        public bool Connect(
            string name,
            Action? onConnected = null,
            Action? onDisconnected = null,
            Action? onTimeout = null,
            int timeoutInterval = 60000)
        {
            return client.ConnectToName(
                name,
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
            client.SendBytes(new byte[] { 0xff }, FeedbackServiceUUID, HapticsUUID);
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
            if (quat.Length >= 4)
            {
                // Not only are the axes from android in right handed coordinates and unity in left handed,
                // Unity has its reference (I) pointing towards z axis, while on Android its the x axis.
                // The following have been found by looking at the rotation directions and axes at I of the watch.
                Orientation = new Quaternion(-quat[1], -quat[2], quat[0], quat[3]);
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

        private void protobufCallback(byte[] data)
        {
            Debug.Log($"Got {data.Length} bytes from protobuf service");
            var update = Proto.Update.Parser.ParseFrom(data);
            Debug.Log($"Got {update.SensorFrame.Count} sensor frames from protobuf service");
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
            for (int i = vec.Length - 1; i >= 0; i--)
            {
                vec[i] = System.BitConverter.ToSingle(reversed, 4 * (vec.Length - 1 - i));
            }
            return vec;
        }
    }
}
