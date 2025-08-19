# CaptureImage2 - Flyleaf Video Frame Extractor

CaptureImage2 is a C# class that uses the Flyleaf video player library to extract frames from video sources, including real-time RTSP streams and local video files.

## Features

1. **Single Frame Capture**: Extract a specific frame from a video at a given timestamp
2. **Multiple Frame Capture**: Extract frames at regular intervals from video sources
3. **RTSP Stream Support**: Capture frames from live RTSP video streams
4. **Local Video Support**: Extract frames from local video files (MP4, AVI, MKV, etc.)
5. **Flexible Output**: Support for different image formats (JPEG, PNG, BMP)
6. **Frame Resizing**: Optional resizing of captured frames
7. **Asynchronous Operations**: Non-blocking frame capture operations
8. **Connection Testing**: Test connectivity to video sources before capture

## Dependencies

- **FlyleafLib**: The main video player library (v3.7.64)
- **System.Drawing**: For bitmap operations
- **.NET 8.0**: Target framework

## Installation

The FlyleafLib package is automatically included in the project via NuGet:

```xml
<PackageReference Include="FlyleafLib" Version="3.7.64" />
```

## Usage Examples

### Basic Single Frame Capture

```csharp
using var captureImage = new CaptureImage2();

// Capture from RTSP stream
bool success = await captureImage.CaptureFrameAsync(
    "rtsp://example.com/stream", 
    "output/frame.jpg", 
    timePosition: 0, 
    timeoutSeconds: 30
);

// Capture from local video file at specific time
bool success = await captureImage.CaptureFrameAsync(
    "video.mp4", 
    "output/frame_5s.jpg", 
    timePosition: 5.0, 
    timeoutSeconds: 30
);
```

### Multiple Frame Capture

```csharp
using var captureImage = new CaptureImage2();

// Capture frames from RTSP stream every 2 seconds for 10 seconds
int frameCount = await captureImage.CaptureMultipleFramesAsync(
    "rtsp://example.com/stream",
    "output/frames/",
    captureInterval: 2.0,
    totalDuration: 10.0,
    timeoutSeconds: 30
);

// Extract frames from local video every 1 second
int frameCount = await captureImage.CaptureMultipleFramesAsync(
    "video.mp4",
    "output/frames/",
    captureInterval: 1.0,
    totalDuration: 0, // Use entire video
    startPosition: 0,
    timeoutSeconds: 30
);
```

### Connection Testing

```csharp
using var captureImage = new CaptureImage2();

// Test RTSP stream connection
bool isConnected = captureImage.TestConnection("rtsp://example.com/stream", 10);

// Test local file access
bool fileExists = captureImage.TestConnection("video.mp4", 5);
```

### Frame Resizing

```csharp
using var captureImage = new CaptureImage2();

// Capture and resize frame to 1280x720
bool success = await captureImage.CaptureFrameAsync(
    "video.mp4", 
    "output/resized_frame.jpg", 
    timePosition: 10.0,
    timeoutSeconds: 30,
    width: 1280,
    height: 720
);
```

## Method Reference

### CaptureFrame / CaptureFrameAsync

Captures a single frame from a video source.

**Parameters:**
- `videoSource`: Video source URL (RTSP) or file path
- `outputPath`: Output image file path
- `timePosition`: Time position in seconds (default: 0)
- `timeoutSeconds`: Connection timeout in seconds (default: 30)
- `width`: Output image width (0 = original, default: 0)
- `height`: Output image height (0 = original, default: 0)

**Returns:** `bool` - Success status

### CaptureMultipleFramesAsync

Captures multiple frames at regular intervals.

**Parameters:**
- `videoSource`: Video source URL or file path
- `outputFolder`: Output folder path
- `captureInterval`: Interval between captures in seconds
- `totalDuration`: Total capture duration (0 = entire video for files)
- `startPosition`: Start position in seconds (for files only)
- `timeoutSeconds`: Connection timeout in seconds
- `width`: Output image width (0 = original)
- `height`: Output image height (0 = original)
- `cancellationToken`: Cancellation token for stopping operation

**Returns:** `Task<int>` - Number of successfully captured frames

### TestConnection

Tests connectivity to a video source.

**Parameters:**
- `videoSource`: Video source URL or file path
- `timeoutSeconds`: Connection timeout in seconds

**Returns:** `bool` - Connection success status

## Supported Video Sources

### RTSP Streams
- `rtsp://username:password@ip:port/path`
- `rtsp://ip:port/path`

### Local Video Files
- MP4, AVI, MKV, MOV, WMV, FLV
- Any format supported by FFmpeg

### Network Streams
- HTTP/HTTPS video streams
- UDP/RTP streams
- TCP streams

## Output Formats

Supported image formats are determined by file extension:
- `.jpg`, `.jpeg` - JPEG format
- `.png` - PNG format
- `.bmp` - Bitmap format
- Default: JPEG

## Error Handling

The class includes comprehensive error handling:
- Connection timeouts for streams
- Invalid file paths
- Unsupported video formats
- Frame extraction failures
- Memory management issues

Errors are logged to console and methods return appropriate failure indicators.

## Performance Considerations

1. **Memory Usage**: Frames are processed and disposed promptly to minimize memory usage
2. **Threading**: Operations are thread-safe with internal locking
3. **Timeouts**: Configurable timeouts prevent hanging on unresponsive sources
4. **Resource Cleanup**: Implements IDisposable for proper resource management

## Limitations

1. **Pixel Format Support**: Currently supports YUV420P and RGB24 formats primarily
2. **Hardware Acceleration**: Software decoding only (no GPU acceleration)
3. **Audio**: Audio streams are disabled for frame extraction
4. **Subtitles**: Subtitle rendering is disabled

## Example Application

Run the example application to test all functionality:

```csharp
await CaptureImage2Example.RunExample();
```

The example demonstrates:
- RTSP stream frame capture
- Local video frame extraction
- Multiple frame capture scenarios
- Error handling and user interaction

## Troubleshooting

### Common Issues

1. **RTSP Connection Failures**
   - Verify URL format and credentials
   - Check network connectivity
   - Increase timeout values
   - Test with VLC or similar player first

2. **Local File Issues**
   - Verify file exists and is accessible
   - Check file format compatibility
   - Ensure sufficient disk space for output

3. **Frame Extraction Failures**
   - Video may be corrupted
   - Unsupported codec
   - Insufficient memory

### Debug Tips

- Enable Flyleaf logging for detailed error information
- Test with known working video sources
- Verify output directory permissions
- Monitor memory usage during batch operations

## License

This implementation uses the Flyleaf library which has its own licensing terms. Please review the Flyleaf license before commercial use.