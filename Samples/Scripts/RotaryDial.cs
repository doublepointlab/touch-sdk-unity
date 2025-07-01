using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Psix.Examples
{
    public class RotaryDialExample : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI valueText;
        private float dialValue;

        private void Update()
        {
            dialValue -= Watch.Instance.AngularVelocity.z * Time.deltaTime;
            dialValue = Mathf.Clamp01(dialValue);
            fillImage.fillAmount = Remap(dialValue, 0, 1, 0.125f, 0.875f);
            valueText.text = (dialValue * 100).ToString("F0") + "%";
        }

        private static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
