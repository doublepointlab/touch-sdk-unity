using System.Linq;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UI;

using Psix;

public class WatchExample : MonoBehaviour
{

    public Text StatusText;
    public Text ButtonPositionText;

    private ConcurrentQueue<Psix.Interaction.Gesture> gestureQueue
        = new ConcurrentQueue<Psix.Interaction.Gesture>();

    private ConcurrentQueue<Psix.Interaction.TouchEvent> touchQueue
        = new ConcurrentQueue<Psix.Interaction.TouchEvent>();

    private ConcurrentQueue<Psix.Interaction.MotionEvent> motionQueue
        = new ConcurrentQueue<Psix.Interaction.MotionEvent>();

    private string StatusMessage
    {
        set
        {
            StatusText.text = value;
        }
    }

    private Watch watch;

    void Start()
    {
        watch = new Watch("Galaxy Watch4 Classic (NHYP)");

        // Enqueue gestures, motion events, and touch events to dedicated queues
        watch.OnGesture = (gesture) => { gestureQueue.Enqueue(gesture); };
        watch.OnTouchEvent = (touchEvent) => { touchQueue.Enqueue(touchEvent); };
        watch.OnMotionEvent = (motionEvent) => { motionQueue.Enqueue(motionEvent); };

        // Start scanning for the watch. Connect to it if found
        watch.Connect();
    }

    void Update()
    {
        // Query last known orientation. This will return (1, 0, 0, 0) if
        // no device is yet connected.
        float[] quat = watch.GetRotationalVelocity();
        ButtonPositionText.text = string.Format("{0:+0.00;-0.00}", quat[0])
                          + " " + string.Format("{0:+0.00;-0.00}", quat[1])
                          + " " + string.Format("{0:+0.00;-0.00}", quat[2]);


        // Get interaction events from queues

        Psix.Interaction.Gesture gesture;
        Psix.Interaction.TouchEvent touch;
        Psix.Interaction.MotionEvent motion;

        while (gestureQueue.TryDequeue(out gesture))
        {
            switch(gesture)
            {
                case Psix.Interaction.Gesture.Tap: StatusMessage = "TAP"; break;
            }
        }

        while (touchQueue.TryDequeue(out touch))
        {
            string coordinates = touch.coords[0].ToString() + ", " + touch.coords[1].ToString();
            switch(touch.type)
            {
                case Psix.Interaction.TouchType.On: StatusMessage = "TOUCH AT " + coordinates; break;
                case Psix.Interaction.TouchType.Off: StatusMessage = "UNTOUCH AT " + coordinates; break;
                case Psix.Interaction.TouchType.Move: StatusMessage = "SLIDE AT " + coordinates; break;
            }
        }

        while (motionQueue.TryDequeue(out motion))
        {
            switch(motion.type)
            {
                case Psix.Interaction.MotionType.Rotary:
                    switch(motion.info)
                    {
                        case Psix.Interaction.MotionInfo.Clockwise:
                            StatusMessage = "ROTATION CLOCKWISE"; break;
                        case Psix.Interaction.MotionInfo.CounterClockwise:
                            StatusMessage = "ROTATION COUNTER-CLOCKWISE"; break;
                    }
                    break;
            }
        }
    }

    public void Trigger()
    {
        watch.TriggerHaptics(200, 200);
    }
}
