# HoloLens2ForCV Integration Folder

This folder is intended to contain camera provider scripts and native components from the [microsoft/HoloLens2ForCV](https://github.com/microsoft/HoloLens2ForCV) repository.

## What to Copy Here

### From HoloLens2ForCV Repository

1. **C# Scripts** from one of the sample projects:
   ```
   Source: HoloLens2ForCV/Samples/[StreamRecorder or ComputeOnDevice]/Unity/Assets/Scripts/HoloLensForCV/
   
   Required files:
   - CameraResources.cs
   - SensorFrameStreamer.cs  
   - MediaFrameReaderController.cs
   - Any related helper/utility scripts
   ```

2. **Native Plugins** (after building the native components):
   ```
   Source: Build output from HoloLens2ForCV native projects
   Destination: Assets/Plugins/WSA/ARM64/
   
   Required:
   - Research Mode DLLs
   - WinMD metadata files
   ```

## Current Status

Currently this folder contains:
- `HoloLensResearchModeProvider.cs` - A placeholder implementation with TODO comments

## Integration Steps

See the main integration guide at: `Assets/README_HoloLens2ForCV_Integration.md`

Quick steps:
1. Clone the HoloLens2ForCV repository
2. Copy required scripts from a sample project (StreamRecorder recommended)
3. Build native components or use pre-built binaries
4. Enable ENABLE_WINMD_SUPPORT in Player Settings
5. Update HoloLensResearchModeProvider.cs to use the imported components

## Why Not Include These Files?

The HoloLens2ForCV repository is:
- Actively maintained by Microsoft
- Contains native code that must be built for specific configurations
- Licensed separately (MIT license - check for compatibility)
- Large binary files that shouldn't be duplicated

Developers should reference the official repository for the latest version.

## References

- HoloLens2ForCV: https://github.com/microsoft/HoloLens2ForCV
- Research Mode Documentation: https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/research-mode
