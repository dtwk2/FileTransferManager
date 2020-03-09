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
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : Control, IProgressView
    {
        protected readonly Subject<Unit> transferButtonsClicks = new Subject<Unit>();
        protected readonly Subject<Unit> templateApplied = new Subject<Unit>();
        protected readonly Subject<bool> showTransferChanges = new Subject<bool>();
        protected Button transferButton;
        protected Panel TopPanel;
        protected TextBlock TitleTextBlock;
        protected ProgressBar progressBar;
        protected ContentControl ConfigContentControl;

        static ProgressControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressControl), new FrameworkPropertyMetadata(typeof(ProgressControl)));
        }

        public ProgressControl()
        {

            RunCommand = new DelegateCommand<object>(unit => transferButtonsClicks.OnNext(Unit.Default));

            ConfigContentControlChanges.CombineLatest(this.templateApplied,(a,b)=>a).Subscribe(content =>
                ConfigContentControl.Content = content);

            showTransferChanges.CombineLatest(this.templateApplied, (a, b) => a).SubscribeOnDispatcher().Subscribe(a =>
            {
                transferButton.Visibility = a ? Visibility.Visible : Visibility.Hidden;
            });

            readOnlyChanges.CombineLatest(this.templateApplied, (a, b) => a).Subscribe(a =>
            {
                TopPanel.Visibility = a ? Visibility.Collapsed : Visibility.Visible;
                ConfigContentControl.Visibility = a ? Visibility.Collapsed : Visibility.Visible;
                //txtDestination.IsReadOnly = a;
                //txtSource.IsReadOnly = a;
            });

            titleChanges.CombineLatest(this.ToLoadedChanges(), (a, b) => a).Subscribe(a =>
            {
                this.TitleTextBlock.Text = a;
            });

            ScheduleProgress();

            this.ToLoadedChanges().Subscribe(a =>
            {
                readOnlyChanges.OnNext(IsReadOnly);
                showTransferChanges.OnNext(ShowTransfer);
            });
        }

        public override void OnApplyTemplate()
        {
            this.transferButton = this.GetTemplateChild("transferButton") as Button;
            this.TopPanel = this.GetTemplateChild("TopPanel") as Panel;
            this.transferButton = this.GetTemplateChild("transferButton") as Button;
            this.ConfigContentControl = this.GetTemplateChild("ContentControl1") as ContentControl;

            transferButton.Command = RunCommand;

            TitleTextBlock = this.GetTemplateChild("TitleTextBlock") as TextBlock;
            TitleTextBlock.Text = Title;
            //this.lblPercent = this.GetTemplateChild("lblPercent") as TextBlock;
            //this.lblPercent.Text = "0 %";
            this.progressBar = this.GetTemplateChild("progressBar") as ProgressBar;
            
            base.OnApplyTemplate();
            templateApplied.OnNext(Unit.Default);
        }

        protected virtual void ScheduleProgress()
        {
            var obs = transferButtonsClicks
                .CombineLatest(
                            this.transfererChanges,
                            (a, b) => b);

            obs.Subscribe(a =>
            {
                TitleTextBlock.Visibility = Visibility.Visible;
                transferButton.Visibility = Visibility.Collapsed;
            });

            obs
                .SelectMany(transferer =>
                {
                    return transferer.Transfer()
                        .Scan((date: DateTime.Now, default(TimeSpan), default(TransferProgress)),
                            (d, t) => (d.date, DateTime.Now - d.date, t));
                }).Subscribe(a =>
                {
                    (DateTime start, TimeSpan timeSpan, TransferProgress transferProgress) = a;
                    this.Dispatcher.Invoke(() =>
                    {
                        IsComplete = false;
                        progressBar.Value = transferProgress.Percentage;

                        progressBar.Tag = transferProgress.AsPercentage();
                        if (transferProgress.BytesTransferred == transferProgress.Total ||
                            transferProgress.Transferred == transferProgress.Total)
                        {
                            IsComplete = true;
                            RaiseCompleteEvent(timeSpan);
                            TitleTextBlock.Opacity = 0.5;
                            // transferButton.Visibility = Visibility.Visible;
                        }
                    });
                });
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ProgressControl), new PropertyMetadata("Progressing", TitleChanged));

        protected readonly Subject<string> titleChanges = new Subject<string>();

        private static void TitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ProgressControl).titleChanges.OnNext((string)e.NewValue);
        }
   

        public string Details
        {
            get { return (string)GetValue(DetailsProperty); }
            set { SetValue(DetailsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Details.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailsProperty =
            DependencyProperty.Register("Details", typeof(string), typeof(ProgressControl),
                new PropertyMetadata("Details about task", DetailsChanged));

        private static void DetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // (d as ProgressControl).TitleTextBlock.Text = (string)e.NewValue;
        }

        public ITransferer Transferer
        {
            get { return (ITransferer)GetValue(TransfererProperty); }
            set { SetValue(TransfererProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Transferer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransfererProperty =
            DependencyProperty.Register("Transferer", typeof(ITransferer), typeof(ProgressControl),
                new PropertyMetadata(TransfererChanged));

        protected readonly Subject<ITransferer> transfererChanges = new Subject<ITransferer>();

        private static void TransfererChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ProgressControl).transfererChanges.OnNext(e.NewValue as ITransferer);
        }


        public bool IsComplete
        {
            get { return (bool)GetValue(IsCompleteProperty); }
            set { SetValue(IsCompleteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Complete.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCompleteProperty =
            DependencyProperty.Register("IsComplete", typeof(bool), typeof(ProgressControl),
                new PropertyMetadata(false));

        public static readonly RoutedEvent CompleteEvent = EventManager.RegisterRoutedEvent("Complete",
            RoutingStrategy.Bubble, typeof(MultiProgress.TimeSpanRoutedEventHandler), typeof(ProgressControl));

        // Provide CLR accessors for the event
        public event MultiProgress.TimeSpanRoutedEventHandler Complete
        {
            add => AddHandler(CompleteEvent, value);
            remove => RemoveHandler(CompleteEvent, value);
        }

        public IObservable<TimeSpan> CompleteEvents => this.ToCompleteChanges();

        // This method raises the Complete event
        protected void RaiseCompleteEvent(TimeSpan timeSpan)
        {
            RaiseEvent(new TimeSpanRoutedEventArgs(ProgressControl.CompleteEvent, timeSpan));
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ProgressControl),
                new PropertyMetadata(false, readOnlyChanged));

        protected readonly Subject<bool> readOnlyChanges = new Subject<bool>();

        private static void readOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ProgressControl).readOnlyChanges.OnNext((bool)e.NewValue);
        }

        public ICommand RunCommand
        {
            get { return (ICommand)GetValue(RunCommandProperty); }
            set { SetValue(RunCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RunCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RunCommandProperty =
            DependencyProperty.Register("RunCommand", typeof(ICommand), typeof(ProgressControl),
                new PropertyMetadata(null));

        public bool ShowTransfer
        {
            get { return (bool)GetValue(ShowTransferProperty); }
            set { SetValue(ShowTransferProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowTransfer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowTransferProperty =
            DependencyProperty.Register("ShowTransfer", typeof(bool), typeof(ProgressControl),
                new PropertyMetadata(true, showTransferChanged));


        private static void showTransferChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ProgressControl).showTransferChanges.OnNext((bool)e.NewValue);
        }


        //private void Button_Click_Transfer(object sender, RoutedEventArgs e)
        //{
        //    transferButtonsClicks.OnNext(Unit.Default);
        //}



        public object ConfigContent
        {
            get { return (object)GetValue(ConfigContentProperty); }
            set { SetValue(ConfigContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConfigContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConfigContentProperty =
            DependencyProperty.Register("ConfigContent", typeof(object), typeof(ProgressControl), new PropertyMetadata(null,ConfigContentChanged));


        protected readonly Subject<object> ConfigContentControlChanges = new Subject<object>();
        private static void ConfigContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ProgressControl).ConfigContentControlChanges.OnNext(e.NewValue);

        }
    }

    public static class ObservableHelper
    {

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
    }
}
