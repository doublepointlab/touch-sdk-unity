// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using Psix.Interaction;
using System;
using UnityEngine;

namespace Psix
{
    /* Interface defining a generic watch.
    */
    public interface IWatch
    {
        event Action<Gesture> OnGesture;

        event Action<TouchEvent> OnTouch;

        event Action OnButton;

        event Action<Direction> OnRotary;

        event Action<Vector3> OnAcceleration;

        event Action<Vector3> OnAngularVelocity;

        event Action<Quaternion> OnOrientation;

        event Action<Vector3> OnGravity;

        event Action OnConnect;
        event Action OnDisconnect;
        public bool Connected { get; }

        public void Vibrate(int length, float amplitude);
        public void CancelVibration();

        public void Connect();
        public void Disconnect();

        // Needs to clear all Action subscriptions
        public void ClearSubscriptions();
    }
}