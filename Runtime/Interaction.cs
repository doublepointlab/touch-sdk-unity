/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace Psix.Interaction
{
    public enum Gesture
    {
        NoGesture = 0,
        PinchTap = 1,
        Clench = 2,
        SurfaceTap = 3,
        PinchRelease = 4,
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

    public enum Hand
    {
        None = 0,
        Right = 1,
        Left = 2
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
