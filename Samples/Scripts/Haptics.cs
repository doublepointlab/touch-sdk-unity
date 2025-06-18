using System.Collections;
using UnityEngine;

namespace Psix.Examples
{
    public class HapticsExample : MonoBehaviour
    {
        [SerializeField] private HapticPattern[] hapticPatterns;

        public void StartHapticPattern()
        {
            StartCoroutine(SendHapticsToWatch());
        }

        private IEnumerator SendHapticsToWatch()
        {
            for (int i = 0; i < hapticPatterns.Length; i++)
            {
                Watch.Instance.Vibrate(hapticPatterns[i].duration, hapticPatterns[i].intensity);
                yield return new WaitForSeconds((hapticPatterns[i].duration + hapticPatterns[i].delay) / 1000f);
            }
        }
   
        [System.Serializable]
        public struct HapticPattern
        {
            public float intensity;
            public int duration;
            public int delay;
        }
    }
}