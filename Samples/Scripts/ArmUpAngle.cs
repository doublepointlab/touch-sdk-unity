using TMPro;
using UnityEngine;

namespace Psix.Examples
{
    public class ArmUpExample : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Transform armPivot;

        private void Update()
        {
            var gravityAngle = 90 - Mathf.Atan2(-Watch.Instance.Gravity.z, new Vector2(Watch.Instance.Gravity.x, Watch.Instance.Gravity.y).magnitude) * Mathf.Rad2Deg;
            armPivot.localEulerAngles = new Vector3(0, 0, gravityAngle);
            text.text = gravityAngle.ToString("F1") + "°";
        }
    }
}