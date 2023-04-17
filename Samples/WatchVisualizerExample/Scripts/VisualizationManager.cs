using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Psix.Examples
{

    public class VisualizationManager : MonoBehaviour
    {
        // To be replaced with IWatch if Unity implements support
        // https://forum.unity.com/threads/serialized-interface-fields.1238785/
        public TextMeshPro connectionText;
        public AccelerometerVisualizer accelerometerVisualizer;
        public GyroscopeVisualizer gyroscopeVisualizer;
        public TouchVisualizer touchVisualizer;
        public DialVisualizer dialVisualizer;
        public TapVisualizer tapVisualizer;
        public QuaternionVisualizer quaternionVisualizer;

        void Start()
        {
            if (Watch.Instance == null){
                Debug.Log("VisualizationManager: Null watch");
                return;
            }
            Watch.Instance.OnAcceleration += accelerometerVisualizer.UpdateVisualizer;
            Watch.Instance.OnAngularVelocity += gyroscopeVisualizer.UpdateGyroscope;
            Watch.Instance.OnTouch += touchVisualizer.UpdateTouchIndicator;
            Watch.Instance.OnRotary += dialVisualizer.UpdateDialPosition;
            Watch.Instance.OnGesture += tapVisualizer.UpdateTapCount;
            Watch.Instance.OnOrientation += quaternionVisualizer.UpdateOrientation;

            Watch.Instance.OnConnect += () => { connectionText.text = "Connected"; };
            Watch.Instance.OnDisconnect += () => { connectionText.text = "Disconnected"; };
        }
    }
}
