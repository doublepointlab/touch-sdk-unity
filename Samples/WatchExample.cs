using System.Linq;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UI;

using Psix;

public class WatchExample : MonoBehaviour
{

    public Text StatusText;
    public Text ButtonPositionText;

    private ConcurrentQueue<int> gestureQueue
        = new ConcurrentQueue<int>();

    private ConcurrentQueue<(int type, float[] coords)> touchQueue
        = new ConcurrentQueue<(int type, float[] coords)>();

    private ConcurrentQueue<(int type, int info)> motionQueue
        = new ConcurrentQueue<(int type, int info)>();

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
        watch.OnGesture = (type) => { gestureQueue.Enqueue(type); };
        watch.OnTouchEvent = (type, coords) => { touchQueue.Enqueue((type, coords)); };
        watch.OnMotionEvent = (type, info) => { motionQueue.Enqueue((type, info)); };

        // Start scanning for the watch. Connect to it if found
        watch.Connect();
    }

    void Update()
    {
        // Query last known orientation. This will return (1, 0, 0, 0) if
        // no device is yet connected.
        float[] quat = watch.GetOrientation();
        ButtonPositionText.text = string.Format("{0:+0.00;-0.00}", quat[0])
                          + " " + string.Format("{0:+0.00;-0.00}", quat[1])
                          + " " + string.Format("{0:+0.00;-0.00}", quat[2])
                          + " " + string.Format("{0:+0.00;-0.00}", quat[3]);


        // Get interaction events from queues

        int gesture;
        (int type, float[] coords) touch;
        (int type, int info) motion;

        while (gestureQueue.TryDequeue(out gesture))
        {
            switch(gesture)
            {
                case 0: StatusMessage = "TAP"; break;
                case 1: StatusMessage = "TAP"; break;
                case 2: StatusMessage = "TAP"; break;
            }
        }

        while (touchQueue.TryDequeue(out touch))
        {
            switch(touch.type)
            {
                case 0: StatusMessage = "TOUCH"; break;
                case 1: StatusMessage = "UNTOUCH"; break;
                case 2: StatusMessage = "SLIDE AT " + touch.coords[0].ToString() + ", " + touch.coords[1].ToString(); break;
            }
        }

        while (motionQueue.TryDequeue(out motion))
        {
            switch(motion.type)
            {
                case 255: StatusMessage = "ROTATION CLOCKWISE"; break;
                case 1: StatusMessage = "ROTATION COUNTER-CLOCKWISE"; break;
            }
        }
    }

    public void Trigger()
    {
        watch.TriggerHaptics(200, 200);
    }
}
