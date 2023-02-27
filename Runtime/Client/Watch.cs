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
        private static PsixLogger logger = new PsixLogger("Watch");

        private GattConnection? client;
        private GattConnector? connector;

        List<Subscription> subs = new List<Subscription>();

        public Watch()
        {
            subs.Add(new Subscription(GattServices.ProtobufServiceUUID, GattServices.ProtobufOutputUUID, updateCallback));
        }

        /**
         * Connect to the watch running Port 6 XR Controller app.
         *
         * @param onConnected Action that is called once the connection is established.
         * @param onDisconnected Action that is called when the connection is severed.
         * @param onTimeout Action that is called if no matching device is found.
         */
        public void Connect(
            string name,
            Action? onConnected = null,
            Action? onDisconnected = null,
            Action? onTimeout = null,
            int timeout = 120 * 1000)
        {
            connector = new GattConnector(onAccepted: (conn) =>
            {
                logger.Trace("OnAccept");
                client = conn;
                // Add disconnect callbacks only once a connection is found
                conn.OnDisconnect += (c) =>
                {
                    // Unfortunately the action delegates do not seem immutable
                    // as would be intuitive, but this action gets called to every
                    // disconnecting device.
                    if (c.Address == conn.Address)
                    {
                        disconnectAction();
                        onDisconnected?.Invoke();
                    }
                };
                connectAction();
                onConnected?.Invoke();
            }, name, subs,
            new List<string>() { GattServices.InteractionServiceUUID }, timeout, select);
        }

        /**
         * Disconnect a connected watch.
         */
        public void Disconnect()
        {
            logger.Trace("Disconnect");
            connector?.StopAndDisconnect();
            client?.Disconnect();
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
            var update = new Proto.InputUpdate
            {
                HapticEvent = new Proto.HapticEvent
                {
                    Type = Proto.HapticEvent.Types.HapticType.Oneshot,
                    Length = clampedLength,
                    Intensity = clampedAmplitude
                }
            };

            client?.SendBytes(update.ToByteArray(), GattServices.ProtobufServiceUUID, GattServices.ProtobufInputUUID);

        }

        /**
         * Cancel an ongoing haptic effect that was triggered earlier.
         */
        public void CancelVibration()
        {
            var update = new Proto.InputUpdate
            {
                HapticEvent = new Proto.HapticEvent
                {
                    Type = Proto.HapticEvent.Types.HapticType.Cancel
                }
            };

            client?.SendBytes(update.ToByteArray(), GattServices.ProtobufServiceUUID, GattServices.ProtobufInputUUID);
        }

        /// Angular velocity of the watch in degrees per second.
        /// Returns a zero vector if no watch is connected.
        public Vector3? AngularVelocity { get; private set; } = null;
        public Action<Vector3> OnAngularVelocityUpdated = (data) => { return; };

        /// Acceleration of the watch in meters per second squared.
        /// Returns a zero vector if no watch is connected.
        public Vector3? Acceleration { get; private set; } = null;
        public Action<Vector3> OnAccelerationUpdated = (data) => { return; };

        /// Estimated direction of gravity meters per second squared.
        /// Returns a zero vector if no watch is connected.
        public Vector3? Gravity { get; private set; } = null;
        public Action<Vector3> OnGravityUpdated = (data) => { return; };

        /// Absolute orientation quaternion of watch in a reference coordinate system.
        /// Quaternion {x*sin(t/2), y*sin(t/2), z*sin(t/2), cos(t/2)} corresponds
        /// to a rotation of watch from the reference position around the unit vector
        /// axis {x, y, z}, such that the directions {1, 0, 0}, {0, 1, 0}, and {0, 0, 1}
        /// correspond to the "magnetic" East, magnetic North, and upwards directions,
        /// respectively. Returns {0, 0, 0, 1} if no watch is connected.
        public Quaternion? Orientation { get; private set; } = null;
        public Action<Quaternion> OnOrientationUpdated = (data) => { return; };

        // Which hand the watch is worn on?
        public Hand Handedness = Hand.None;

        // User callbacks for interaction events
        public Action<Gesture> OnGesture = (gesture) => { return; };
        public Action<TouchEventArgs> OnTouchEvent = (touchEvent) => { return; };
        public Action<MotionEventArgs> OnMotionEvent = (motionEvent) => { return; };
        public Action<Hand> OnHandednessChangeEvent = (hand) => { return; };


        private bool select(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);
            return update.Signals.All(signal => (signal != Proto.Update.Types.Signal.Disconnect));
        }

        private void RequestInfo() {
            client?.RequestBytes(
                GattServices.ProtobufServiceUUID,
                GattServices.ProtobufOutputUUID,
                infoCallback
            );
        }

        // Internal callbacks
        private void updateCallback(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);

            if (update.SensorFrames.Count > 0)
            {
                var frame = update.SensorFrames.Last();
                // Update sensor stuff
                Acceleration = new Vector3(frame.Acc.Y, frame.Acc.Z, -frame.Acc.X);
                Gravity = new Vector3(frame.Grav.Y, frame.Grav.Z, -frame.Grav.X);
                AngularVelocity = new Vector3(-frame.Gyro.Y, -frame.Gyro.Z, frame.Gyro.X);
                Orientation = new Quaternion(-frame.Quat.Y, -frame.Quat.Z, frame.Quat.X, frame.Quat.W);

                OnAccelerationUpdated(Acceleration ?? new Vector3());
                OnGravityUpdated(Gravity ?? new Vector3());
                OnAngularVelocityUpdated(AngularVelocity ?? new Vector3());
                OnOrientationUpdated(Orientation ?? new Quaternion());
            }

            foreach (var gesture in update.Gestures)
            {
                OnGesture((Interaction.Gesture)gesture.Type);
            }
            foreach (var touchEvent in update.TouchEvents)
            {
                var coords = touchEvent.Coords.First();
                OnTouchEvent(new TouchEventArgs(
                    (Interaction.TouchType)(touchEvent.EventType),
                    new Vector2(coords.X, coords.Y)
                ));
            }

            foreach (var buttonEvent in update.ButtonEvents)
            {
                OnMotionEvent(new MotionEventArgs(
                    Interaction.MotionType.Button,
                    (Interaction.MotionInfo)(buttonEvent.Id)
                ));
            }

            foreach (var rotaryEvent in update.RotaryEvents)
            {
                OnMotionEvent(new MotionEventArgs(
                    Interaction.MotionType.Rotary,
                    (Interaction.MotionInfo)((rotaryEvent.Step > 0) ? 1 : 0)
                ));
            }

            foreach (var signal in update.Signals)
            {
                if (signal == Proto.Update.Types.Signal.Disconnect)
                {
                    Disconnect();
                }
            }

        }

        private void infoCallback(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);
            var info = update.Info;

            var newHandedness = Hand.None;

            if (info.Hand == Proto.Info.Types.Hand.Right)
                newHandedness = Hand.Right;
            if (info.Hand == Proto.Info.Types.Hand.Left)
                newHandedness = Hand.Left;

            if (newHandedness != Handedness) {
                OnHandednessChangeEvent(newHandedness);
            }

            Handedness = newHandedness;

        }

        // Internal connection lifecycle callbacks

        private void connectAction()
        {
            RequestInfo();
            connector = null;
            logger.Debug("connect action");
            IsConnected = true;
        }

        private void disconnectAction()
        {
            logger.Debug("disconnect action");
            IsConnected = false;
            AngularVelocity = null;
            Acceleration = null;
            Gravity = null;
            Orientation = null;
        }

        private void timeoutAction()
        {
            logger.Debug("timeout action");
            IsConnected = false;
        }

    }
}
