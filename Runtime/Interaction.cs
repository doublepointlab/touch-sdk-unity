// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace Psix.Interaction
{
    public enum Gesture
    {
        NoGesture = 0,
        PinchTap = 1,
        Clench = 2,
        SurfaceTap = 3,
        PinchHold = 4,
    };

    public enum TouchType
    {
        None = 0,
        Press = 1,
        Release = 2,
        Move = 3,
    };

    public enum Direction
    {
        Clockwise = 0,
        CounterClockwise = 1,
    };

    public struct TouchEvent
    {
        public TouchEvent(TouchType t, Vector2 c)
        {
            type = t;
            coords = c;
        }

        public TouchType type;
        public Vector2 coords;
    };
}
