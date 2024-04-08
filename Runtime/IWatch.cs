// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using Psix.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Psix
{
    /* Interface defining a generic watch.
    */
    public interface IWatch
    {
        event Action<Gesture> OnGesture;

        event Action<float> OnGestureProbability;

        event Action<TouchEvent> OnTouch;

        event Action OnButton;

        event Action<Direction> OnRotary;

        event Action<Vector3> OnAcceleration;

        event Action<Vector3> OnAngularVelocity;

        event Action<Quaternion> OnOrientation;

        event Action<Vector3> OnGravity;

        event Action<Hand> OnHandednessChange;

        event Action<HashSet<Gesture>> OnDetectedGesturesChange;

        event Action OnConnect;
        event Action OnDisconnect;
        public bool Connected { get; }

        /**
         * Trigger a one-shot haptic feedback effect on the watch.
         *
         * @param length The duration of the effect in milliseconds.
         * @param amplitude The strength of the effect, between 0.0 and 1.0.
         */
        public void Vibrate(int length, float amplitude);

        /**
         * Cancel an ongoing haptic effect that was triggered earlier.
         */
        public void CancelVibration();

        public void RequestGestureDetection(Gesture gesture);

        /**
         * Connect to the watch running Doublepoint Controller app.
         */
        public void Connect(string name = "");

        /**
         * Disconnect a connected watch.
         */
        public void Disconnect();

        // Clears all Action subscriptions
        public void ClearSubscriptions();
    }
}
