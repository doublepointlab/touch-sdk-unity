using UnityEngine;

namespace Psix.Examples
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraAdjuster : MonoBehaviour
    {
        [SerializeField] private Canvas worldSpaceCanvas;
        private Camera orthoCamera;

        private void Awake()
        {
            orthoCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            AdjustCameraSize();
        }

        private void AdjustCameraSize()
        {
            if (worldSpaceCanvas == null)
            {
                Debug.Log("World Space Canvas not assigned.");
                return;
            }

            var canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
            
            var canvasWidth = canvasRect.rect.width * worldSpaceCanvas.transform.localScale.x;
            var canvasHeight = canvasRect.rect.height * worldSpaceCanvas.transform.localScale.y;

            var screenAspect = (float) Screen.width / (float) Screen.height;
            var canvasAspect = canvasWidth / canvasHeight;

            orthoCamera.orthographicSize =
                canvasAspect >= screenAspect ? canvasWidth / screenAspect / 2f : canvasHeight / 2f;

            orthoCamera.transform.position = new Vector3(canvasRect.position.x, canvasRect.position.y,
                orthoCamera.transform.position.z);
        }
    }
}