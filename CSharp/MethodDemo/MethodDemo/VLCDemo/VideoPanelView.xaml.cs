using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;

namespace VLCDemo
{
    /// <summary>
    /// VideoPanelView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPanelView : UserControl, IDisposable
    {
        private readonly IDisposable _subscribe;
        private readonly IDisposable _subscribe2;
        private readonly IDisposable _subscribe3;
        private readonly Subject<IObservable< List<string>>> _codeSubject = new Subject<IObservable<List<string>>>();
        private readonly Subject<List<string>> _subject = new Subject< List<string>>();
        private List<VideoPlayView> players = new List<VideoPlayView>();
        private int num;
        private int interval;
        /// <summary>
        /// 相机编号组
        /// </summary>
        public List<string> CameraCodes
        {
            get { return (List<string>)GetValue(CameraCodesProperty); }
            set { SetValue(CameraCodesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CameraCodes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CameraCodesProperty =
            DependencyProperty.Register("CameraCodes", typeof(List<string>), typeof(VideoPanelView), new PropertyMetadata(default,
                (o, args) =>
                {
                    try
                    {
                        var dp = (VideoPanelView)o;
                        if(args.NewValue is List<string> codes)
                        {
                            dp.Play(codes);
                        }

                    }
                    catch(Exception ex)
                    {

                    }
                }));
        public int Row
        {
            get => (int)GetValue(RowProperty);
            set => SetValue(RowProperty, value);
        }

        /// <summary>
        /// 面板分为几行，默认两行
        /// </summary>
        public static readonly DependencyProperty RowProperty =
            DependencyProperty.Register("Row", typeof(int), typeof(VideoPanelView), new PropertyMetadata(2));

        public int Column
        {
            get => (int)GetValue(ColumnProperty);
            set => SetValue(ColumnProperty, value);
        }

        /// <summary>
        /// 面板分为几列，默认两列
        /// </summary>
        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register("Column", typeof(int), typeof(VideoPanelView), new PropertyMetadata(2));



        /// <summary>
        /// 切换间隔
        /// </summary>
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Interval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(VideoPanelView), new PropertyMetadata(5));


        public VideoPanelView()
        {
            InitializeComponent();          
            var loadOrUnload = Observable.FromEventPattern<RoutedEventArgs>(this, nameof(UserControl.Loaded))
              .Select(_ => "loaded")
              .Merge(Observable.FromEventPattern<RoutedEventArgs>(this, nameof(UserControl.Unloaded))
                  .Select(_ => "unloaded"))
              .Throttle(TimeSpan.FromMilliseconds(500))
              .DistinctUntilChanged();
            _subscribe3 = loadOrUnload
                .Skip(1)
                .Subscribe(value =>
                {
                    switch (value)
                    {
                        case "unloaded":
                            Dispose();
                            break;
                    }
                });

            _subscribe2 = _codeSubject.Switch()
                .SkipUntil(loadOrUnload.Where(x => x == "loaded"))
                 .TakeUntil(loadOrUnload.Where(x => x == "unloaded"))
                 .Repeat()
                 .Subscribe(x =>
                 {
                     Application.Current.Dispatcher.Invoke(() =>
                     {
                         try
                         {
                             if (x.Count == 0)
                             {
                                 players.ForEach(player =>
                                 {
                                     player.CameraCode = null;
                                 });
                                 return;
                             }
                             for (var i = 0; i < players.Count; i++)
                             {
                                 players[i].CameraCode = x.Count > i ? x[i] : null;
                             }
                         }
                         catch (Exception ex)
                         {

                         }

                     });
                 });

            _subscribe = _subject.Throttle(TimeSpan.FromMilliseconds(500))
                .DistinctUntilChanged().Select(x =>
                {
                    var codes=  x;
                    if (interval == 0 || codes.Count == 0)
                    {
                        return Observable.Return(new List<string>());
                    }
                    if(codes.Count<=num)
                    {
                        return Observable.Return(codes);
                    }
                    var o = Observable.Interval(TimeSpan.FromSeconds(interval))
                    .Select(tick => tick + 1)
                    .StartWith(0)
                    .Select(tick =>
                    {
                        var c = codes.Count / num;
                        if (codes.Count % num != 0)
                        {
                            c += 1;
                        }
                        var start = ((int)tick % c) * num;
                        var end = start + num;
                        if (end > codes.Count)
                        {
                            end = codes.Count;
                        }
                        var list = new List<string>(num);
                        for (var i = start; i < end; i++)
                        {
                            list.Add(codes[i]);
                        }
                        return list;
                    });
                    return o;
                }).Subscribe(x =>
                {
                    _codeSubject.OnNext(x);
                });
        }
   
        public void Play(List<string> codes)
        {
            _subject.OnNext( codes);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            num = Row * Column;
            interval = Interval;
            for (var i = 0; i < Row; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (var i = 0; i < Column; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var r = 0; r < grid.RowDefinitions.Count; r++)
            {
                for (var c = 0; c < grid.ColumnDefinitions.Count; c++)
                {
                    VideoPlayView videoPlayView = new VideoPlayView();
                    videoPlayView.FullScreenClick += VideoPlayView_FullScreenClick;
                    videoPlayView.RestoreScreenClick += VideoPlayView_RestoreScreenClick;
                    Grid.SetRow(videoPlayView, r);
                    Grid.SetColumn(videoPlayView, c);
                    grid.Children.Add(videoPlayView);
                    players.Add(videoPlayView);
                }
            }
        }

        private void VideoPlayView_RestoreScreenClick(object sender, RoutedEventArgs e)
        {
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            for (var i = 0; i < Row; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            for (var i = 0; i < Column; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            foreach (var screen in players)
            {
                screen.Visibility = Visibility.Visible;
            }
        }

        private void VideoPlayView_FullScreenClick(object sender, RoutedEventArgs e)
        {
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Add(new RowDefinition());
            foreach (var screen in players.Where(x => !Equals(x, e.OriginalSource)))
            {
                screen.Visibility = Visibility.Collapsed;
            }
        }

        public void Dispose()
        {
            _subscribe.Dispose();
            _subscribe2.Dispose();
            _subscribe3.Dispose();
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in players)
                {
                    item.FullScreenClick -= VideoPlayView_FullScreenClick;
                    item.RestoreScreenClick-= VideoPlayView_RestoreScreenClick;
                    item.Dispose();
                }                    
            });
        }
    }
}
