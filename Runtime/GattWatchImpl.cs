/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf;

using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Psix
{

    using Interaction;

    /**
     * Implementation of smartwatch interface for bluetooth devices.
     * Provides methods and callbacks related to connecting to WowMouse
     * smartwatch app.
     * Check also IWatch.
     */
    [DefaultExecutionOrder(-50)]
    class GattWatchImpl : WatchImpl
    {

        // The bluetooth name of the watch */
        public string ConnectedWatchName
        {
            get;
            private set;
        }

        // Connecting to a gatt server might take minutes at worst on some
        // machines.  Most devices will hopefully connect within 30 seconds.
        public int connectionTimeoutSeconds = 120;

        private static PsixLogger logger = new PsixLogger("GattWatchImpl");

        private GattConnection? client;
        private GattConnector? connector;

        List<Subscription> subs = new List<Subscription>();

        override public void Connect(string name = "")
        {
            if (connector != null || client != null)
                return;

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
                    }
                };
                RequestInfo();
                connectAction();
                connector = null;
            }, name, subs,
            new List<string>() { GattServices.InteractionServiceUUID }, connectionTimeoutSeconds * 1000, select, OnScanTimeout);
        }

        override public void Disconnect()
        {
            logger.Trace("Disconnect");
            connector?.StopAndDisconnect();
            client?.Disconnect();
        }

        override public void Vibrate(int length, float amplitude)
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

        override public void CancelVibration()
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

        override public void RequestGestureDetection(Gesture gesture)
        {
            var update = new Proto.InputUpdate
            {
                ModelRequest = new Proto.Model
                {
                    Gestures = { (Proto.GestureType)(gesture) }
                }
            };

            client?.SendBytes(update.ToByteArray(), GattServices.ProtobufServiceUUID, GattServices.ProtobufInputUUID);

        }

        public GattWatchImpl()
        {
            ConnectedWatchName = "";
            subs.Add(new Subscription(GattServices.ProtobufServiceUUID, GattServices.ProtobufOutputUUID, OnProtobufData));
        }

        public event Action? OnScanTimeout = null;

        private bool select(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);
            return update.Signals.All(signal => (signal != Proto.Update.Types.Signal.Disconnect));
        }

        private void RequestInfo()
        {
            client?.RequestBytes(
                GattServices.ProtobufServiceUUID,
                GattServices.ProtobufOutputUUID,
                ReadCallback
            );
        }

    }
}
