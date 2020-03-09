using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IOExtensions.Abstract;
using IOExtensions.Core;
using IOExtensions.Reactive;
using Prism.Commands;

namespace IOExtensions.View
{
    /// <summary>
    /// Interaction logic for FileProgressView.xaml
    /// </summary>
    public partial class FileProgressView : UserControl, IProgressView
    {
        private readonly Subject<Unit> transferButtonsClicks = new Subject<Unit>();

        public FileProgressView()
        {
            InitializeComponent();
            ScheduleProgress();
            this.Loaded += FileProgressView_Loaded;

            RunCommand = new DelegateCommand<object>(unit => transferButtonsClicks.OnNext(Unit.Default));

            //CompleteEvents = this.ToCompleteChanges();

            showTransferChanges.SubscribeOnDispatcher().Subscribe(a =>
            {
                transferButton.Visibility = a ? Visibility.Visible : Visibility.Hidden;
            });

            transferButton.Command = RunCommand;

            readOnlyChanges.Subscribe(a =>
            {
                Expander1.Visibility = a ? Visibility.Collapsed : Visibility.Visible;
                BrowseDestination.Visibility = a ? Visibility.Collapsed : Visibility.Visible;
                BrowseSource.Visibility = a ? Visibility.Collapsed : Visibility.Visible;
                //txtSource.Background = Brushes.Transparent;
                //txtDestination.Background = Brushes.Transparent;
                //txtSource.BorderBrush = Brushes.Transparent;
                //txtDestination.BorderBrush = Brushes.Transparent;
                txtDestination.IsReadOnly = a;
                txtSource.IsReadOnly = a;
            });
        }

        private void FileProgressView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Source != null)
            {
                sourceChanges.OnNext(Source);
            }

            if (Destination != null)
            {
                destinationChanges.OnNext(Destination);
            }

