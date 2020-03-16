using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace IOExtensions.View
{
    public static class ObservableHelper
    {
        public static IObservable<Unit> ToClicks(this Button button)
        {
            return Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                    h => button.Click += h,
                    h => button.Click -= h)
                .Select(a => Unit.Default);
        }

        public static IObservable<TimeSpan> ToCompleteChanges(this ProgressControl control)
        {
            return Observable.FromEventPattern<MultiProgress.TimeSpanRoutedEventHandler, TimeSpanRoutedEventArgs>(
                    h => control.Complete += h,
                    h => control.Complete -= h)
                .Select(a => a.EventArgs.TimeSpan);
        }


        public static IObservable<RoutedEventArgs> ToLoadedChanges(this Control control)
        {
            return Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                    h => control.Loaded += h,
                    h => control.Loaded -= h)
                .Select(a => a.EventArgs);
        }

        public static IObservable<string> ToThrottledObservable(this TextBox textBox)
        {
            return Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                    s => textBox.TextChanged += s,
                    s => textBox.TextChanged -= s)
                .Select(evt => textBox.Text) // better to select on the UI thread
                .Throttle(TimeSpan.FromMilliseconds(500))
                .DistinctUntilChanged();
        }

    }
}
