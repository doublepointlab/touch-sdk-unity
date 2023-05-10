// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf;

using UnityEngine;

#if UNITY_EDITOR_WIN
#warning "Bluetooth support in Play Mode is experimental and unstable."
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif


namespace Psix
{

    using Interaction;

    /**
     * Implementation of smartwatch interface for bluetooth devices.
     * Provides methods and callbacks related to connecting to Port 6 XR Controller
     * smartwatch app.
     * Check also IWatch.
     */
    [DefaultExecutionOrder(-50)]
    public class BluetoothWatchProvider : MonoBehaviour, IWatch
    {
        [SerializeField] public string watchName = "";

        // The bluetooth name of the watch */
        public string ConnectedWatchName {
            get;
            private set;
        }

        // Connecting to a gatt server might take minutes at worst on some
        // machines.  Most devices will hopefully connect within 30 seconds.
        [HideInInspector] public int connectionTimeoutSeconds = 120;

        public bool ConnectOnStart = true;

        private static PsixLogger logger = new PsixLogger("BluetoothWatchProvider");

        private GattConnection? client;
        private GattConnector? connector;

        List<Subscription> subs = new List<Subscription>();

        /**
         * Connect to the watch running Port 6 XR Controller app.
         */
        public void Connect()
        {
            connector = new GattConnector(onAccepted: (conn, _watchName) =>
            {
                logger.Info("Connected to \"{0}\"", _watchName);
                client = conn;
                ConnectedWatchName = _watchName;
                // Add disconnect callbacks only once a connection is found
                conn.OnDisconnect += (c) =>
                {
                    // Unfortunately the action delegates do not seem immutable
                    // as would be intuitive, but this action gets called to every
                    // disconnecting device.
                    if (c.Address == conn.Address)
                    {
                        disconnectAction();
                        OnDisconnect?.Invoke();
                    }
                };
                connectAction();
                OnConnect?.Invoke();
            }, watchName, subs,
            new List<string>() { GattServices.InteractionServiceUUID }, connectionTimeoutSeconds * 1000, select, OnScanTimeout);
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

        public bool Connected { get; private set; } = false;

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

        public void RequestGestureDetection(Gesture gesture) {
            var update = new Proto.InputUpdate
            {
                ModelRequest = new Proto.Model
                {
                    Gestures = { (Proto.GestureType)(gesture) }
                }
            };

            client?.SendBytes(update.ToByteArray(), GattServices.ProtobufServiceUUID, GattServices.ProtobufInputUUID);

        }

        private void Awake()
        {
            Watch.Instance.RegisterProvider(this);
        }

        private void Start()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("No location permission.");
                Permission.RequestUserPermission(Permission.FineLocation);
            }
#else
            if (ConnectOnStart)
                Connect();
#endif
        }

#if UNITY_ANDROID
        private void Update()
        {
            if (Permission.HasUserAuthorizedPermission(Permission.FineLocation) && client == null && connector == null)
                Connect();
        }
#endif

        public BluetoothWatchProvider()
        {
            ConnectedWatchName = "";
            subs.Add(new Subscription(GattServices.ProtobufServiceUUID, GattServices.ProtobufOutputUUID, protobufCallback));
        }

        /* Documented in WatchInterface */
        public event Action<Vector3>? OnAngularVelocity = null;
        public event Action<Vector3>? OnAcceleration = null;
        public event Action<Vector3>? OnGravity = null;
        public event Action<Quaternion>? OnOrientation = null;
        public event Action<Gesture>? OnGesture = null;
        public event Action<TouchEvent>? OnTouch = null;
        public event Action? OnButton = null;
        public event Action<Direction>? OnRotary = null;

        private Hand Handedness = Hand.None;
        public event Action<Hand>? OnHandednessChange = null;

        private HashSet<Gesture> ActiveGestures = new HashSet<Gesture>();
        public event Action<HashSet<Gesture>>? OnDetectedGesturesChange = null;

        public event Action? OnConnect = null;
        public event Action? OnDisconnect = null;

