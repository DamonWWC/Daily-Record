using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BlankApp1.Controls
{
    public static class HoverTipService
    {
        // Public attached properties
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(HoverTipService),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(HoverTipService),
                new PropertyMetadata(string.Empty, OnTextChanged));

        public static void SetText(DependencyObject element, string value) => element.SetValue(TextProperty, value);
        public static string GetText(DependencyObject element) => (string)element.GetValue(TextProperty);

        public static readonly DependencyProperty MaxWidthProperty =
            DependencyProperty.RegisterAttached(
                "MaxWidth",
                typeof(double),
                typeof(HoverTipService),
                new PropertyMetadata(320.0, OnSizingChanged));

        public static void SetMaxWidth(DependencyObject element, double value) => element.SetValue(MaxWidthProperty, value);
        public static double GetMaxWidth(DependencyObject element) => (double)element.GetValue(MaxWidthProperty);

        public static readonly DependencyProperty MaxHeightProperty =
            DependencyProperty.RegisterAttached(
                "MaxHeight",
                typeof(double),
                typeof(HoverTipService),
                new PropertyMetadata(200.0, OnSizingChanged));

        public static void SetMaxHeight(DependencyObject element, double value) => element.SetValue(MaxHeightProperty, value);
        public static double GetMaxHeight(DependencyObject element) => (double)element.GetValue(MaxHeightProperty);

        public static readonly DependencyProperty CloseDelayProperty =
            DependencyProperty.RegisterAttached(
                "CloseDelay",
                typeof(int),
                typeof(HoverTipService),
                new PropertyMetadata(1500)); // milliseconds

        public static void SetCloseDelay(DependencyObject element, int value) => element.SetValue(CloseDelayProperty, value);
        public static int GetCloseDelay(DependencyObject element) => (int)element.GetValue(CloseDelayProperty);

        public static readonly DependencyProperty PrewarmOnLoadProperty =
            DependencyProperty.RegisterAttached(
                "PrewarmOnLoad",
                typeof(bool),
                typeof(HoverTipService),
                new PropertyMetadata(true));

        public static void SetPrewarmOnLoad(DependencyObject element, bool value) => element.SetValue(PrewarmOnLoadProperty, value);
        public static bool GetPrewarmOnLoad(DependencyObject element) => (bool)element.GetValue(PrewarmOnLoadProperty);

        // Internal state attached property
        private static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached(
                "State",
                typeof(HoverState),
                typeof(HoverTipService),
                new PropertyMetadata(null));

        private static void SetState(DependencyObject element, HoverState value) => element.SetValue(StateProperty, value);
        private static HoverState GetState(DependencyObject element) => (HoverState)element.GetValue(StateProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element) return;

            if ((bool)e.NewValue)
            {
                Attach(element);
            }
            else
            {
                Detach(element);
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var state = GetState(d);
            if (state?.TextBlock is TextBlock tb)
            {
                tb.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void OnSizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var state = GetState(d);
            if (state?.Border is Border border)
            {
                border.MaxWidth = GetMaxWidth(d);
                border.MaxHeight = GetMaxHeight(d);
            }
        }

        private static void Attach(FrameworkElement element)
        {
            var state = GetState(element);
            if (state == null)
            {
                state = new HoverState();
                SetState(element, state);
            }

            element.MouseEnter += OnHostMouseEnter;
            element.MouseLeave += OnHostMouseLeave;
            element.Unloaded += OnHostUnloaded;
            element.Loaded += OnHostLoaded;
        }

        private static void Detach(FrameworkElement element)
        {
            element.MouseEnter -= OnHostMouseEnter;
            element.MouseLeave -= OnHostMouseLeave;
            element.Unloaded -= OnHostUnloaded;
            element.Loaded -= OnHostLoaded;

            var state = GetState(element);
            if (state != null)
            {
                state.Dispose();
                SetState(element, null);
            }
        }

        private static void OnHostLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element) return;
            if (!GetIsEnabled(element)) return;
            if (!GetPrewarmOnLoad(element)) return;
            var state = EnsurePopup(element);
            if (state.Prewarmed) return;
            PrewarmPopup(element, state);
        }

        private static void OnHostUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                Detach(element);
            }
        }

        private static void OnHostMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is not FrameworkElement element) return;
            var state = EnsurePopup(element);
            state.IsMouseOverHost = true;
            StopCloseTimer(element, state);
            state.Popup.IsOpen = true;
        }

        private static void OnHostMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is not FrameworkElement element) return;
            var state = GetState(element);
            if (state == null) return;
            state.IsMouseOverHost = false;
            ScheduleCloseWithDelay(element, state);
        }

        private static HoverState EnsurePopup(FrameworkElement element)
        {
            var state = GetState(element);
            if (state == null)
            {
                state = new HoverState();
                SetState(element, state);
            }

            if (state.Popup == null)
            {
                // Build popup UI
                var popup = new Popup
                {
                    PlacementTarget = element,
                    Placement = PlacementMode.Bottom,
                    AllowsTransparency = true,
                    Focusable = false,
                    StaysOpen = true,
                };

                var border = new Border
                {
                    Background = SystemColors.InfoBrush,
                    BorderBrush = SystemColors.InfoTextBrush,
                    BorderThickness = new Thickness(1),
                    MaxWidth = GetMaxWidth(element),
                    MaxHeight = GetMaxHeight(element)
                };

                var scrollViewer = new ScrollViewer
                {
                    CanContentScroll = true,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                };
                scrollViewer.PreviewMouseWheel += (s, ev) =>
                {
                    if (s is ScrollViewer sv)
                    {
                        sv.ScrollToVerticalOffset(sv.VerticalOffset - ev.Delta);
                        ev.Handled = true;
                    }
                };

                var textBlock = new TextBlock
                {
                    Margin = new Thickness(8),
                    TextWrapping = TextWrapping.Wrap,
                    Text = GetText(element) ?? string.Empty
                };
                TextOptions.SetTextFormattingMode(textBlock, TextFormattingMode.Display);
                TextOptions.SetTextRenderingMode(textBlock, TextRenderingMode.ClearType);
                RenderOptions.SetBitmapScalingMode(textBlock, BitmapScalingMode.NearestNeighbor);

                scrollViewer.Content = textBlock;
                border.Child = scrollViewer;
                popup.Child = border;

                border.MouseEnter += (s, e) => { state.IsMouseOverPopup = true; StopCloseTimer(element, state); };
                border.MouseLeave += (s, e) =>
                {
                    state.IsMouseOverPopup = false;
                    ScheduleCloseWithDelay(element, state);
                };

                state.Popup = popup;
                state.Border = border;
                state.TextBlock = textBlock;
            }

            return state;
        }

        private static void ScheduleCloseWithDelay(FrameworkElement element, HoverState state)
        {
            if (state.CloseTimer == null)
            {
                state.CloseTimer = new DispatcherTimer(DispatcherPriority.Background, element.Dispatcher);
                state.CloseTimer.Tick += (s, ev) =>
                {
                    if (!state.IsMouseOverHost && !state.IsMouseOverPopup)
                    {
                        if (state.Popup != null)
                        {
                            state.Popup.IsOpen = false;
                        }
                        state.CloseTimer?.Stop();
                    }
                    else
                    {
                        // If hovered again, stop timer
                        state.CloseTimer?.Stop();
                    }
                };
            }

            state.CloseTimer.Interval = TimeSpan.FromMilliseconds(GetCloseDelay(element));
            state.CloseTimer.Stop();
            state.CloseTimer.Start();
        }

        private static void StopCloseTimer(FrameworkElement element, HoverState state)
        {
            state.CloseTimer?.Stop();
        }

        private static void PrewarmPopup(FrameworkElement element, HoverState state)
        {
            // Ensure popup exists
            EnsurePopup(element);
            if (state.Popup == null || state.Prewarmed) return;

            var originalOffsetH = state.Popup.HorizontalOffset;
            var originalOffsetV = state.Popup.VerticalOffset;
            var originalOpacity = 1.0;
            if (state.Popup.Child is UIElement child)
            {
                originalOpacity = child.Opacity;
                child.Opacity = 0.0; // invisible
            }

            // move offscreen to avoid flicker
            state.Popup.HorizontalOffset = 100000;
            state.Popup.VerticalOffset = 100000;
            state.Popup.IsOpen = true;

            // After first render, close and restore
            element.Dispatcher.BeginInvoke(new Action(() =>
            {
                state.Popup.IsOpen = false;
                state.Popup.HorizontalOffset = originalOffsetH;
                state.Popup.VerticalOffset = originalOffsetV;
                if (state.Popup.Child is UIElement child2)
                {
                    child2.Opacity = originalOpacity;
                }
                state.Prewarmed = true;
            }), DispatcherPriority.Render);
        }

        private class HoverState : IDisposable
        {
            public Popup Popup { get; set; }
            public Border Border { get; set; }
            public TextBlock TextBlock { get; set; }
            public bool IsMouseOverHost { get; set; }
            public bool IsMouseOverPopup { get; set; }
            public DispatcherTimer CloseTimer { get; set; }
            public bool Prewarmed { get; set; }

            public void Dispose()
            {
                if (Popup != null)
                {
                    Popup.IsOpen = false;
                    Popup.Child = null;
                    Popup = null;
                }
                CloseTimer?.Stop();
                CloseTimer = null;
                Border = null;
                TextBlock = null;
            }
        }
    }
}