            showTransferChanges.OnNext(ShowTransfer);
            readOnlyChanges.OnNext(IsReadOnly);
        }

        private void ScheduleProgress()
        {
            //var scheduler = RxApp.MainThreadScheduler;
            //this.sourceChanges.Subscribe(a => { txtSource.Text = a; });
            //this.destinationChanges.Subscribe(a => { txtDestination.Text = a; });
            //lblPercent.Text = "0 %";

            ////CreateDummyFile();
            //TitleTextBlock.Text = Title;

            //var obs = transferButtonsClicks
            //    .WithLatestFrom(this.sourceChanges.DistinctUntilChanged()
            //            .CombineLatest(this.destinationChanges.DistinctUntilChanged(),
            //                this.transfererChanges,
            //                (a, b, c) => (a, b, c)),
            //        (a, b) => b);

            //obs.Subscribe(a =>
            //{

            //    TitleTextBlock.Visibility = Visibility.Visible;
            //    transferButton.Visibility = Visibility.Collapsed;
            //});

            //obs
            //    .SelectMany(a =>
            //    {
            //        var (source, destination, transferer) = a;
            //        Directory.CreateDirectory(destination);
            //        return transferer.Transfer(source, destination)
            //            .Scan((date: DateTime.Now, default(TimeSpan), default(TransferProgress)),
            //                (d, t) => (d.date, DateTime.Now - d.date, t));
            //    }).Subscribe(a =>
            //    {
            //        (DateTime start, TimeSpan timeSpan, TransferProgress transferProgress) = a;
            //        scheduler.Schedule(() =>
            //        {
            //            IsComplete = false;
            //            progressBar.Value = transferProgress.Percentage;

            //            lblPercent.Text = transferProgress.AsPercentage();
            //            if (transferProgress.BytesTransferred == transferProgress.Total ||
            //                transferProgress.Transferred == transferProgress.Total)
            //            {
            //                IsComplete = true;
            //                RaiseCompleteEvent(timeSpan);
            //                TitleTextBlock.Opacity = 0.5;
            //                // transferButton.Visibility = Visibility.Visible;
            //            }
            //        });
            //    });


        }



        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(FileProgressView),
                new PropertyMetadata("Progressing", TitleChanged));

        private static void TitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressView).TitleTextBlock.Text = (string)e.NewValue;
        }

        public string Details
        {
            get { return (string)GetValue(DetailsProperty); }
            set { SetValue(DetailsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Details.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailsProperty =
            DependencyProperty.Register("Details", typeof(string), typeof(FileProgressView),
                new PropertyMetadata("Details about task", DetailsChanged));

        private static void DetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // (d as FileProgressView).TitleTextBlock.Text = (string)e.NewValue;
        }


        public ITransferer Transferer
        {
            get { return (ITransferer)GetValue(TransfererProperty); }
            set { SetValue(TransfererProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Transferer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransfererProperty =
            DependencyProperty.Register("Transferer", typeof(ITransferer), typeof(FileProgressView),
                new PropertyMetadata(TransfererChanged));

        private readonly Subject<ITransferer> transfererChanges = new Subject<ITransferer>();

        private static void TransfererChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressView).transfererChanges.OnNext(e.NewValue as ITransferer);
        }




        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(FileProgressView),
                new PropertyMetadata(null, SourceChanged));

        private readonly Subject<string> sourceChanges = new Subject<string>();

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressView).sourceChanges.OnNext(e.NewValue as string);
        }


        public string Destination
        {
            get { return (string)GetValue(DestinationProperty); }
            set { SetValue(DestinationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Destination.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DestinationProperty =
            DependencyProperty.Register("Destination", typeof(string), typeof(FileProgressView),
                new PropertyMetadata(null, DestinationChanged));

        private readonly Subject<string> destinationChanges = new Subject<string>();

        private static void DestinationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressView).destinationChanges.OnNext(e.NewValue as string);
        }

        public bool IsComplete
        {
            get { return (bool)GetValue(IsCompleteProperty); }
            set { SetValue(IsCompleteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Complete.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCompleteProperty =
            DependencyProperty.Register("IsComplete", typeof(bool), typeof(FileProgressView),
                new PropertyMetadata(false));

        public static readonly RoutedEvent CompleteEvent = EventManager.RegisterRoutedEvent("Complete",
            RoutingStrategy.Bubble, typeof(MultiProgress.TimeSpanRoutedEventHandler), typeof(FileProgressView));

        // Provide CLR accessors for the event
        public event MultiProgress.TimeSpanRoutedEventHandler Complete
        {
            add => AddHandler(CompleteEvent, value);
            remove => RemoveHandler(CompleteEvent, value);
        }

        public IObservable<TimeSpan> CompleteEvents { get; }

        // This method raises the Complete event
        void RaiseCompleteEvent(TimeSpan timeSpan)
        {
            RaiseEvent(new TimeSpanRoutedEventArgs(FileProgressView.CompleteEvent, timeSpan));
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(FileProgressView),
                new PropertyMetadata(false, readOnlyChanged));

        private readonly Subject<bool> readOnlyChanges = new Subject<bool>();

        private static void readOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressView).readOnlyChanges.OnNext((bool)e.NewValue);
        }

        public ICommand RunCommand
        {
            get { return (ICommand)GetValue(RunCommandProperty); }
            set { SetValue(RunCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RunCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RunCommandProperty =
            DependencyProperty.Register("RunCommand", typeof(ICommand), typeof(FileProgressView),
                new PropertyMetadata(null));

        public bool ShowTransfer
        {
            get { return (bool)GetValue(ShowTransferProperty); }
            set { SetValue(ShowTransferProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowTransfer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowTransferProperty =
            DependencyProperty.Register("ShowTransfer", typeof(bool), typeof(FileProgressView),
                new PropertyMetadata(true, showTransferChanged));

        private readonly Subject<bool> showTransferChanges = new Subject<bool>();

        private static void showTransferChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressView).showTransferChanges.OnNext((bool)e.NewValue);
        }


        private void Button_Click_Transfer(object sender, RoutedEventArgs e)
        {
            transferButtonsClicks.OnNext(Unit.Default);
        }


        private void Button_Click_Source(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_Destination(object sender, RoutedEventArgs e)
        {

        }
    }

    //public static class ObservableHelper
    //{

    //    public static IObservable<TimeSpan> ToCompleteChanges(this FileProgressView control)
    //    {
    //        return Observable.FromEventPattern<MultiProgress.TimeSpanRoutedEventHandler, TimeSpanRoutedEventArgs>(
    //                h => control.Complete += h,
    //                h => control.Complete -= h)
    //            .Select(a => a.EventArgs.TimeSpan);
    //    }
    //}
}
