namespace Psix
{
    namespace Interaction
    {

        public enum Gesture
        {
            NoGesture = 0,
            Tap = 1,
        };

        public enum TouchType
        {
            On = 0,
            Off = 1,
            Move = 2,
        };

        public enum MotionType
        {
            Rotary = 0,
        };

        public enum MotionInfo
        {
            Clockwise = 0,
            CounterClockwise = 1,
        };

        public struct TouchEvent
        {
            public TouchEvent(TouchType t, float[] c)
            {
                type = t;
                coords = c;
            }

            public TouchType type;
            public float[] coords;
        };

        public struct MotionEvent
        {
            public MotionEvent(MotionType t, MotionInfo i)
            {
                type = t;
                info = i;
            }
            public MotionType type;
            public MotionInfo info;
        };

    }
}
