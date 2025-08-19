using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BlankApp1.Controls
{
    public partial class ScrollableTooltipTextBlock : UserControl
    {
        private bool _isMouseOverHost;
        private bool _isMouseOverPopup;

        public ScrollableTooltipTextBlock()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text), typeof(string), typeof(ScrollableTooltipTextBlock),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty ToolTipTextProperty =
            DependencyProperty.Register(
                nameof(ToolTipText), typeof(string), typeof(ScrollableTooltipTextBlock),
                new PropertyMetadata(string.Empty));

        public string ToolTipText
        {
            get => (string)GetValue(ToolTipTextProperty);
            set => SetValue(ToolTipTextProperty, value);
        }

        public static readonly DependencyProperty TooltipMaxWidthProperty =
            DependencyProperty.Register(
                nameof(TooltipMaxWidth), typeof(double), typeof(ScrollableTooltipTextBlock),
                new PropertyMetadata(300.0));

        public double TooltipMaxWidth
        {
            get => (double)GetValue(TooltipMaxWidthProperty);
            set => SetValue(TooltipMaxWidthProperty, value);
        }

        public static readonly DependencyProperty TooltipMaxHeightProperty =
            DependencyProperty.Register(
                nameof(TooltipMaxHeight), typeof(double), typeof(ScrollableTooltipTextBlock),
                new PropertyMetadata(200.0));

        public double TooltipMaxHeight
        {
            get => (double)GetValue(TooltipMaxHeightProperty);
            set => SetValue(TooltipMaxHeightProperty, value);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (sender is ScrollViewer scrollViewer)
            //{
            //    // Allow mouse wheel to scroll content when tooltip is open
            //    double offset = scrollViewer.VerticalOffset - e.Delta;
            //    scrollViewer.ScrollToVerticalOffset(offset);
            //    e.Handled = true;
            //}
        }

        private void Host_MouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOverHost = true;
            TipPopup.IsOpen = true;
        }

        private void Host_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOverHost = false;
            ScheduleCloseIfNotHovered();
        }

        private void Popup_MouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOverPopup = true;
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOverPopup = false;
            ScheduleCloseIfNotHovered();
        }

        private void ScheduleCloseIfNotHovered()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_isMouseOverHost && !_isMouseOverPopup)
                {
                    TipPopup.IsOpen = false;
                }
            }), DispatcherPriority.Background);
        }
    }
}

