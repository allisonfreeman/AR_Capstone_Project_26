# HoloLens2ForCV Object Detection Integration Guide

This document provides step-by-step instructions for integrating object detection capabilities using Unity Barracuda and HoloLens 2 Research Mode cameras.

## Overview

This integration adds object detection to your Unity AR project using:
- **Unity Barracuda**: For ML model inference
- **WebCamTexture**: For Editor and non-HoloLens testing
- **HoloLens2ForCV**: For accessing HoloLens 2 Research Mode cameras (optional)

## Prerequisites

- Unity 2023.2+ (6000.x series recommended for this project)
- Unity Barracuda package (automatically added via manifest.json)
- HoloLens 2 device (for HoloLens-specific features)
- ONNX object detection model (see Model Selection below)

## Quick Start - Editor Testing

### 1. Add an ONNX Model

The project includes a placeholder model (`Assets/Models/placeholder.nn`). To use real object detection:

1. **Option A - Use a pre-trained ONNX model:**
   - Download a Barracuda-compatible ONNX model (e.g., YOLOv5, SSD, MobileNet)
   - Recommended: [ONNX Model Zoo](https://github.com/onnx/models)
   - Place the `.onnx` file in `Assets/Models/`
   - Unity will automatically import it as an NNModel asset

2. **Option B - Convert your own model:**
   - Use ONNX export from PyTorch, TensorFlow, or other frameworks
   - Ensure compatibility with Unity Barracuda (see Compatibility section below)
   - Place in `Assets/Models/`

### 2. Configure the Scene

1. Open `Assets/Scenes/ObjectDetectionSample.unity`
2. Select the `ObjectDetectionManager` GameObject
3. In the Inspector:
   - Assign your NNModel to the `Model Asset` field
   - Verify `Labels File` is set to `Assets/Models/labels.txt`
   - Adjust `Model Input Width` and `Model Input Height` to match your model
   - Set `Camera Provider Object` to the WebCamTextureProvider GameObject

### 3. Test in Editor

1. Ensure you have a webcam connected
2. Press Play in the Unity Editor
3. The webcam feed will start automatically
4. Object detections will appear as bounding boxes on the canvas

**Note:** The placeholder model won't produce detections. Use a real model for testing.

## HoloLens 2 Deployment

### 1. Import HoloLens2ForCV Components

The HoloLens Research Mode provider requires additional components from microsoft/HoloLens2ForCV:

#### A. Clone HoloLens2ForCV Repository

```bash
git clone https://github.com/microsoft/HoloLens2ForCV.git
```

#### B. Copy Required Files

Copy the following from HoloLens2ForCV to your Unity project:

**C# Scripts** (from Samples/[StreamRecorder or ComputeOnDevice]/Unity/Assets/Scripts/):
```
HoloLens2ForCV/Samples/[Sample]/Unity/Assets/Scripts/HoloLensForCV/
  -> Copy to: Assets/Plugins/HoloLens2ForCV/
  
Required files:
- CameraResources.cs
- SensorFrameStreamer.cs
- MediaFrameReaderController.cs
- And any related helper scripts
```

**Native Plugins** (from the HoloLens2ForCV build output):
```
Build output ARM64 DLLs/WinMD files
  -> Copy to: Assets/Plugins/WSA/ARM64/
```

#### C. Build Native Components (if needed)

If pre-built binaries aren't available, build the HoloLens2ForCV native components:

1. Open HoloLens2ForCV solution in Visual Studio
2. Build for ARM64, Release configuration
3. Copy output DLLs to `Assets/Plugins/WSA/ARM64/`

### 2. Configure Unity Player Settings

**File > Build Settings > Player Settings:**

#### Platform Settings
- **Platform:** Universal Windows Platform (UWP)
- **Target Device:** HoloLens
- **Architecture:** ARM64
- **Build Type:** D3D Project
- **Target SDK Version:** Latest installed
- **Minimum Platform Version:** 10.0.18362.0 or higher

#### XR Settings
- **XR Plug-in Management:** OpenXR
- **Enable:** Microsoft HoloLens feature group

#### Other Settings
- **Scripting Backend:** IL2CPP
- **API Compatibility Level:** .NET Standard 2.1
- **Scripting Define Symbols:** Add `ENABLE_WINMD_SUPPORT`

#### Publishing Settings
- **Capabilities:** Enable the following
  - [x] InternetClient
  - [x] InternetClientServer  
  - [x] PrivateNetworkClientServer
  - [x] WebCam
  - [x] Microphone
  - [x] SpatialPerception

### 3. Switch to HoloLens Camera Provider

In the ObjectDetectionSample scene:

1. Select the `CameraProviderObject` GameObject
2. Remove or disable the `WebCamTextureProvider` component
3. Add the `HoloLensResearchModeProvider` component
4. In the `ObjectDetectionManager` Inspector, ensure `Camera Provider Object` references the correct GameObject

### 4. Build and Deploy

1. **File > Build Settings**
2. Select **Universal Windows Platform**
3. Click **Build** and choose an output folder
4. Open the generated Visual Studio solution
5. Set configuration to **Release** and **ARM64**
6. Deploy to HoloLens 2 via:
   - USB connection, or
   - Device Portal, or
   - Remote Machine deployment

### 5. Run on HoloLens 2

1. Launch the app on HoloLens 2
2. Grant camera permissions when prompted
3. Object detections will appear as holograms in your view

## Model Selection and Compatibility

### Barracuda-Compatible Models

Unity Barracuda supports a subset of ONNX operations. Compatible models include:

- **YOLOv5/YOLOv7** (small variants recommended)
- **SSD MobileNet**
- **EfficientDet Lite**
- **Custom models using supported ops**

### Recommended Models

For HoloLens 2, prioritize small, efficient models:

| Model | Input Size | Performance | Use Case |
|-------|------------|-------------|----------|
| YOLOv5s | 416x416 | Good | General object detection |
| YOLOv5n | 416x416 | Excellent | Real-time, multiple objects |
| SSD MobileNetV2 | 300x300 | Excellent | Mobile/embedded |
| EfficientDet-Lite0 | 320x320 | Very Good | Balanced accuracy/speed |

### Model Conversion

If your model isn't ONNX format:

**From PyTorch:**
```python
import torch
model = torch.load('model.pth')
dummy_input = torch.randn(1, 3, 416, 416)
torch.onnx.export(model, dummy_input, "model.onnx", opset_version=9)
```

**From TensorFlow:**
```python
import tf2onnx
import onnx
spec = (tf.TensorSpec((None, 416, 416, 3), tf.float32, name="input"),)
output_path = "model.onnx"
model_proto, _ = tf2onnx.convert.from_keras(model, input_signature=spec, output_path=output_path)
```

### Verify Barracuda Compatibility

After importing the ONNX model to Unity:

1. Select the NNModel asset in the Project window
2. Check the Inspector for import warnings
3. Unsupported layers will be listed
4. Refer to [Barracuda documentation](https://docs.unity3d.com/Packages/com.unity.barracuda@3.0/manual/index.html) for supported operations

## Post-Processing Implementation

The `ObjectDetectionManager.PostProcessOutput()` method requires model-specific implementation.

### Example: YOLOv5 Post-Processing

```csharp
private List<Detection> PostProcessOutput(Tensor output, int frameWidth, int frameHeight)
{
    List<Detection> detections = new List<Detection>();
    
    // YOLOv5 output shape: [1, 25200, 85] for 80 classes
    // Format: [x_center, y_center, width, height, objectness, class_0_prob, ..., class_79_prob]
    
    int numDetections = output.shape[1];
    int numClasses = output.shape[2] - 5;
    
    for (int i = 0; i < numDetections; i++)
    {
        float objectness = output[0, i, 4];
        
        if (objectness < confidenceThreshold)
            continue;
            
        // Find best class
        float maxClassScore = 0;
        int bestClass = 0;
        for (int c = 0; c < numClasses; c++)
        {
            float classScore = output[0, i, 5 + c];
            if (classScore > maxClassScore)
            {
                maxClassScore = classScore;
                bestClass = c;
            }
        }
        
        float confidence = objectness * maxClassScore;
        if (confidence < confidenceThreshold)
            continue;
            
        // Convert from normalized coordinates
        float x = output[0, i, 0];
        float y = output[0, i, 1];
        float w = output[0, i, 2];
        float h = output[0, i, 3];
        
        // Create detection
        Rect bbox = new Rect(
            (x - w/2),  // left
            (y - h/2),  // top
            w,          // width
            h           // height
        );
        
        string label = (bestClass < labels.Length) ? labels[bestClass] : $"Class_{bestClass}";
        detections.Add(new Detection(label, confidence, bbox));
    }
    
    // Apply Non-Maximum Suppression (NMS) to remove duplicate detections
    detections = ApplyNMS(detections, 0.45f);
    
    return detections;
}
```

### Example: SSD Post-Processing

```csharp
private List<Detection> PostProcessOutput(Tensor output, int frameWidth, int frameHeight)
{
    List<Detection> detections = new List<Detection>();
    
    // SSD output shape typically: [1, num_detections, 7]
    // Format: [image_id, label, confidence, x_min, y_min, x_max, y_max]
    
    int numDetections = output.shape[1];
    
    for (int i = 0; i < numDetections; i++)
    {
        float confidence = output[0, i, 2];
        
        if (confidence < confidenceThreshold)
            continue;
            
        int classId = (int)output[0, i, 1];
        float xMin = output[0, i, 3];
        float yMin = output[0, i, 4];
        float xMax = output[0, i, 5];
        float yMax = output[0, i, 6];
        
        Rect bbox = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        string label = (classId < labels.Length) ? labels[classId] : $"Class_{classId}";
        
        detections.Add(new Detection(label, confidence, bbox));
    }
    
    return detections;
}
```

## Converting Detections to 3D World Space

To place virtual objects at detected locations in HoloLens:

```csharp
public Vector3 ConvertDetectionToWorldSpace(Detection detection, Camera arCamera, float estimatedDepth)
{
    // Get center of detection in viewport coordinates (0-1 range)
    Vector3 viewportPoint = new Vector3(
        detection.boundingBox.center.x,
        detection.boundingBox.center.y,
        estimatedDepth
    );
    
    // Convert to world space
    Vector3 worldPosition = arCamera.ViewportToWorldPoint(viewportPoint);
    
    return worldPosition;
}
```

For better accuracy on HoloLens 2:
1. Use Research Mode depth sensor data
2. Access camera intrinsics for precise projection
3. Combine with spatial mapping for collision

## Troubleshooting

### Issue: "No webcam devices found"
**Solution:** Check that a webcam is connected and accessible. Check Unity permissions.

### Issue: Model import fails
**Solution:** 
- Verify ONNX model is compatible with Barracuda
- Check Unity console for specific unsupported operations
- Try a different model or convert to supported ops

### Issue: Black screen on HoloLens 2
**Solution:**
- Ensure webcam capability is enabled in manifest
- Grant camera permissions when app launches
- Check that Research Mode components are properly integrated

### Issue: Poor performance / low FPS
**Solution:**
- Use smaller model (YOLOv5n, MobileNet)
- Reduce input resolution
- Increase `inferenceInterval` in ObjectDetectionManager
- Build Release configuration (not Debug)

### Issue: No detections appearing
**Solution:**
- Verify model is correctly loaded (check console logs)
- Implement proper post-processing for your model type
- Check confidence threshold isn't too high
- Ensure labels file matches model output classes

## Performance Tips

1. **Model Size:** Smaller models = faster inference
2. **Input Resolution:** 416x416 or lower recommended
3. **Inference Interval:** Run inference every 5-10 frames, not every frame
4. **Build Configuration:** Always use Release mode for deployment
5. **Barracuda Backend:** ComputePrecompiled provides best performance

## Additional Resources

- [Unity Barracuda Documentation](https://docs.unity3d.com/Packages/com.unity.barracuda@3.0/manual/index.html)
- [HoloLens2ForCV Repository](https://github.com/microsoft/HoloLens2ForCV)
- [ONNX Model Zoo](https://github.com/onnx/models)
- [Mixed Reality Toolkit](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/)
- [HoloLens 2 Research Mode](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/research-mode)

## License and Attribution

- Unity Barracuda: Unity Technologies
- HoloLens2ForCV: Microsoft (MIT License)
- This integration: Follow your project's license

## Support

For issues specific to:
- **Barracuda:** [Unity Forums](https://forum.unity.com/)
- **HoloLens2ForCV:** [GitHub Issues](https://github.com/microsoft/HoloLens2ForCV/issues)
- **This integration:** Open an issue in this repository
