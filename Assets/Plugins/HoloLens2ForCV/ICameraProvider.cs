using UnityEngine;

namespace HoloLens2ObjectDetection
{
    /// <summary>
    /// Interface for camera frame providers (WebCam, HoloLens Research Mode, etc.)
    /// </summary>
    public interface ICameraProvider
    {
        /// <summary>
        /// Get the latest camera frame as a Texture2D
        /// </summary>
        /// <returns>The latest frame, or null if not available</returns>
        Texture2D GetLatestFrame();

        /// <summary>
        /// Check if the camera provider is currently running
        /// </summary>
        /// <returns>True if running and providing frames</returns>
        bool IsRunning();

        /// <summary>
        /// Start the camera provider
        /// </summary>
        void StartProvider();

        /// <summary>
        /// Stop the camera provider and release resources
        /// </summary>
        void StopProvider();
    }
}
