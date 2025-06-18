using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class OrientationVisualizer : MonoBehaviour
    {
        [SerializeField] private Transform visualizerObject;
        [SerializeField] private TextMeshPro valueX, valueY, valueZ,valueW;
        [SerializeField] private LinePlotter plotterX, plotterY, plotterZ, plotterW;

        private void OnEnable()
        {
            Watch.Instance.OnOrientation += UpdateOrientation;
        }
        private void OnDisable()
        {
            Watch.Instance.OnOrientation -= UpdateOrientation;
        }

        private void UpdateOrientation(Quaternion currentQuaternion)
        {
            valueX.text = currentQuaternion.x.ToString("F4");
            valueY.text = currentQuaternion.y.ToString("F4");
            valueZ.text = currentQuaternion.z.ToString("F4");
            valueW.text = currentQuaternion.w.ToString("F4");
            
            plotterX.AddPoint(currentQuaternion.x, -1, 1);
            plotterY.AddPoint(currentQuaternion.y, -1, 1);
            plotterZ.AddPoint(currentQuaternion.z, -1, 1);
            plotterW.AddPoint(currentQuaternion.w, -1, 1);
            
            visualizerObject.rotation = currentQuaternion;
        }
    }
}
