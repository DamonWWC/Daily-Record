using System.IO;

namespace Flyleaf.Configuration
{
    /// <summary>
    /// Configuration class for the video player application
    /// </summary>
    public class VideoPlayerConfig
    {
        private static readonly Lazy<VideoPlayerConfig> _instance = new(() => new VideoPlayerConfig());
        public static VideoPlayerConfig Instance => _instance.Value;

        #region Properties

        /// <summary>
        /// Default video URLs to try
        /// </summary>
        public List<string> DefaultVideoUrls { get; set; } = new()
        {
            "rtmp://ns8.indexforce.com/home/mystream1",
            "站台扶梯1.mp4"
        };

        /// <summary>
        /// Playback delay in milliseconds
        /// </summary>
        public int PlaybackDelayMs { get; set; } = 200;

        /// <summary>
        /// Enable automatic retry on playback failure
        /// </summary>
        public bool EnableAutoRetry { get; set; } = true;

        /// <summary>
        /// Maximum retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Retry delay in milliseconds
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Enable debug logging
        /// </summary>
        public bool EnableDebugLogging { get; set; } = true;

        /// <summary>
        /// Default snapshot directory
        /// </summary>
        public string SnapshotDirectory { get; set; } = "Snapshots";

        /// <summary>
        /// Snapshot file name format
        /// </summary>
        public string SnapshotFileNameFormat { get; set; } = "snapshot_{0:yyyyMMdd_HHmmss}.png";

        /// <summary>
        /// Window settings
        /// </summary>
        public WindowSettings Window { get; set; } = new();

        /// <summary>
        /// FlyleafLib DLL path (relative to application directory)
        /// </summary>
        public string FlyleafLibPath { get; set; } = "FlyleafLib1.dll";

        #endregion

        #region Methods

        /// <summary>
        /// Load configuration from file
        /// </summary>
        /// <param name="configPath">Configuration file path</param>
        /// <returns>Loaded configuration or default if file doesn't exist</returns>
        public static VideoPlayerConfig LoadFromFile(string configPath = "appsettings.json")
        {
            //try
            //{
            //    if (File.Exists(configPath))
            //    {
            //        string json = File.ReadAllText(configPath);
            //        var config = JsonSerializer.Deserialize<VideoPlayerConfig>(json, new JsonSerializerOptions
            //        {
            //            PropertyNameCaseInsensitive = true,
            //            WriteIndented = true
            //        });
                    
            //        if (config != null)
            //        {
            //            return config;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
            //}

            return new VideoPlayerConfig();
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        /// <param name="configPath">Configuration file path</param>
        public void SaveToFile(string configPath = "appsettings.json")
        {
            try
            {
                //string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                //{
                //    WriteIndented = true
                //});
                
                //File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Get formatted snapshot file name
        /// </summary>
        /// <returns>Formatted snapshot file name</returns>
        public string GetSnapshotFileName()
        {
            try
            {
                // Ensure snapshot directory exists
                if (!Directory.Exists(SnapshotDirectory))
                {
                    Directory.CreateDirectory(SnapshotDirectory);
                }

                string fileName = string.Format(SnapshotFileNameFormat, DateTime.Now);
                return Path.Combine(SnapshotDirectory, fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating snapshot filename: {ex.Message}");
                return $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            }
        }

        #endregion
    }

    /// <summary>
    /// Window-specific settings
    /// </summary>
    public class WindowSettings
    {
        /// <summary>
        /// Default window width
        /// </summary>
        public double DefaultWidth { get; set; } = 1000;

        /// <summary>
        /// Default window height
        /// </summary>
        public double DefaultHeight { get; set; } = 600;

        /// <summary>
        /// Minimum window width
        /// </summary>
        public double MinWidth { get; set; } = 600;

        /// <summary>
        /// Minimum window height
        /// </summary>
        public double MinHeight { get; set; } = 400;

        /// <summary>
        /// Remember window position and size
        /// </summary>
        public bool RememberWindowState { get; set; } = true;

        /// <summary>
        /// Start in fullscreen mode
        /// </summary>
        public bool StartFullscreen { get; set; } = false;
    }
}