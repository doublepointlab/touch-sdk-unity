using TMPro;
using UnityEngine;

namespace Psix.Examples
{
    public class PalmUpExample : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI isPalmUpText;
        [SerializeField] private Transform watchPivot;
        [SerializeField] private float palmUpThreshold = 50f;
        
        private static float AngleToWorldDown => Vector3.Angle(Vector3.down, Watch.Instance.Gravity.normalized);
        private bool IsPalmUp => AngleToWorldDown < palmUpThreshold;
        private void Update()
        {
            var angleToWorldDown = AngleToWorldDown;
            watchPivot.localEulerAngles = new Vector3(0, 0, angleToWorldDown);
            text.text = angleToWorldDown.ToString("F1") + "°";
            isPalmUpText.text = IsPalmUp ? "Palm Up" : "Palm Down";
        }
    }
}