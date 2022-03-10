using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Psix;

namespace Psix.Examples
{
    public class VisualizationManager : MonoBehaviour
    {
        public WatchManager watchManager;
        public TextMeshPro connectionText;
        public AccelerometerManager accelerometerManager;
        public GyroscopeManager gyroscopeManager;
        public TouchManager touchManager;
        public DialManager dialManager;
        public TapManager tapManager;
        public QuaternionManager quaternionManager;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            accelerometerManager.UpdateVisualizer(watchManager.Acceleration);
            gyroscopeManager.UpdateGyroscope(watchManager.AngularVelocity);
            touchManager.UpdateTouchIndicator(watchManager.TouchCoordinates[0] / 450, watchManager.TouchCoordinates[1] / 450, watchManager.IsTouching);
            dialManager.UpdateDialPosition(watchManager.RotaryPosition);
            tapManager.UpdateTapCount(watchManager.TapCount);
            quaternionManager.UpdateOrientation(watchManager.Orientation);

            //inputText.text = watchManager.IsConnected.ToString() +":\n" +
            //    watchManager.Acceleration.ToString("F4") + "\n" +
            //    watchManager.AngularVelocity.ToString("F4") + "\n" +
            //    watchManager.TouchCoordinates[0].ToString("F4") + ", " + watchManager.TouchCoordinates[1].ToString("F4") + "\n" +
            //    watchManager.RotaryPosition.ToString() + "\n" +
            //    watchManager.TapCount.ToString()
            //    ;
            connectionText.text = watchManager.IsConnected.ToString();


        }
    }
}
