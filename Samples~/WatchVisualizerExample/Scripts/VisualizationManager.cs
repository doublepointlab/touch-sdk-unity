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
        public AccelerometerVisualizer accelerometerVisualizer;
        public GyroscopeVisualizer gyroscopeVisualizer;
        public TouchVisualizer touchVisualizer;
        public DialVisualizer dialVisualizer;
        public TapVisualizer tapVisualizer;
        public QuaternionVisualizer quaternionVisualizer;

        void Update()
        {
            accelerometerVisualizer.UpdateVisualizer(watchManager.Acceleration);
            gyroscopeVisualizer.UpdateGyroscope(watchManager.AngularVelocity);
            touchVisualizer.UpdateTouchIndicator(watchManager.TouchCoordinates[0] / 450, watchManager.TouchCoordinates[1] / 450, watchManager.IsTouching);
            dialVisualizer.UpdateDialPosition(watchManager.RotaryPosition);
            tapVisualizer.UpdateTapCount(watchManager.TapCount);
            quaternionVisualizer.UpdateOrientation(watchManager.Orientation);

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
