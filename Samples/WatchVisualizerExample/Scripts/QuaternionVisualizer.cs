using UnityEngine;
using TMPro;

namespace Psix.Examples
{
    public class QuaternionVisualizer : VisualizerElement
    {

        [SerializeField] private Transform visualizerObject;

        [SerializeField] private TextMeshPro valueX;
        [SerializeField] private TextMeshPro valueY;
        [SerializeField] private TextMeshPro valueZ;
        [SerializeField] private TextMeshPro valueW;


        public override void RegisterWatch(Watch watch)
        {
            watch.OnOrientation += UpdateOrientation;
        }

        public void UpdateOrientation(Quaternion currentQuaternion)
        {
            Quaternion unityQuaternion = new Quaternion(currentQuaternion.x, currentQuaternion.y, currentQuaternion.z, currentQuaternion.w);

            valueX.text = currentQuaternion.x.ToString("F4");
            valueY.text = currentQuaternion.y.ToString("F4");
            valueZ.text = currentQuaternion.z.ToString("F4");
            valueW.text = currentQuaternion.w.ToString("F4");

            visualizerObject.rotation = unityQuaternion;

        }
    }
}
