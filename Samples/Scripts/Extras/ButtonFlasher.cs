using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Psix.Examples
{
    public class ButtonFlasher : MonoBehaviour
    {
        [SerializeField] private Image imageToFlash;
        [SerializeField] private float flashDuration = 0.5f;
        private Coroutine currentFx;
    
        public void FlashButton()
        {
            if (currentFx != null) StopCoroutine(currentFx);
            currentFx = StartCoroutine(FlashFX());
        }
    
        private IEnumerator FlashFX()
        {
            const float duration = .5f;
            var elapsedTime = 0f;
            var currentColor = imageToFlash.color;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                currentColor.a = Mathf.Lerp(1, 0, elapsedTime/ duration);
                imageToFlash.color = currentColor;
                yield return null;
            }
            currentColor.a = 0;
            imageToFlash.color = currentColor;
        }
    }
}