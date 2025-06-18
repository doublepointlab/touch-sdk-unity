using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Psix.Examples
{
    public class SliderExample : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 1f;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshPro valueText;

        void Update()
        {
            slider.value -= Watch.Instance.GravityCorrectedGyroDelta.x * sensitivity * Time.deltaTime;
            valueText.text = slider.value.ToString("F1");
        }
    }
}
