using System;
using System.Collections;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace IOExtensions.View
{
    /// <summary>
    /// Interaction logic for MultiProgressView.xaml
    /// </summary>
    public class MultiProgress : Control
    {
        static MultiProgress()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiProgress), new FrameworkPropertyMetadata(typeof(MultiProgress)));
        }

        public MultiProgress()
        {
            this.Loaded += MultiStageProgressView_Loaded;

        }

        private void MultiStageProgressView_Loaded(object sender, RoutedEventArgs e)
        {
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
                        var arr = ProgressViews.Cast<IProgressView>().ToArray();
                        foreach (var progressView in arr)
                        {
                            await StepChange(progressView);
                        }

                        RaiseCompleteEvent();
                    });
        }

        public int CountDown { get; set; } = 3;

        protected virtual async Task<TimeSpan> StepChange(IProgressView progressView)
        {

            await this.Dispatcher.InvokeAsync(Callback);
     
            return await progressView.CompleteEvents.Take(1).ToTask();

            async Task Callback()
            { 
                var animation = GetAnimation();
                var task = ToTask(animation);
                contentControl.BeginAnimation(UIElement.OpacityProperty, animation);
                await task;

                contentControl.Content = progressView;
                
                var animation2 = GetAnimation2();
                task = ToTask(animation2);
                contentControl.BeginAnimation(UIElement.OpacityProperty, animation2);
                progressView.RunCommand.Execute(null);
                await task;

                static DoubleAnimation GetAnimation() => new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(2),
                    FillBehavior = FillBehavior.Stop
                };

                static DoubleAnimation GetAnimation2() => new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromSeconds(2),
                    FillBehavior = FillBehavior.Stop
                };
            }
            static Task<EventPattern<object>> ToTask(DoubleAnimation animation)
            {
                return Observable.FromEventPattern(a => animation.Completed += a, a => animation.Completed -= a).Take(1).ToTask();
            }
        }

 

        protected virtual void CountDownChange(int a)
        {
            textBlock.Text = a.ToString();
        }

        public IEnumerable ProgressViews
        {
            get => (IEnumerable)GetValue(ProgressViewsProperty);
            set => SetValue(ProgressViewsProperty, value);
        }

        // Using a DependencyProperty as the backing store for ProgressViews.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressViewsProperty =
            DependencyProperty.Register("ProgressViews", typeof(IEnumerable), typeof(MultiProgress), new PropertyMetadata(null));


        public bool IsComplete
        {
            get => (bool)GetValue(IsCompleteProperty);
            set => SetValue(IsCompleteProperty, value);
        }

        // Using a DependencyProperty as the backing store for Complete.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCompleteProperty =
            DependencyProperty.Register("IsComplete", typeof(bool), typeof(MultiProgress), new PropertyMetadata(false));

        public static readonly RoutedEvent CompleteEvent = EventManager.RegisterRoutedEvent("Complete", RoutingStrategy.Bubble, typeof(TimeSpanRoutedEventHandler), typeof(MultiProgress));
        private ContentControl contentControl;
        private TextBlock textBlock;

        // Provide CLR accessors for the event
        public event RoutedEventHandler Complete
        {
            add => AddHandler(CompleteEvent, value);
            remove => RemoveHandler(CompleteEvent, value);
        }

        // This method raises the Complete event
        void RaiseCompleteEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(MultiProgress.CompleteEvent);
            RaiseEvent(newEventArgs);
        }

        public delegate void TimeSpanRoutedEventHandler(object sender, TimeSpanRoutedEventArgs e);


    }


    public class TimeSpanRoutedEventArgs : RoutedEventArgs
    {
        public TimeSpanRoutedEventArgs(RoutedEvent routedEvent, TimeSpan timeSpan) : base(routedEvent)
        {
            this.TimeSpan = timeSpan;
        }

        public TimeSpan TimeSpan { get; set; }
    }
}
