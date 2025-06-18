using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class AccelerometerVisualizer : MonoBehaviour
    {
        [SerializeField] private Transform indicatorX;
        [SerializeField] private Transform indicatorY;
        [SerializeField] private Transform indicatorZ;

        [SerializeField] private Transform accelerometerVector;
        [SerializeField] private Transform accelerometerTip;

        [SerializeField] private TextMeshPro textX;
        [SerializeField] private TextMeshPro textY;
        [SerializeField] private TextMeshPro textZ;
        
        [SerializeField] private LinePlotter plotterX, plotterY, plotterZ;
        
        private const float AccelerationMultiplier = 250f;

        private void OnEnable()
        {
            Watch.Instance.OnAcceleration += UpdateVisualizer;
        }
        
        private void OnDisable()
        {
            Watch.Instance.OnAcceleration -= UpdateVisualizer;
        }


        private void UpdateVisualizer(Vector3 acceleration)
        {
            const float clampDist = .05f;
            var accelerationClamped = new Vector3(
                Mathf.Clamp(acceleration.x / AccelerationMultiplier, -clampDist, clampDist),
                Mathf.Clamp(acceleration.y / AccelerationMultiplier, -clampDist, clampDist), 
                Mathf.Clamp(acceleration.z / AccelerationMultiplier, -clampDist, clampDist));
            
            accelerometerTip.localPosition = accelerationClamped;
            
            accelerometerVector.localScale = new Vector3(1, 1, Vector3.Distance(accelerometerTip.localPosition, Vector3.zero));
            accelerometerVector.LookAt(accelerometerTip);
            
            indicatorX.localPosition = new Vector3(0,0, accelerationClamped.x);
            indicatorY.localPosition = new Vector3(0,0, accelerationClamped.y);
            indicatorZ.localPosition = new Vector3(0,0, accelerationClamped.z);

            textX.text = acceleration.x.ToString("F4");
            textY.text = acceleration.y.ToString("F4");
            textZ.text = acceleration.z.ToString("F4");
            
            const float max = 100;
            plotterX.AddPoint(acceleration.x, -max, max);
            plotterY.AddPoint(acceleration.y, -max, max);
            plotterZ.AddPoint(acceleration.z, -max, max);
        }
    }
}