using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Psix.Examples
{
    public class BatteryExample : MonoBehaviour
    {
        [SerializeField] private Image batteryFillLevel;
        [SerializeField] private TextMeshProUGUI batteryPercentageText;

        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                batteryFillLevel.fillAmount = (float) Watch.Instance.BatteryPercentage / 100;
                batteryPercentageText.text = Watch.Instance.BatteryPercentage + "%";
            }
        }
    }
}