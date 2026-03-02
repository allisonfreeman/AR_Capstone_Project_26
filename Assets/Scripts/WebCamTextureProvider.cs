using UnityEngine;

namespace HoloLens2ObjectDetection
{
    /// <summary>
    /// Camera provider using Unity's WebCamTexture for Editor and non-HoloLens testing
    /// </summary>
    public class WebCamTextureProvider : MonoBehaviour, ICameraProvider
    {
        [SerializeField]
        [Tooltip("Requested camera resolution width")]
        private int requestedWidth = 1280;

        [SerializeField]
        [Tooltip("Requested camera resolution height")]
        private int requestedHeight = 720;

        [SerializeField]
        [Tooltip("Requested camera framerate")]
        private int requestedFPS = 30;

        private WebCamTexture webCamTexture;
        private Texture2D frameTexture;
        private bool isRunning = false;

        public Texture2D GetLatestFrame()
        {
            if (webCamTexture == null || !webCamTexture.isPlaying)
            {
                return null;
            }

            if (!webCamTexture.didUpdateThisFrame)
            {
                return frameTexture;
            }

            // Create or resize frame texture if needed
            if (frameTexture == null || 
                frameTexture.width != webCamTexture.width || 
                frameTexture.height != webCamTexture.height)
            {
                if (frameTexture != null)
                {
                    Destroy(frameTexture);
                }
                frameTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
            }

            // Copy pixels from WebCamTexture to Texture2D
            Color[] pixels = webCamTexture.GetPixels();
            frameTexture.SetPixels(pixels);
            frameTexture.Apply();

            return frameTexture;
        }

        public bool IsRunning()
        {
            return isRunning && webCamTexture != null && webCamTexture.isPlaying;
        }

        public void StartProvider()
        {
            if (isRunning)
            {
                Debug.LogWarning("WebCamTextureProvider is already running");
                return;
            }

            // Find available webcam devices
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("No webcam devices found");
                return;
            }

            // Use the first available webcam
            string deviceName = devices[0].name;
            Debug.Log($"Starting WebCamTexture with device: {deviceName}");

            webCamTexture = new WebCamTexture(deviceName, requestedWidth, requestedHeight, requestedFPS);
            webCamTexture.Play();
            isRunning = true;

            Debug.Log($"WebCamTexture started: {webCamTexture.width}x{webCamTexture.height} @ {webCamTexture.requestedFPS} fps");
        }

        public void StopProvider()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                Destroy(webCamTexture);
                webCamTexture = null;
            }

            if (frameTexture != null)
            {
                Destroy(frameTexture);
                frameTexture = null;
            }

            isRunning = false;
            Debug.Log("WebCamTextureProvider stopped");
        }

        private void OnDestroy()
        {
            StopProvider();
        }

        private void OnDisable()
        {
            StopProvider();
        }
    }
}
