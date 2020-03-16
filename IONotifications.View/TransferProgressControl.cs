using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using IOExtensions.Abstract;


namespace IOExtensions.View
{
    /// <summary>
    /// Interaction logic for FileProgressControl.xaml
    /// </summary>
    public partial class TransferProgressControl : ProgressControl
    {
        private PathBrowser BrowseSource;
        private PathBrowser BrowseDestination;



        static TransferProgressControl()
        {
            //  DefaultStyleKeyProperty.OverrideMetadata(typeof(FileProgressControl), new FrameworkPropertyMetadata(typeof(FileProgressControl)));
        }

        public TransferProgressControl() : base()
        {
            this.sourceChanges.CombineLatest(this.templateApplied, (a, b) => a)
                .DistinctUntilChanged()
                .Subscribe(a => { BrowseSource.SetPath.Execute(a); });

            this.destinationChanges.CombineLatest(this.templateApplied, (a, b) => a)
                .DistinctUntilChanged()
                .Subscribe(a => { BrowseDestination.SetPath.Execute(a); });
        }

        public override void OnApplyTemplate()
        {
            var stackPanel = new StackPanel();
            this.BrowseSource = SourceType==PathType.File? (PathBrowser)new FileBrowser():new FolderBrowser();
            this.BrowseDestination = DestinationType == PathType.File ? (PathBrowser)new FileBrowser() : new FolderBrowser();
            this.BrowseSource.TextChange += (s, e) => sourceChanges.OnNext(e.Text);
            this.BrowseDestination.TextChange += (s, e) => destinationChanges.OnNext(e.Text);

            this.ConfigContent ??= stackPanel;

            if (Source != null)
            {
                sourceChanges.OnNext(Source);
            }

            if (Destination != null)
            {
                destinationChanges.OnNext(Destination);
            }


            base.OnApplyTemplate();
        }


        protected override void ScheduleProgress()
        {
            var obs = transferButtonsClicks
                .WithLatestFrom(this.sourceChanges.DistinctUntilChanged()
                        .CombineLatest(this.destinationChanges.DistinctUntilChanged(),
                            this.transfererChanges,
                            (a, b, c) => (a, b, c)),
                    (a, b) => b);

            obs.CombineLatest(templateApplied, (a, b) => a).Subscribe(a =>
                {
                    TitleTextBlock.Visibility = Visibility.Visible;
                    transferButton.Visibility = Visibility.Collapsed;
                });

            obs
                .SelectMany(abc =>
                {
                    var (source, destination, transferer) = abc;
                    return transferer.Transfer(source, destination)
                        .Scan((date: DateTime.Now, default(TimeSpan), default(ITransferProgress)),
                            (d, t) => (d.date, DateTime.Now - d.date, t));
                })
                .CombineLatest(templateApplied, (a, b) => a)
                .Subscribe(a =>
                {

                    (DateTime start, TimeSpan timeSpan, ITransferProgress transferProgress) = a;
                    this.Dispatcher.Invoke(() =>
                    {
                        IsComplete = false;
                        progressBar.Value = transferProgress.Percentage;
                        progressBar.Tag = transferProgress.Fraction.ToString("00 %");

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


        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(TransferProgressControl), new PropertyMetadata(null, SourceChanged));

        private readonly Subject<string> sourceChanges = new Subject<string>();

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TransferProgressControl).sourceChanges.OnNext(e.NewValue as string);
        }


        public string Destination
        {
            get { return (string)GetValue(DestinationProperty); }
            set { SetValue(DestinationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Destination.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DestinationProperty =
            DependencyProperty.Register("Destination", typeof(string), typeof(TransferProgressControl), new PropertyMetadata(null, DestinationChanged));

        private readonly Subject<string> destinationChanges = new Subject<string>();

        private static void DestinationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TransferProgressControl).destinationChanges.OnNext(e.NewValue as string);
        }





        public PathType SourceType
        {
            get { return (PathType)GetValue(SourceTypeProperty); }
            set { SetValue(SourceTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PathType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceTypeProperty =
            DependencyProperty.Register("PathType", typeof(PathType), typeof(TransferProgressControl), new PropertyMetadata(PathType.Directory));



        public PathType DestinationType
        {
            get { return (PathType)GetValue(DestinationTypeProperty); }
            set { SetValue(DestinationTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DestinationType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DestinationTypeProperty =
            DependencyProperty.Register("DestinationType", typeof(PathType), typeof(TransferProgressControl), new PropertyMetadata(PathType.Directory));





    }
}
