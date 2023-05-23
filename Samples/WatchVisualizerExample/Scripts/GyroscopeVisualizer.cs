using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class GyroscopeVisualizer : VisualizerElement
    {
        [SerializeField] private Transform markerX;
        [SerializeField] private Transform markerY;
        [SerializeField] private Transform markerZ;

        [SerializeField] private TextMeshPro textX;
        [SerializeField] private TextMeshPro textY;
        [SerializeField] private TextMeshPro textZ;

        public override void RegisterWatch(Watch watch)
        {
            watch.OnAngularVelocity += UpdateGyroscope;
        }

        public void UpdateGyroscope(Vector3 gyro)
        {
            markerX.localEulerAngles = new Vector3(gyro.x*20, 0, 0);
            markerY.localEulerAngles = new Vector3(0, gyro.y*20, 0);
            markerZ.localEulerAngles = new Vector3(0,0, gyro.z*20);

            textX.text = gyro.x.ToString("F4");
            textY.text = gyro.y.ToString("F4");
            textZ.text = gyro.z.ToString("F4");
        }
    }
}
