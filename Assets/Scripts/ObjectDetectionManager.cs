using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using UnityEngine.UI;

namespace HoloLens2ObjectDetection
{
    /// <summary>
    /// Main object detection manager using Unity Barracuda for ML inference
    /// </summary>
    public class ObjectDetectionManager : MonoBehaviour
    {
        [Header("Model Configuration")]
        [SerializeField]
        [Tooltip("The ONNX model asset (placeholder or real model)")]
        private NNModel modelAsset;

        [SerializeField]
        [Tooltip("Text file containing class labels (one per line)")]
        private TextAsset labelsFile;

        [SerializeField]
        [Tooltip("Input image width expected by the model")]
        private int modelInputWidth = 416;

        [SerializeField]
        [Tooltip("Input image height expected by the model")]
        private int modelInputHeight = 416;

        [SerializeField]
        [Tooltip("Confidence threshold for detections (0-1)")]
        private float confidenceThreshold = 0.5f;

        [Header("Camera Provider")]
        [SerializeField]
        [Tooltip("GameObject with ICameraProvider component (WebCamTextureProvider or HoloLensResearchModeProvider)")]
        private GameObject cameraProviderObject;

        [Header("Visualization")]
        [SerializeField]
        [Tooltip("Canvas for rendering 2D bounding boxes")]
        private Canvas detectionCanvas;

        [SerializeField]
        [Tooltip("Prefab for bounding box UI (optional)")]
        private GameObject boundingBoxPrefab;

        [SerializeField]
        [Tooltip("Run inference every N frames (1 = every frame)")]
        private int inferenceInterval = 5;

        // Private fields
        private ICameraProvider cameraProvider;
        private IWorker worker;
        private Model runtimeModel;
        private string[] labels;
        private List<Detection> currentDetections = new List<Detection>();
        private List<GameObject> boundingBoxObjects = new List<GameObject>();
        private int frameCount = 0;
        private bool isInitialized = false;

        // Detection result structure
        [System.Serializable]
        public class Detection
        {
            public string label;
            public float confidence;
            public Rect boundingBox; // Normalized coordinates (0-1)
            public Vector3 worldPosition; // Optional 3D world position

            public Detection(string label, float confidence, Rect boundingBox)
            {
                this.label = label;
                this.confidence = confidence;
                this.boundingBox = boundingBox;
                this.worldPosition = Vector3.zero;
            }
        }

        void Start()
        {
            InitializeDetectionSystem();
        }

        void Update()
        {
            if (!isInitialized || cameraProvider == null || !cameraProvider.IsRunning())
            {
                return;
            }

            // Run inference at specified interval
            frameCount++;
            if (frameCount % inferenceInterval == 0)
            {
                RunInference();
            }

            // Update visualization
            UpdateDetectionVisualization();
        }

        private void InitializeDetectionSystem()
        {
            // Load labels
            LoadLabels();

            // Get camera provider
            if (cameraProviderObject != null)
            {
                cameraProvider = cameraProviderObject.GetComponent<ICameraProvider>();
                if (cameraProvider == null)
                {
                    Debug.LogError("Camera provider object does not have an ICameraProvider component");
                    return;
                }
            }
            else
            {
                Debug.LogError("Camera provider object not assigned");
                return;
            }

            // Load model
            if (modelAsset != null)
            {
                try
                {
                    runtimeModel = ModelLoader.Load(modelAsset);
                    worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, runtimeModel);
                    Debug.Log($"Model loaded successfully: {runtimeModel.name}");
                    Debug.Log($"Model inputs: {string.Join(", ", runtimeModel.inputs)}");
                    Debug.Log($"Model outputs: {string.Join(", ", runtimeModel.outputs)}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load model: {e.Message}");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("No model asset assigned. Add a valid ONNX model to enable detection.");
            }

            // Start camera provider
            if (cameraProvider != null)
            {
                cameraProvider.StartProvider();
                Debug.Log("Camera provider started");
            }

            isInitialized = true;
            Debug.Log("Object Detection System initialized");
        }

        private void LoadLabels()
        {
            if (labelsFile != null)
            {
                labels = labelsFile.text.Split('\n');
                // Clean up labels (remove empty lines and trim whitespace)
                List<string> cleanedLabels = new List<string>();
                foreach (string label in labels)
                {
                    string trimmed = label.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        cleanedLabels.Add(trimmed);
                    }
                }
                labels = cleanedLabels.ToArray();
                Debug.Log($"Loaded {labels.Length} class labels");
            }
            else
            {
                Debug.LogWarning("No labels file assigned");
                labels = new string[0];
            }
        }

        private void RunInference()
        {
            if (worker == null || cameraProvider == null)
            {
                return;
            }

            Texture2D frame = cameraProvider.GetLatestFrame();
            if (frame == null)
            {
                return;
            }

            try
            {
                // Prepare input tensor
                Tensor inputTensor = PrepareInputTensor(frame);

                // Run inference
                worker.Execute(inputTensor);

                // Get output
                Tensor output = worker.PeekOutput();

                // Process output (TODO: Adapt for specific model output format)
                currentDetections = PostProcessOutput(output, frame.width, frame.height);

                // Cleanup
                inputTensor.Dispose();
                output.Dispose();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Inference failed: {e.Message}");
            }
        }

