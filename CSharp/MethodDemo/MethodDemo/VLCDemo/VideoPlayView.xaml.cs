using LibVLCSharp.Shared;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace VLCDemo
{
    /// <summary>
    /// VideoPlayView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlayView : UserControl, IDisposable
    {
        private static readonly Lazy<LibVLC> _libVlcLazy = new Lazy<LibVLC>(() =>
        {
            //string currentDirectory = AppDomain.CurrentDomain.BaseDirectory + @"VLC";
            //Core.Initialize(currentDirectory);
            return new LibVLC("--rtsp-tcp", "--network-caching=300", "--avcodec-hw=any");
        });

        internal static LibVLC LibVlcInstance => _libVlcLazy.Value;
        private MediaPlayer _player;

        private readonly Subject<Tuple<string, string>> _subject = new Subject<Tuple<string, string>>();

        public string CameraCode
        {
            get { return (string)GetValue(CameraCodeProperty); }
            set { SetValue(CameraCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CameraCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CameraCodeProperty =
            DependencyProperty.Register("CameraCode", typeof(string), typeof(VideoPlayView), new PropertyMetadata(default,
                (o, args) =>
                {
                    var dp = (VideoPlayView)o;
                    if (args.NewValue is string code)
                    {
                        if(string.IsNullOrWhiteSpace(code))
                            ThreadPool.QueueUserWorkItem(_ => dp.StopPlay());
                        else 
                            dp.StartPlay(code);
                        
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(_ => dp.StopPlay());
                    }                 
                }));

        public bool IsPlaying { get; private set; }
        //private Camera CurrentPlayItem { get; set; }

      

        public VideoPlayView()
        {
            InitializeComponent();
            _player = new MediaPlayer(LibVlcInstance);
           
            videoView.MediaPlayer = _player;  
           
            Observable.FromEventPattern<EventArgs>(_player, nameof(MediaPlayer.Playing)).Select(_ => "play")
                .Do(_ =>
                {
                    ChangeTip(string.Empty);                   
                    IsPlaying = true;
                })
                .Merge(Observable.FromEventPattern<EventArgs>(_player, nameof(MediaPlayer.Stopped)).Select(_ => "stop")
                .Do(_ =>
                { 
                    ChangeTip("视频已停止播放",type:"stop");                   
                    IsPlaying = false;
                }))
                .Merge(Observable.FromEventPattern<EventArgs>(_player,nameof(MediaPlayer.EncounteredError)).Select(_=>"error")
                .Do(_ =>
                {
                    IsPlaying = false;
                }))
                .StartWith("stop")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .DistinctUntilChanged()
                .ObserveOnDispatcher()
                .Subscribe(value =>
                {
                    var color = Brushes.Transparent;
                    if (value == "stop")
                    {
                        color = Brushes.Black;
                    }
                    backBorder.Background = color;
                });
            Observable.FromEventPattern<RoutedEventArgs>(fullScreenButton, nameof(fullScreenButton.Click))
                .Select(_ => "full")
                .Merge(Observable
                .FromEventPattern<RoutedEventArgs>(restoreScreenButton, nameof(restoreScreenButton.Click))
                .Select(_ => "restore"))
                .StartWith("restore")
                .Throttle(TimeSpan.FromMilliseconds(200))
                .DistinctUntilChanged()
                .ObserveOnDispatcher()
                .Subscribe(value =>
                {
                    switch(value)
                    {
                        case "full":
                            fullScreenButton.Visibility = Visibility.Collapsed;
                            restoreScreenButton.Visibility = Visibility.Visible;
                            RaiseEvent(new RoutedEventArgs(FullScreenClickEvent, this));
                            break;
                        case"restore":
                            fullScreenButton.Visibility = Visibility.Visible;
                            restoreScreenButton.Visibility = Visibility.Collapsed;
                            RaiseEvent(new RoutedEventArgs(RestoreScreenClickEvent, this));
                            break;
                    }
                });
            _subject
             .Throttle(TimeSpan.FromMilliseconds(500))
             .DistinctUntilChanged()
             .Select( x =>
             {
                 switch (x.Item1)
                 {
                     case "play":
                         if (_player.IsPlaying)
                         {
                             //ThreadPool.QueueUserWorkItem(_ =>
                             //{
                             //    _player.Stop();
                             //});
                             _player.Stop();
                         }
                         
                         ChangeTip("加载视频链接中...",true);

                         //var result = await CctvCameraHelper.GetInstance().GetCamera(x.Item2, null);

                         //if (result == null || string.IsNullOrWhiteSpace(result.Url))
                         //{
                         //    return Tuple.Create(x.Item1, string.Empty, (Camera)null);
                         //}
                         return Tuple.Create(x.Item1, x.Item2);

                     case "stop":
                     default:
                         return Tuple.Create(x.Item1, string.Empty);
                 }
             })          
             .Subscribe( value =>
             {
                 //var newPlayInfo = value.Item3;
                 // 停止播放需要调用一下停止播放接口
                 //if (CurrentPlayItem != null)
                 //{
                 //    if (newPlayInfo == null || newPlayInfo.CameraCode != CurrentPlayItem.CameraCode)
                 //    {
                 //        await CctvCameraHelper.GetInstance().StopPlayAgent(new System.Collections.Generic.List<Camera> { CurrentPlayItem });
                 //    }
                 //}
                 //CurrentPlayItem = newPlayInfo;

                 //Application.Current?.Dispatcher.Invoke(() => tipPanel.ToolTip = $"CamerCode:【{CurrentPlayItem.CameraCode}】Url:【{CurrentPlayItem.Url}】");

                 switch (value.Item1)
                 {
                     case "play":
                         {
                             if (string.IsNullOrWhiteSpace(value.Item2))
                             {
                                 ChangeTip("加载视频链接失败",type: "stop");
                             }
                             else
                             {
                                 ChangeTip("加载视频流中...",true);
                                 ThreadPool.QueueUserWorkItem(_ =>
                                 {                                  
                                     using (var media = new Media(LibVlcInstance, new Uri(value.Item2)))
                                     {
                                         if (!_dispose && Visibility == Visibility.Visible)
                                         {
                                             var isSuccess = _player.Play(media);
                                         }
                                     }
                                 });
                             }
                             break;
                         }
                     case "stop":
                     default:
                         ChangeTip("无视频播放",type:"stop");
                        
                         if (_player.IsPlaying)
                         {
                             ThreadPool.QueueUserWorkItem(_ => _player.Stop());
                         }
                         break;
                 }
             });

            IsVisibleChanged += (_, args) =>
            {
                if (args.NewValue is bool value)
                {
                    backPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    backBorder.Background = value && !_player.IsPlaying ? Brushes.Black : Brushes.Transparent;
                }
            };
          
        }
        public static readonly RoutedEvent FullScreenClickEvent = EventManager.RegisterRoutedEvent(nameof(FullScreenClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VideoPlayView));
        /// <summary>
        /// 全屏按钮点击事件
        /// </summary>
        public event RoutedEventHandler FullScreenClick
        {
            add => AddHandler(FullScreenClickEvent, value);
            remove => RemoveHandler(FullScreenClickEvent, value);
        }

        public static readonly RoutedEvent RestoreScreenClickEvent = EventManager.RegisterRoutedEvent(nameof(RestoreScreenClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VideoPlayView));

        /// <summary>
        /// 还原按钮点击事件
        /// </summary>
        public event RoutedEventHandler RestoreScreenClick
        {
            add => AddHandler(RestoreScreenClickEvent, value);
            remove => RemoveHandler(RestoreScreenClickEvent, value);
        }



        public void StartPlay(string cameraCode)
        {
            if (!_subject.IsDisposed)
            {
                _subject.OnNext(Tuple.Create("play", cameraCode));
            }
        }
        public void StopPlay()
        {
            _subject.OnNext(Tuple.Create("stop", string.Empty));
        }
        private void ChangeTip(string content, bool showloading = false, string type = null)
        {          
            Application.Current?.Dispatcher.Invoke(() => 
            { 
                tip.Text = content; 
                loading.Visibility = showloading ? Visibility.Visible : Visibility.Collapsed;
                Image_NoneVideo.Visibility = type == "stop" ? Visibility.Visible : Visibility.Collapsed;
                tipPanel.Visibility= type == "stop" ? Visibility.Collapsed : Visibility.Visible;
            }); 
        }

        private bool _dispose;

        public void Dispose()
        {
            if (_dispose) return;
            _dispose = true;
            ThreadPool.QueueUserWorkItem(_ => _player.Stop());
            videoView.Dispose();
        }
    }
}