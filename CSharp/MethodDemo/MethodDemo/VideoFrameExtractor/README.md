# Video Frame Extractor

This project provides utilities for capturing frames from video sources, including RTSP streams and local video files.

## CaptureImage1 Class

The `CaptureImage1` class provides functionality to capture frames from video sources without using external FFmpeg executables. It uses OpenCvSharp for video processing.

### Features

1. Capture a single frame from a video source (RTSP stream or local file)
2. Capture multiple frames at fixed intervals
3. Support for both real-time video streams (RTSP, HTTP) and local video files (MP4, AVI, etc.)

### Requirements

To use the `CaptureImage1` class, you need to add the OpenCvSharp NuGet package to your project:

```
Install-Package OpenCvSharp4
Install-Package OpenCvSharp4.runtime.win
```

### Usage Examples

#### Capture a single frame from an RTSP stream

```csharp
using var captureImage = new CaptureImage1();
string rtspUrl = "rtsp://username:password@camera-ip:554/stream";
string outputPath = "output/frame.jpg";
bool success = captureImage.CaptureFrame(rtspUrl, outputPath);
```

#### Capture a frame at a specific time position from a local video file

```csharp
using var captureImage = new CaptureImage1();
string videoPath = "videos/sample.mp4";
string outputPath = "output/frame_10s.jpg";
bool success = captureImage.CaptureFrame(videoPath, outputPath, timePosition: 10.0);
```

#### Capture multiple frames at regular intervals from an RTSP stream

```csharp
using var captureImage = new CaptureImage1();
string rtspUrl = "rtsp://username:password@camera-ip:554/stream";
string outputFolder = "output/frames";
double interval = 1.0; // 1 second interval
double duration = 10.0; // Capture for 10 seconds
int count = await captureImage.CaptureMultipleFramesAsync(rtspUrl, outputFolder, interval, duration);
```

#### Capture multiple frames at regular intervals from a local video file

```csharp
using var captureImage = new CaptureImage1();
string videoPath = "videos/sample.mp4";
string outputFolder = "output/frames";
double interval = 1.0; // 1 second interval
int count = await captureImage.CaptureMultipleFramesAsync(videoPath, outputFolder, interval);