        private Tensor PrepareInputTensor(Texture2D frame)
        {
            // Resize frame to model input size
            Texture2D resizedFrame = ResizeTexture(frame, modelInputWidth, modelInputHeight);

            // Convert to tensor (normalized 0-1 range, RGB format)
            Tensor inputTensor = new Tensor(resizedFrame, channels: 3);

            Destroy(resizedFrame);

            return inputTensor;
        }

        private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
            rt.filterMode = FilterMode.Bilinear;

            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        private List<Detection> PostProcessOutput(Tensor output, int frameWidth, int frameHeight)
        {
            // TODO: Adapt this method for the specific ONNX model output format
            // Different models (YOLO, SSD, Faster R-CNN, etc.) have different output formats
            // This is a placeholder implementation

            List<Detection> detections = new List<Detection>();

            // Example placeholder processing
            // Real implementation depends on model architecture
            // Common formats:
            // - YOLO: Grid-based predictions [batch, num_detections, 5+num_classes]
            // - SSD: [batch, num_detections, 7] where each detection is [image_id, label, conf, x1, y1, x2, y2]
            // - Faster R-CNN: Multiple outputs for boxes, scores, and classes

            Debug.Log($"Output tensor shape: [{string.Join(", ", output.shape)}]");

            // Placeholder: Return empty detections list
            // Developer must implement proper post-processing for their chosen model

            return detections;
        }

        private void UpdateDetectionVisualization()
        {
            if (detectionCanvas == null)
            {
                return;
            }

            // Clear existing bounding boxes
            foreach (GameObject box in boundingBoxObjects)
            {
                Destroy(box);
            }
            boundingBoxObjects.Clear();

            // Create new bounding boxes for current detections
            foreach (Detection detection in currentDetections)
            {
                if (detection.confidence < confidenceThreshold)
                {
                    continue;
                }

                // Create bounding box visualization
                GameObject boxObj = CreateBoundingBox(detection);
                if (boxObj != null)
                {
                    boundingBoxObjects.Add(boxObj);
                }
            }
        }

        private GameObject CreateBoundingBox(Detection detection)
        {
            if (boundingBoxPrefab != null)
            {
                GameObject box = Instantiate(boundingBoxPrefab, detectionCanvas.transform);
                
                // Position and scale the box based on normalized coordinates
                RectTransform rectTransform = box.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    PositionBoundingBox(rectTransform, detection.boundingBox);
                }

                // Update label text if available
                Text labelText = box.GetComponentInChildren<Text>();
                if (labelText != null)
                {
                    labelText.text = $"{detection.label}: {detection.confidence:F2}";
                }

                return box;
            }
            else
            {
                // Create simple debug visualization using UI elements
                GameObject box = new GameObject($"Detection_{detection.label}");
                box.transform.SetParent(detectionCanvas.transform, false);

                RectTransform rectTransform = box.AddComponent<RectTransform>();
                Image image = box.AddComponent<Image>();
                image.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green

                PositionBoundingBox(rectTransform, detection.boundingBox);

                // Add label
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(box.transform, false);
                Text labelText = labelObj.AddComponent<Text>();
                labelText.text = $"{detection.label}: {detection.confidence:F2}";
                labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                labelText.fontSize = 14;
                labelText.color = Color.white;
                labelText.alignment = TextAnchor.UpperLeft;

                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, 1);
                labelRect.anchorMax = new Vector2(0, 1);
                labelRect.pivot = new Vector2(0, 1);
                labelRect.anchoredPosition = Vector2.zero;

                return box;
            }
        }

        private void PositionBoundingBox(RectTransform rectTransform, Rect normalizedBox)
        {
            // Convert normalized coordinates (0-1) to canvas coordinates
            RectTransform canvasRect = detectionCanvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            Vector2 position = new Vector2(
                normalizedBox.x * canvasWidth,
                (1 - normalizedBox.y) * canvasHeight // Flip Y coordinate
            );

            Vector2 size = new Vector2(
                normalizedBox.width * canvasWidth,
                normalizedBox.height * canvasHeight
            );

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
        }

        /// <summary>
        /// Convert a 2D detection to a 3D world position using camera-to-world transformation
        /// TODO: Implement based on HoloLens spatial mapping and camera intrinsics
        /// </summary>
        public Vector3 ConvertDetectionToWorldSpace(Detection detection, Camera arCamera)
        {
            // Placeholder implementation
            // Real implementation would use:
            // 1. Camera intrinsics (focal length, principal point)
            // 2. Depth information (from Research Mode depth sensor)
            // 3. Camera pose (position and rotation in world space)
            
            // Example basic implementation (needs depth):
            // Ray ray = arCamera.ViewportPointToRay(new Vector3(
            //     detection.boundingBox.center.x,
            //     detection.boundingBox.center.y,
            //     0
            // ));
            // Vector3 worldPos = ray.GetPoint(estimatedDepth);

            return Vector3.zero;
        }

        /// <summary>
        /// Get current detections for external access
        /// </summary>
        public List<Detection> GetCurrentDetections()
        {
            return new List<Detection>(currentDetections);
        }

        private void OnDestroy()
        {
            // Cleanup
            if (worker != null)
            {
                worker.Dispose();
                worker = null;
            }

            if (cameraProvider != null)
            {
                cameraProvider.StopProvider();
            }

            foreach (GameObject box in boundingBoxObjects)
            {
                if (box != null)
                {
                    Destroy(box);
                }
            }
            boundingBoxObjects.Clear();
        }

        private void OnDisable()
        {
            if (cameraProvider != null && cameraProvider.IsRunning())
            {
                cameraProvider.StopProvider();
            }
        }
    }
}
