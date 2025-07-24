using Flyleaf.Common;
using FlyleafLib;
using FlyleafLib.Controls.WPF;
using FlyleafLib.MediaPlayer;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FlyleafLib1.Controls
{
    public class LiveControl : FlyleafHost, ILiveControl
    {
        public Action<Flyleaf.Common.Status>? StatusAction { get; set; }
        public Action OpenCompleted { get; set; }
        static LiveControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LiveControl), new FrameworkPropertyMetadata(typeof(LiveControl)));
        }
        public Config Config { get; set; }

        public LiveControl()
        {
            Engine.Start(new EngineConfig()
            {
#if DEBUG
                LogOutput = ":debug",
                LogLevel = LogLevel.Debug,
                FFmpegLogLevel = FFmpegLogLevel.Warning,
#endif

                PluginsPath = ":Plugins",
                FFmpegPath = ":FFmpeg",

                // Use UIRefresh to update Stats/BufferDuration (and CurTime more frequently than a second)
                UIRefresh = true,
                UIRefreshInterval = 100,
                UICurTimePerSecond = false // If set to true it updates when the actual timestamps second change rather than a fixed interval
            });
            Config = new Config();

            Config.Player.Stats = true;

            Player = new Player(Config);
            Player.OpenCompleted += (o, e) =>
            {
                OpenCompleted?.Invoke();
                //Task.Run(async () =>
                //{
                //    await Task.Delay(3000);
                //    TakeSnapShot(null);
                //});
            };
            Player.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(Player.Status))
                {
                    Flyleaf.Common.Status status = Player.Status switch
                    {
                        FlyleafLib.MediaPlayer.Status.Playing => Flyleaf.Common.Status.Playing,
                        FlyleafLib.MediaPlayer.Status.Paused => Flyleaf.Common.Status.Paused,
                        FlyleafLib.MediaPlayer.Status.Stopped => Flyleaf.Common.Status.Stopped,
                        FlyleafLib.MediaPlayer.Status.Failed => Flyleaf.Common.Status.Failed,
                        FlyleafLib.MediaPlayer.Status.Ended => Flyleaf.Common.Status.Ended,
                        FlyleafLib.MediaPlayer.Status.Opening => Flyleaf.Common.Status.Opening,
                        _=> Flyleaf.Common.Status.Stopped

                    };
                    StatusAction?.Invoke(status);
                }
            };      
        }

        public object GetInstance()
        {
            return this;
        }

        /// <summary>
        /// Live Stream Url
        /// </summary>
        public string CameraUrl
        {
            get { return (string)GetValue(CameraUrlProperty); }
            set { SetValue(CameraUrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CameraUrl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CameraUrlProperty =
            DependencyProperty.Register("CameraUrl", typeof(string), typeof(LiveControl), new PropertyMetadata(default, CamerasChangedCallback));

        private static void CamerasChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as LiveControl;
            if (e.NewValue is string url && !string.IsNullOrWhiteSpace(url))
            {
                control?.PlayLive(url);
            }
            else
            {
                control?.StopLive();
            }
        }

        public void SetCameraUrl(string url)
        {
            CameraUrl = url;
        }
        /// <summary>
        /// Play Live Stream
        /// </summary>
        /// <param name="url"></param>
        public void PlayLive(string url)
        {
            try
            {
                Player?.OpenAsync(url); 
            }
            catch(Exception ex)
            {

            }
        }
        /// <summary>
        /// Stop Live Stream
        /// </summary>
        public void StopLive()
        {
            try
            {
                if (Player != null && Player.IsPlaying)
                {
                    Player?.Stop();
                }
            }
            catch(Exception ex)
            {
              
            }
        }

        public void TakeSnapShot(string fileName)
        {
            Player?.TakeSnapshotToFile(fileName);
        }
    }
}
