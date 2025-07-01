using System.Collections;
using Psix.Interaction;
using TMPro;
using UnityEngine;

namespace Psix.Examples
{
    public class HandednessExample : MonoBehaviour
    {
        [SerializeField] private Transform handImage;
        [SerializeField] private TextMeshProUGUI handednessText;

        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                handImage.localScale = new Vector3(1, 1, Watch.Instance.Handedness == Hand.Right ? 1 : -1);
                handednessText.text = Watch.Instance.Handedness.ToString();
            }
        }
    }
}