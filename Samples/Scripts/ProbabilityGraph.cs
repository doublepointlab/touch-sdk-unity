using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Psix.Examples
{
    public class ProbabilityGraph : MonoBehaviour
    {
        public GameObject barPrefab;
        private Vector3 startPosition;
        private float stepDistance;

        private readonly Queue<GameObject> barPool = new();
        [SerializeField] private int totalBars = 300;
        [SerializeField] private Material barEven, barOdd;
        [SerializeField] private Material barBelowThreshold;

        private int barCount;
        private Transform barHolder;

        private void Awake()
        {
            var width = GetComponent<RectTransform>().rect.width;
            startPosition = new Vector3(width * .5f, 0, 0);
            stepDistance = width / totalBars * .001f;

            barHolder = new GameObject("BarHolder").transform;
            barHolder.SetParent(transform);
            barHolder.localPosition = new Vector3(0, -50, 0);
            barHolder.localScale = Vector3.one;
        }

        private void OnEnable()
        {
            Watch.Instance.OnGestureProbability += SpawnBar;
        }

        private void OnDisable()
        {
            Watch.Instance.OnGestureProbability -= SpawnBar;
        }

        private void SpawnBar(float probability)
        {
            foreach (Transform child in barHolder)
            {
                if (child.gameObject.activeSelf)
                {
                    child.position = new Vector3(child.position.x - stepDistance, child.position.y, child.position.z);
                }
            }

            var bar = GetBarFromPool();
            bar.transform.localPosition = startPosition;
            bar.transform.localScale = new Vector3(stepDistance * 10, probability, 1);

            var mat = (barCount % 2 == 0) ? barEven : barOdd;
            if (probability < .5f) mat = barBelowThreshold;
            bar.GetComponent<Image>().material = mat;

            bar.SetActive(true);
            barCount++;

            CheckForRecycling();
        }

        private void CheckForRecycling()
        {
            var endPositionX = -startPosition.x;

            foreach (Transform child in barHolder)
            {
                if (!child.gameObject.activeSelf || !(child.localPosition.x <= endPositionX)) continue;
                child.gameObject.SetActive(false);
                barPool.Enqueue(child.gameObject);
            }
        }

        private GameObject GetBarFromPool()
        {
            if (barPool.Count > 0)
            {
                return barPool.Dequeue();
            }

            var newBar = Instantiate(barPrefab, barHolder);
            newBar.SetActive(false);
            return newBar;
        }
    }
}