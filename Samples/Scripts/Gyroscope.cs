using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class GyroscopeVisualizer : MonoBehaviour
    {
        [SerializeField] private Transform markerX, markerY, markerZ;
        [SerializeField] private TextMeshPro textX,textY ,textZ; 
        [SerializeField] private LinePlotter plotterX, plotterY, plotterZ;

        private void OnEnable()
        {
            Watch.Instance.OnAngularVelocity += UpdateGyroscope;
        }
        
        private void OnDisable()
        {
            Watch.Instance.OnAngularVelocity -= UpdateGyroscope;
        }
        
        private void UpdateGyroscope(Vector3 gyro)
        {
            const float gyroFactor = 20;
            
            markerX.localEulerAngles = new Vector3(gyro.x * gyroFactor, 0, 0);
            markerY.localEulerAngles = new Vector3(0, gyro.y * gyroFactor, 0);
            markerZ.localEulerAngles = new Vector3(0, 0, gyro.z * gyroFactor);

            textX.text = gyro.x.ToString("F4");
            textY.text = gyro.y.ToString("F4");
            textZ.text = gyro.z.ToString("F4");
            
            const float maxGyro = 20;
            plotterX.AddPoint(gyro.x, -maxGyro, maxGyro);
            plotterY.AddPoint(gyro.y, -maxGyro, maxGyro);
            plotterZ.AddPoint(gyro.z, -maxGyro, maxGyro);
        }
    }
}
