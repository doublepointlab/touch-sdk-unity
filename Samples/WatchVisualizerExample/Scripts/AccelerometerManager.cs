using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Psix;

namespace Psix.Examples
{
    public class AccelerometerManager : MonoBehaviour
    {

        [SerializeField]
        private GameObject thingX;

        [SerializeField]
        private GameObject thingY;

        [SerializeField]
        private GameObject thingZ;

        [SerializeField]
        private GameObject accelerometerVector;

        [SerializeField]
        private GameObject accelerometerTip;

        private float accelerometerMagnitude;

        [SerializeField]
        private TextMeshPro textX;
        [SerializeField]
        private TextMeshPro textY;
        [SerializeField]
        private TextMeshPro textZ;
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void UpdateVisualizer(Vector3 acceleration)
        {
            accelerometerMagnitude = acceleration.magnitude / 150;
            accelerometerTip.transform.localPosition = new Vector3(acceleration.x / 150, acceleration.y / 150, acceleration.z / 150);

            accelerometerVector.transform.localPosition = new Vector3(acceleration.x / 300, acceleration.y / 300, acceleration.z / 300);
            accelerometerVector.transform.localScale = new Vector3(0.002f, 0.002f, accelerometerMagnitude);
            accelerometerVector.transform.LookAt(accelerometerTip.transform);
            
            thingX.transform.localPosition = new Vector3(acceleration.x/150, 0, 0);
            thingY.transform.localPosition = new Vector3(acceleration.y/150, 0, 0);
            thingZ.transform.localPosition = new Vector3(acceleration.z/150, 0, 0);

            textX.text = acceleration.x.ToString("F4");
            textY.text = acceleration.y.ToString("F4");
            textZ.text = acceleration.z.ToString("F4");

        }
    }
}

