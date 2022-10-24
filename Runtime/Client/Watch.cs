// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Google.Protobuf;

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
        private string ProtobufOutputUUID = "f9d60371-5325-4c64-b874-a68c7c555bad";
        private string ProtobufInputUUID = "f9d60372-5325-4c64-b874-a68c7c555bad";

        /**
         * Constructor.
         *
         * @param name The nametag of the watch that should be connected to.
         */
        public Watch()
        {
            client = new GattClient();

            // Use this to detect connection
            client.SubscribeToCharacteristic(ProtobufServiceUUID, ProtobufOutputUUID, protobufCallback);
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
                timeout: timeoutInterval,
                selector: select
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
            var update = new Proto.InputUpdate {
                HapticEvent = new Proto.HapticEvent {
                    Type = Proto.HapticEvent.Types.HapticType.Oneshot,
                    Length = clampedLength,
                    Intensity = clampedAmplitude
                }
            };

            client.SendBytes(update.ToByteArray(), ProtobufServiceUUID, ProtobufInputUUID);

        }

        /**
         * Cancel an ongoing haptic effect that was triggered earlier.
         */
        public void CancelVibration()
        {
            var update = new Proto.InputUpdate {
                HapticEvent = new Proto.HapticEvent {
                    Type = Proto.HapticEvent.Types.HapticType.Cancel
                }
            };

            client.SendBytes(update.ToByteArray(), ProtobufServiceUUID, ProtobufInputUUID);
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


        private bool select(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);
            return update.Signals.All(signal => (signal != Proto.Update.Types.Signal.Disconnect));
        }

        // Internal callback
        private void protobufCallback(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);

            if (update.SensorFrames.Count > 0) {
                var frame = update.SensorFrames.Last();
                // Update sensor stuff
                Acceleration = new Vector3(frame.Acc.X, frame.Acc.Y, frame.Acc.Z);
                Gravity = new Vector3(frame.Grav.X, frame.Grav.Y, frame.Grav.Z);
                AngularVelocity = new Vector3(frame.Gyro.X, frame.Gyro.Y, frame.Gyro.Z);
                Orientation = new Quaternion(-frame.Quat.Y, -frame.Quat.Z, frame.Quat.X, frame.Quat.W);

                OnAccelerationUpdated(Acceleration);
                OnGravityUpdated(Gravity);
                OnAngularVelocityUpdated(AngularVelocity);
                OnOrientationUpdated(Orientation);
            }

            foreach (var gesture in update.Gestures) {
                OnGesture((Interaction.Gesture)gesture.Type);
            }
            foreach (var touchEvent in update.TouchEvents) {
                var coords = touchEvent.Coords.First();
                OnTouchEvent(new TouchEventArgs(
                    (Interaction.TouchType)(touchEvent.EventType),
                    new Vector2(coords.X, coords.Y)
                ));
            }

            foreach (var buttonEvent in update.ButtonEvents) {
                OnMotionEvent(new MotionEventArgs(
                    Interaction.MotionType.Button,
                    (Interaction.MotionInfo)(buttonEvent.Id)
                ));
            }

            foreach (var rotaryEvent in update.RotaryEvents) {
                OnMotionEvent(new MotionEventArgs(
                    Interaction.MotionType.Rotary,
                    (Interaction.MotionInfo)((rotaryEvent.Step > 0) ? 1 : 0)
                ));
            }

            foreach (var signal in update.Signals) {
                if (signal == Proto.Update.Types.Signal.Disconnect) {
                    Disconnect();
                }
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
    }
}
