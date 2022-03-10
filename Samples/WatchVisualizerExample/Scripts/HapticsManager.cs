using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class HapticsManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject region;
        [SerializeField]
        private GameObject axisOrigin;
        [SerializeField]
        private WatchManager watchManager;

        [SerializeField]
        private GameObject cursor;
        [SerializeField]
        private GameObject cursorHead;
        private Renderer cursorHeadRenderer;
        [SerializeField]
        private TextMeshPro cursorPowerText;
        [SerializeField]
        private TextMeshPro cursorLengthText;

        [SerializeField]
        private TextMeshPro powerText;
        [SerializeField]
        private TextMeshPro lengthText;

        private Ray cameraRay;
        private float distanceToCamera = 0;
        private Plane regionPlane;
        private Vector3 raycastCursorPosition;
        private Vector3 raycastCursorLocalPosition;

        private float yRange;
        private float xRange;

        private float normalizedPower;
        private float normalizedDuration;

        [SerializeField]
        private float bpm = 30;
        [SerializeField]
        private int maxDurationMilliseconds = 800;
        [SerializeField]
        private int minDurationMilliseconds = 1;
        private float scaledDuration;

        private float timeOfLastPulse;

        // Start is called before the first frame update
        void Start()
        {
            yRange = region.transform.localScale.y;
            xRange = region.transform.localScale.x;

            regionPlane = new Plane(region.transform.forward, region.transform.position);

            cursorHeadRenderer = cursorHead.GetComponent<Renderer>();
        }

        // Update is called once per frame
        void Update()
        {

            cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // Return everything to the right color after timeout
            if (Time.time - timeOfLastPulse > 0.1)
            {
                cursorHeadRenderer.material.color = Color.grey;
                powerText.color = Color.grey;
                lengthText.color = Color.grey;
                cursorPowerText.color = Color.grey;
                cursorLengthText.color = Color.grey;
            }


            if (regionPlane.Raycast(cameraRay, out distanceToCamera))
            {
                raycastCursorPosition = cameraRay.GetPoint(distanceToCamera);
                raycastCursorLocalPosition = axisOrigin.transform.InverseTransformPoint(raycastCursorPosition);

                if (raycastCursorLocalPosition.y > 0 &
                raycastCursorLocalPosition.y < yRange &
                raycastCursorLocalPosition.x > 0 &
                raycastCursorLocalPosition.x < xRange)
                {

                    cursor.transform.localPosition = raycastCursorLocalPosition;

                    normalizedPower = raycastCursorLocalPosition.y / yRange;
                    normalizedDuration = raycastCursorLocalPosition.x / xRange;
                    scaledDuration = Mathf.Pow((maxDurationMilliseconds / minDurationMilliseconds), normalizedDuration) * minDurationMilliseconds;

                    cursorPowerText.text = normalizedPower.ToString("F4");
                    cursorLengthText.text = ((int)Mathf.Round(scaledDuration)).ToString();

                    float bps = bpm / 60;
                    if (Time.time - timeOfLastPulse > 1 / bps)
                    {
                        timeOfLastPulse = Time.time;


                        
                        watchManager.Vibrate((int)Mathf.Round(scaledDuration), normalizedPower);
                        cursorHeadRenderer.material.color = Color.white;
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
