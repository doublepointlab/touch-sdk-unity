
// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

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
     * Abstract base class for watch implementations, useful for parsing
     * protobuf messages.
     */
    abstract class WatchImpl : IWatch
    {

        private static PsixLogger logger = new PsixLogger("WatchImpl");

        public abstract void Connect();
        public abstract void Disconnect();
        public abstract void Vibrate(int length, float amplitude);
        public abstract void CancelVibration();
        public abstract void RequestGestureDetection(Gesture gesture);

        public bool Connected { get; protected set; } = false;

        public WatchImpl(string name = "") { }

        /* Documented in WatchInterface */
        public event Action<Vector3>? OnAngularVelocity = null;
        public event Action<float>? OnGestureProbability = null;
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

        private Proto.GestureType lastGesture = Proto.GestureType.None;

        private void HandleSensorframes(Proto.Update update)
        {
            if (update.SensorFrames.Count > 0)
            {
                var frame = update.SensorFrames.Last();
                // Update sensor stuff

                OnAcceleration?.Invoke(new Vector3(frame.Acc.Y, frame.Acc.Z, -frame.Acc.X));
                OnGravity?.Invoke(new Vector3(frame.Grav.Y, frame.Grav.Z, -frame.Grav.X));
                OnAngularVelocity?.Invoke(new Vector3(-frame.Gyro.Y, -frame.Gyro.Z, frame.Gyro.X));
                OnOrientation?.Invoke(new Quaternion(-frame.Quat.Y, -frame.Quat.Z, frame.Quat.X, frame.Quat.W));
            }
        }

        private void HandleGestures(Proto.Update update)
        {
            foreach (var gesture in update.Gestures)
            {
                if (gesture.Type != Proto.GestureType.PinchHold)
                {
                    if (lastGesture == Proto.GestureType.PinchHold || lastGesture == Proto.GestureType.PinchTap)
                    {
                        OnGesture?.Invoke(Interaction.Gesture.PinchRelease);
                    }
                    else if (gesture.Type != Proto.GestureType.None)
                    {
                        OnGesture?.Invoke((Interaction.Gesture)gesture.Type);
                    }
                }
                lastGesture = gesture.Type;
            }
        }

        private void HandlePredictionOutput(Proto.Update update) {
            foreach (var entry in update.Probabilities) {
                if (entry.Label == Proto.GestureType.PinchHold
                    || entry.Label == Proto.GestureType.PinchTap) {
                    OnGestureProbability?.Invoke(entry.Probability);
                } else if (entry.Label == Proto.GestureType.None) {
                    OnGestureProbability?.Invoke(1 - entry.Probability);
                }
            }
        }

        private void HandleTouchEvents(Proto.Update update)
        {
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
        }

        private void HandleButtonEvents(Proto.Update update)
        {
            foreach (var buttonEvent in update.ButtonEvents)
                OnButton?.Invoke();
        }

        private void HandleRotaryEvents(Proto.Update update)
        {
            foreach (var rotaryEvent in update.RotaryEvents)
                OnRotary?.Invoke((rotaryEvent.Step > 0) ? Direction.CounterClockwise : Direction.Clockwise);
        }

        private void HandleSignals(Proto.Update update)
        {
            foreach (var signal in update.Signals)
            {
                if (signal == Proto.Update.Types.Signal.Disconnect)
                    Disconnect();
            }
        }

        protected void OnProtobufData(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);


            HandleSensorframes(update);
            HandleGestures(update);
            HandlePredictionOutput(update);
            HandleTouchEvents(update);
            HandleRotaryEvents(update);
            HandleSignals(update);
            HandleInfo(update.Info);
        }

        protected byte[] GetHapticsMessage(int length, float amplitude) {

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
            return update.ToByteArray();
        }

        protected byte[] GetHapticsCancellation() {

            var update = new Proto.InputUpdate
            {
                HapticEvent = new Proto.HapticEvent
                {
                    Type = Proto.HapticEvent.Types.HapticType.Cancel
                }
            };
            return update.ToByteArray();
        }

        protected byte[] GetGestureDetectionRequest(Gesture gesture) {
            var update = new Proto.InputUpdate
            {
                ModelRequest = new Proto.Model
                {
                    Gestures = { (Proto.GestureType)(gesture) }
                }
            };

            return update.ToByteArray();
        }

        protected void ReadCallback(byte[] data)
        {
            var update = Proto.Update.Parser.ParseFrom(data);
            var info = update.Info;
            HandleInfo(info);
        }

        // Internal callbacks

        private void HandleInfo(Proto.Info info)
        {
            var newHandedness = Hand.None;

            try
            {
                if (info.Hand == Proto.Info.Types.Hand.Right)
                    newHandedness = Hand.Right;
                else if (info.Hand == Proto.Info.Types.Hand.Left)
                    newHandedness = Hand.Left;

                if (newHandedness != Hand.None && newHandedness != Handedness)
                {
                    Handedness = newHandedness;
                    OnHandednessChange?.Invoke(newHandedness);
                }

            }
            catch (NullReferenceException e)
            {
                logger.Debug("Info:{0}", e.Message);
            }

            try
            {
                var newActiveGestures = new HashSet<Gesture>(info.ActiveModel.Gestures.Select(gesture =>
                    (Gesture)gesture
                ));
                if (newActiveGestures.Count > 0)
                {
                    if (newActiveGestures != ActiveGestures)
                    {
                        ActiveGestures = newActiveGestures;
                        OnDetectedGesturesChange?.Invoke(newActiveGestures);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                logger.Debug("Gestures:{0}", e.Message);
            }
        }


        // Internal connection lifecycle callbacks

        protected void connectAction()
        {
            logger.Debug("connect action");
            Connected = true;
            OnConnect?.Invoke();
        }

        protected void disconnectAction()
        {
            logger.Debug("disconnect action");
            Connected = false;
            OnDisconnect?.Invoke();
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
