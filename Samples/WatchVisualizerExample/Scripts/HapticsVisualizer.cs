using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class HapticsVisualizer : MonoBehaviour
    {
        [SerializeField] private WatchManager watchManager;

        [Space]

        [SerializeField] private Transform region;
        [SerializeField] private Transform axisOrigin;

        [SerializeField] private Transform cursor;
        [SerializeField] private Renderer cursorHead;
        [SerializeField] private TextMeshPro cursorPowerText;
        [SerializeField] private TextMeshPro cursorLengthText;

        [SerializeField] private TextMeshPro powerText;
        [SerializeField] private TextMeshPro lengthText;

        private Plane regionPlane;

        private float yRange;
        private float xRange;

        [SerializeField] private float bpm = 30;
        [SerializeField] private int maxDurationMilliseconds = 800;
        [SerializeField] private int minDurationMilliseconds = 1;

        private float scaledDuration;
        private float timeOfLastPulse;

        void Start()
        {
            yRange = region.localScale.y;
            xRange = region.localScale.x;

            regionPlane = new Plane(region.forward, region.position);
        }

        void Update()
        {
            Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // Return everything to the right color after timeout
            if (Time.time - timeOfLastPulse > 0.1)
            {
                cursorHead.material.color = Color.grey;
                powerText.color = Color.grey;
                lengthText.color = Color.grey;
                cursorPowerText.color = Color.grey;
                cursorLengthText.color = Color.grey;
            }


            if (regionPlane.Raycast(cameraRay, out float distanceToCamera))
            {
                Vector3 raycastCursorPosition = cameraRay.GetPoint(distanceToCamera);
                Vector3 raycastCursorLocalPosition = axisOrigin.InverseTransformPoint(raycastCursorPosition);

                if (raycastCursorLocalPosition.y > 0 &&
                    raycastCursorLocalPosition.y < yRange &&
                    raycastCursorLocalPosition.x > 0 &&
                    raycastCursorLocalPosition.x < xRange)
                {

                    cursor.localPosition = raycastCursorLocalPosition;

                    float normalizedPower = raycastCursorLocalPosition.y / yRange;
                    float normalizedDuration = raycastCursorLocalPosition.x / xRange;
                    scaledDuration = Mathf.Pow((maxDurationMilliseconds / minDurationMilliseconds), normalizedDuration) * minDurationMilliseconds;

                    cursorPowerText.text = normalizedPower.ToString("F4");
                    cursorLengthText.text = ((int)Mathf.Round(scaledDuration)).ToString();

                    float bps = bpm / 60;
                    if (Time.time - timeOfLastPulse > 1 / bps)
                    {
                        timeOfLastPulse = Time.time;
                        
                        watchManager.Vibrate((int)Mathf.Round(scaledDuration), normalizedPower);
                        cursorHead.material.color = Color.white;
                        powerText.color = Color.white;
                        lengthText.color = Color.white;
                        cursorPowerText.color = Color.white;
                        cursorLengthText.color = Color.white;

                        powerText.text = normalizedPower.ToString("F4");
                        lengthText.text = ((int)Mathf.Round(scaledDuration)).ToString();
                    }
                }
            }
        }
    }
}
