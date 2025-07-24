using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace Flyleaf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Optimized main window with better error handling and resource management
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants and Configuration

        private const string DEFAULT_RTMP_URL = "rtmp://ns8.indexforce.com/home/mystream";
        private const string DEFAULT_LOCAL_FILE = "站台扶梯.mp4";

        #endregion

        #region Properties

        /// <summary>
        /// Current video URL being played
        /// </summary>
        public string? CurrentVideoUrl { get; private set; }

        /// <summary>
        /// Indicates if the application is in the process of loading
        /// </summary>
        public bool IsLoading { get; private set; }

        #endregion

        #region Constructor and Initialization

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Subscribe to events
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            
            // Setup video control event handlers
            SetupVideoControlEvents();
            
            LogInfo("MainWindow initialized");
        }

        /// <summary>
        /// Setup event handlers for the video control
        /// </summary>
        private void SetupVideoControlEvents()
        {
            if (flyleaf != null)
            {
                flyleaf.PlayAction = OnPlaybackStatusChanged;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle window loaded event
        /// </summary>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeVideoPlaybackAsync();
        }

        /// <summary>
        /// Handle window closing event
        /// </summary>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Stop playback before closing
                flyleaf?.StopPlay();
                
                // Dispose resources
                flyleaf?.Dispose();
                
                LogInfo("MainWindow closing - resources cleaned up");
            }
            catch (Exception ex)
            {
                LogError($"Error during window closing: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Handle playback status changes
        /// </summary>
        /// <param name="isPlaying">Current playback status</param>
        private void OnPlaybackStatusChanged(bool isPlaying)
        {
            //try
            //{
            //    // Update UI on the main thread
            //    Dispatcher.BeginInvoke(() =>
            //    {
            //        // Update button text based on playback status
            //        if (sender is System.Windows.Controls.Button button)
            //        {
            //            button.Content = isPlaying ? "Stop" : "Play";
            //        }
                    
            //        // Update window title
            //        string status = isPlaying ? "Playing" : "Stopped";
            //        Title = $"Flyleaf Video Player - {status}";
                    
            //        LogInfo($"Playback status changed: {status}");
            //    });
            //}
            //catch (Exception ex)
            //{
            //    LogError($"Error handling playback status change: {ex.Message}", ex);
            //}
        }

        /// <summary>
        /// Handle stop/play button click
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (flyleaf == null)
                {
                    ShowErrorMessage("Video Control Error", "Video control is not available.");
                    return;
                }

                if (flyleaf.IsPlaying)
                {
                    // Stop playback
                    bool success = flyleaf.StopPlay();
                    if (success)
                    {
                        LogInfo("Playback stopped by user");
                        UpdatePlayStopButton(false);
                        UpdateStatusText("Playback stopped");
                    }
                    else
                    {
                        ShowErrorMessage("Stop Error", "Failed to stop video playback.");
                    }
                }
                else
                {
                    // Start playback
                    if (!string.IsNullOrEmpty(CurrentVideoUrl))
                    {
                        UpdateStatusText("Starting playback...");
                        bool success = await flyleaf.StartPlayAsync(CurrentVideoUrl);
                        if (success)
                        {
                            LogInfo("Playback started by user");
                            UpdatePlayStopButton(true);
                            UpdateStatusText($"Playing: {CurrentVideoUrl}");
                        }
                        else
                        {
                            ShowErrorMessage("Play Error", "Failed to start video playback.");
                            UpdateStatusText("Failed to start playback");
                        }
                    }
                    else
                    {
                        ShowErrorMessage("No Video", "No video URL is available to play.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in button click handler: {ex.Message}", ex);
                ShowErrorMessage("Unexpected Error", $"An unexpected error occurred: {ex.Message}");
                UpdateStatusText("Error occurred");
            }
        }

        /// <summary>
        /// Handle snapshot button click
        /// </summary>
        private void SnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (flyleaf == null)
                {
                    ShowErrorMessage("Video Control Error", "Video control is not available.");
                    return;
                }

                if (!flyleaf.IsPlaying)
                {
                    ShowErrorMessage("Snapshot Error", "Cannot take snapshot - no video is playing.");
                    return;
                }

                string fileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                bool success = flyleaf.TakeSnapshot(fileName);
                
                if (success)
                {
                    UpdateStatusText($"Snapshot saved: {fileName}");
                    LogInfo($"Snapshot taken: {fileName}");
                }
                else
                {
                    ShowErrorMessage("Snapshot Error", "Failed to take snapshot.");
                    UpdateStatusText("Snapshot failed");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error taking snapshot: {ex.Message}", ex);
                ShowErrorMessage("Snapshot Error", $"Failed to take snapshot: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle load URL button click
        /// </summary>
        private async void LoadUrlButton_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    string newUrl = UrlTextBox?.Text?.Trim() ?? "";
                
            //    if (string.IsNullOrWhiteSpace(newUrl))
            //    {
            //        ShowErrorMessage("Invalid URL", "Please enter a valid video URL.");
            //        return;
            //    }

            //    UpdateStatusText($"Loading: {newUrl}");
            //    bool success = await ChangeVideoSourceAsync(newUrl);
                
            //    if (success)
            //    {
            //        UpdateStatusText($"Loaded: {newUrl}");
            //        UpdatePlayStopButton(true);
            //    }
            //    else
            //    {
            //        UpdateStatusText("Failed to load URL");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogError($"Error loading URL: {ex.Message}", ex);
            //    ShowErrorMessage("Load Error", $"Failed to load URL: {ex.Message}");
            //    UpdateStatusText("Load failed");
            //}
        }

        /// <summary>
        /// Handle fullscreen button click
        /// </summary>
        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WindowState == WindowState.Normal)
                {
                    WindowState = WindowState.Maximized;
                    WindowStyle = WindowStyle.None;
                    LogInfo("Entered fullscreen mode");
                }
                else
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    LogInfo("Exited fullscreen mode");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error toggling fullscreen: {ex.Message}", ex);
            }
        }

        #endregion

        #region Video Playback Methods

        /// <summary>
        /// Initialize video playback with default URL
        /// </summary>
        private async Task InitializeVideoPlaybackAsync()
        {
            if (flyleaf == null)
            {
                ShowErrorMessage("Initialization Error", "Video control is not available.");
                return;
            }

            try
            {
                IsLoading = true;
                LogInfo("Initializing video playback...");

                // Set the default video URL
                CurrentVideoUrl = DEFAULT_RTMP_URL;

                // Start playback
                bool success = await flyleaf.StartPlayAsync(CurrentVideoUrl);
                
                if (success)
                {
                    LogInfo($"Video playback initialized successfully with URL: {CurrentVideoUrl}");
                }
                else
                {
                    LogError("Failed to initialize video playback");
                    
                    // Try fallback to local file
                    await TryFallbackPlayback();
                }
            }
            catch (Exception ex)
            {
                LogError($"Exception during video playback initialization: {ex.Message}", ex);
                ShowErrorMessage("Initialization Error", $"Failed to initialize video playback: {ex.Message}");
                
                // Try fallback
                await TryFallbackPlayback();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Try fallback playback with local file
        /// </summary>
        private async Task TryFallbackPlayback()
        {
            try
            {
                LogInfo("Trying fallback playback with local file...");
                CurrentVideoUrl = DEFAULT_LOCAL_FILE;
                
                bool success = await flyleaf.StartPlayAsync(CurrentVideoUrl);
                if (success)
                {
                    LogInfo($"Fallback playback successful with: {CurrentVideoUrl}");
                }
                else
                {
                    LogError("Fallback playback also failed");
                    ShowErrorMessage("Playback Error", "Failed to start video playback with both remote and local sources.");
                }
            }
            catch (Exception ex)
            {
                LogError($"Exception during fallback playback: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Change video source
        /// </summary>
        /// <param name="newUrl">New video URL</param>
        public async Task<bool> ChangeVideoSourceAsync(string newUrl)
        {
            if (flyleaf == null || string.IsNullOrWhiteSpace(newUrl))
            {
                return false;
            }

            try
            {
                // Stop current playback
                flyleaf.StopPlay();
                
                // Update URL and start new playback
                CurrentVideoUrl = newUrl;
                bool success = await flyleaf.StartPlayAsync(newUrl);
                
                if (success)
                {
                    LogInfo($"Video source changed to: {newUrl}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                LogError($"Error changing video source: {ex.Message}", ex);
                return false;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Log information message
        /// </summary>
        private void LogInfo(string message)
        {
            Debug.WriteLine($"[MainWindow] INFO: {message}");
        }

        /// <summary>
        /// Log error message
        /// </summary>
        private void LogError(string message, Exception? ex = null)
        {
            Debug.WriteLine($"[MainWindow] ERROR: {message}");
            if (ex != null)
            {
                Debug.WriteLine($"[MainWindow] EXCEPTION: {ex}");
            }
        }

        /// <summary>
        /// Show error message to user
        /// </summary>
        private void ShowErrorMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Update the play/stop button appearance
        /// </summary>
        private void UpdatePlayStopButton(bool isPlaying)
        {
            //try
            //{
            //    Dispatcher.BeginInvoke(() =>
            //    {
            //        if (PlayStopButton != null)
            //        {
            //            PlayStopButton.Content = isPlaying ? "⏸️ Stop" : "▶️ Play";
            //        }
                    
            //        if (PlaybackStatusText != null)
            //        {
            //            PlaybackStatusText.Text = isPlaying ? "Playing" : "Stopped";
            //        }
            //    });
            //}
            //catch (Exception ex)
            //{
            //    LogError($"Error updating play/stop button: {ex.Message}", ex);
            //}
        }

        /// <summary>
        /// Update the status text
        /// </summary>
        private void UpdateStatusText(string message)
        {
            //try
            //{
            //    Dispatcher.BeginInvoke(() =>
            //    {
            //        if (StatusText != null)
            //        {
            //            StatusText.Text = message;
            //        }
            //    });
            //}
            //catch (Exception ex)
            //{
            //    LogError($"Error updating status text: {ex.Message}", ex);
            //}
        }

        #endregion
    }
}
