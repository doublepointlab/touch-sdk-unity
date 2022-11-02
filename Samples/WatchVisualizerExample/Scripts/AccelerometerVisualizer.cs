using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class AccelerometerVisualizer : MonoBehaviour
    {

        [SerializeField] private Transform thingX;
        [SerializeField] private Transform thingY;
        [SerializeField] private Transform thingZ;

        [SerializeField] private Transform accelerometerVector;
        [SerializeField] private Transform accelerometerTip;

        [SerializeField] private TextMeshPro textX;
        [SerializeField] private TextMeshPro textY;
        [SerializeField] private TextMeshPro textZ;

        public void UpdateVisualizer(Vector3 acceleration)
        {
            float accelerometerMagnitude = acceleration.magnitude / 150;
            accelerometerTip.localPosition = new Vector3(acceleration.x / 150, acceleration.y / 150, acceleration.z / 150);

            accelerometerVector.localPosition = new Vector3(acceleration.x / 300, acceleration.y / 300, acceleration.z / 300);
            accelerometerVector.localScale = new Vector3(0.002f, 0.002f, accelerometerMagnitude);
            accelerometerVector.LookAt(accelerometerTip);
            
            thingX.localPosition = new Vector3(0,0, acceleration.x / 150);
            thingY.localPosition = new Vector3(0,0, acceleration.y / 150);
            thingZ.localPosition = new Vector3(0,0, acceleration.z / 150);

            textX.text = acceleration.x.ToString("F4");
            textY.text = acceleration.y.ToString("F4");
            textZ.text = acceleration.z.ToString("F4");

        }
    }
}

