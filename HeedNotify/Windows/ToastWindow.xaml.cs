using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HeedNotify.Windows
{
    public partial class ToastWindow : Window
    {
        private static readonly List<ToastWindow> OpenToasts = new();
        private readonly DispatcherTimer closeTimer = new();
        private readonly TimeSpan duration;
        public string Title { get; }
        public string Message { get; }

        public ToastWindow(string title, string message, TimeSpan duration)
        {
            InitializeComponent();
            Title = title;
            Message = message;
            DataContext = this;
            this.duration = duration;

            closeTimer.Interval = duration;
            closeTimer.Tick += (_, __) => CloseToast();

            Loaded += (_, __) =>
            {
                PositionWindow();
                AnimateIn();
                closeTimer.Start();
            };

            Closed += (_, __) =>
            {
                closeTimer.Stop();
                OpenToasts.Remove(this);
                Reflow();
            };
        }

        private void PositionWindow()
        {
            var workingArea = SystemParameters.WorkArea;
            Left = workingArea.Right - Width - 16;

            OpenToasts.Add(this);
            var index = OpenToasts.Count - 1;
            double verticalOffset = OpenToasts.Take(index).Sum(t => t.Height + 12);
            Top = workingArea.Bottom - Height - 16 - verticalOffset;
        }

        private void AnimateIn()
        {
            var slide = new DoubleAnimation
            {
                From = Left + 40,
                To = Left,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(Window.LeftProperty, slide);

            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            BeginAnimation(OpacityProperty, fade);
        }

        private void AnimateOutAndClose()
        {
            var fade = new DoubleAnimation
            {
                From = Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(180)
            };
            fade.Completed += (_, __) => Close();
            BeginAnimation(OpacityProperty, fade);
        }

        private void CloseToast()
        {
            closeTimer.Stop();
            AnimateOutAndClose();
        }

        private static void Reflow()
        {
            var workingArea = SystemParameters.WorkArea;
            double currentOffset = 0;
            foreach (var toast in OpenToasts)
            {
                var targetTop = workingArea.Bottom - toast.Height - 16 - currentOffset;
                var anim = new DoubleAnimation
                {
                    To = targetTop,
                    Duration = TimeSpan.FromMilliseconds(160),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                toast.BeginAnimation(Window.TopProperty, anim);
                currentOffset += toast.Height + 12;
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            CloseToast();
        }
    }
}
