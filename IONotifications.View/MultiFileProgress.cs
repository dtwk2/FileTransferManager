using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IOExtensions.View
{
    /// <summary>
    /// Interaction logic for MultiFileProgressView.xaml
    /// </summary>
    public class MultiFileProgress : Control
    {
        static MultiFileProgress()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiFileProgress), new FrameworkPropertyMetadata(typeof(MultiFileProgress)));
        }

        public MultiFileProgress()
        {
            this.Loaded += MultiStageProgressView_Loaded;
        }

        private void MultiStageProgressView_Loaded(object sender, RoutedEventArgs e)
        {
            var arr = ProgressViews.Cast<FileProgressView>().ToArray();

            //var textBlock = this.Resources["TextBlock1"] as TextBlock;
            contentControl = this.GetChildOfType<ContentControl>();
            textBlock = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                FontSize = 40,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            contentControl.Content = textBlock;
            var countDown = CountDown;
            var dis = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Scan(countDown, (a, _) => a - 1)
                .Take(countDown)
                .ObserveOnDispatcher()
                .Subscribe(CountDownChange,
                    async () =>
                    {
                        foreach (var progressView in arr)
                        {
                            await StepChange(progressView);
                        }

                        RaiseCompleteEvent();
                    });
        }

        public int CountDown { get; set; } = 3;

        protected virtual async Task<Unit> StepChange(FileProgressView progressView)
        {
            await this.Dispatcher.InvokeAsync(() => contentControl.Content = progressView);
            progressView.TransferCommand.Execute(null);
            return await progressView.ToCompleteChanges().Take(1).ToTask();
        }


        protected virtual void CountDownChange(int a)
        {
            textBlock.Text = a.ToString();
        }

        public IEnumerable ProgressViews
        {
            get { return (IEnumerable)GetValue(ProgressViewsProperty); }
            set { SetValue(ProgressViewsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressViews.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressViewsProperty =
            DependencyProperty.Register("ProgressViews", typeof(IEnumerable), typeof(MultiFileProgress), new PropertyMetadata(null));


        public bool IsComplete
        {
            get { return (bool)GetValue(IsCompleteProperty); }
            set { SetValue(IsCompleteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Complete.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCompleteProperty =
            DependencyProperty.Register("IsComplete", typeof(bool), typeof(MultiFileProgress), new PropertyMetadata(false));

        public static readonly RoutedEvent CompleteEvent = EventManager.RegisterRoutedEvent("Complete", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiFileProgress));
        private ContentControl contentControl;
        private TextBlock textBlock;

        // Provide CLR accessors for the event
        public event RoutedEventHandler Complete
        {
            add { AddHandler(CompleteEvent, value); }
            remove { RemoveHandler(CompleteEvent, value); }
        }

        // This method raises the Complete event
        void RaiseCompleteEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(MultiFileProgress.CompleteEvent);
            RaiseEvent(newEventArgs);
        }

    }


    public static class Helper
    {
        public static IObservable<Unit> ToCompleteChanges(this FileProgressView control)
        {
            return Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                    h => control.Complete += h,
                    h => control.Complete -= h)
                .Select(a => Unit.Default);
        }

        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