        /* Not part of generic interface */
        public event Action? OnScanTimeout = null;


        private bool select(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);
            return update.Signals.All(signal => (signal != Proto.Update.Types.Signal.Disconnect));
        }

        private void RequestInfo() {
            client?.RequestBytes(
                GattServices.ProtobufServiceUUID,
                GattServices.ProtobufOutputUUID,
                readCallback
            );
        }

        // Internal callbacks
        private void protobufCallback(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);

            if (update.SensorFrames.Count > 0)
            {
                var frame = update.SensorFrames.Last();
                // Update sensor stuff

                OnAcceleration?.Invoke(new Vector3(frame.Acc.Y, frame.Acc.Z, -frame.Acc.X));
                OnGravity?.Invoke(new Vector3(frame.Grav.Y, frame.Grav.Z, -frame.Grav.X));
                OnAngularVelocity?.Invoke(new Vector3(-frame.Gyro.Y, -frame.Gyro.Z, frame.Gyro.X));
                OnOrientation?.Invoke(new Quaternion(-frame.Quat.Y, -frame.Quat.Z, frame.Quat.X, frame.Quat.W));
            }

            foreach (var gesture in update.Gestures)
                OnGesture?.Invoke((Interaction.Gesture)gesture.Type);
            foreach (var touchEvent in update.TouchEvents)
            {
                Interaction.TouchType type = TouchType.None;
                switch (touchEvent.EventType)
                {
                    case Proto.TouchEvent.Types.TouchEventType.Begin:
                        type = TouchType.Press;
                        break;
                    case Proto.TouchEvent.Types.TouchEventType.End:
                        type = TouchType.Release;
                        break;
                    case Proto.TouchEvent.Types.TouchEventType.Move:
                        type = TouchType.Move;
                        break;
                    default: break;
                }
                var coords = touchEvent.Coords.First();
                OnTouch?.Invoke(new TouchEvent(
                            type,
                    new Vector2(coords.X, coords.Y)
                ));
            }

            foreach (var buttonEvent in update.ButtonEvents)
                OnButton?.Invoke();

            // TODO: Is the direction correct??
            foreach (var rotaryEvent in update.RotaryEvents)
                OnRotary?.Invoke((rotaryEvent.Step > 0) ? Direction.CounterClockwise : Direction.Clockwise);

            foreach (var signal in update.Signals)
            {
                if (signal == Proto.Update.Types.Signal.Disconnect)
                    Disconnect();
            }

            infoCallback(update.Info);

        }

        private void readCallback(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);
            var info = update.Info;
            infoCallback(info);
        }

        private void infoCallback(Proto.Info info) {
            var newHandedness = Hand.None;

            if (info.Hand == Proto.Info.Types.Hand.Right)
                newHandedness = Hand.Right;
            if (info.Hand == Proto.Info.Types.Hand.Left)
                newHandedness = Hand.Left;

            if (newHandedness != Hand.None && newHandedness != Handedness) {
                Handedness = newHandedness;
                OnHandednessChange?.Invoke(newHandedness);
            }

            var newActiveGestures = new HashSet<Gesture>(info.ActiveModel.Gestures.Select(gesture =>
                (Gesture)gesture
            ));
            if (newActiveGestures.Count > 0) {
                if (newActiveGestures != ActiveGestures) {
                    ActiveGestures = newActiveGestures;
                    OnDetectedGesturesChange?.Invoke(newActiveGestures);
                }
            }
        }

        // Internal connection lifecycle callbacks

        private void connectAction()
        {
            RequestInfo();
            connector = null;
            logger.Debug("connect action");
            Connected = true;
        }

        private void disconnectAction()
        {
            logger.Debug("disconnect action");
            Connected = false;
        }

        public void ClearSubscriptions()
        {
            OnGesture = null;
            OnTouch = null;
            OnButton = null;
            OnRotary = null;
            OnHandednessChange = null;
            OnAcceleration = null;
            OnAngularVelocity = null;
            OnOrientation = null;
            OnGravity = null;
            OnConnect = null;
            OnDisconnect = null;
        }
    }
}
