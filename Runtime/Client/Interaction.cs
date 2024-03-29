// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace Psix.Interaction
{
    public enum Gesture
    {
        NoGesture = 0,
        Tap = 1,
        Clench = 2,
    };

    public enum TouchType
    {
        None = 0,
        On = 1,
        Off = 2,
        Move = 3,
        Cancel = 4,
    };

    public enum MotionType
    {
        Rotary = 0,
        Button = 1,
    };

    public enum MotionInfo
    {
        Clockwise = 0,
        CounterClockwise = 1,
    };

    public struct TouchEventArgs
    {
        public TouchEventArgs(TouchType t, Vector2 c)
        {
            type = t;
            coords = c;
        }

        public TouchType type;
        public Vector2 coords;
    };

    public struct MotionEventArgs
    {
        public MotionEventArgs(MotionType t, MotionInfo i)
        {
            type = t;
            info = i;
        }

        public MotionType type;
        public MotionInfo info;
    };
}
