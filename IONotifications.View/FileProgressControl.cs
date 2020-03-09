using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for FileProgressControl.xaml
    /// </summary>
    public partial class FileProgressControl : ProgressControl
    {
        private Button BrowseSource;
        private Button BrowseDestination;
        private TextBox txtDestination;
        private TextBox txtSource;



        static FileProgressControl()
        {
            //  DefaultStyleKeyProperty.OverrideMetadata(typeof(FileProgressControl), new FrameworkPropertyMetadata(typeof(FileProgressControl)));
        }

        public FileProgressControl() : base()
        {
            this.sourceChanges.CombineLatest(this.templateApplied,(a,b)=>a).Subscribe(a => { txtSource.Text = a; });
            this.destinationChanges.CombineLatest(this.templateApplied, (a, b) => a).Subscribe(a => { txtDestination.Text = a; });
        

        }

        public override void OnApplyTemplate()
        {

            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("/IOExtensions.View;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
            var uGrid = myResourceDictionary["Expander1"] as UniformGrid;

            this.ConfigContent = uGrid;

            ;
            this.BrowseSource = uGrid.FindChild<Button>("BrowseSource");
            this.BrowseDestination = uGrid.FindChild<Button>("BrowseDestination");
            this.txtDestination = uGrid.FindChild<TextBox>("txtDestination");
            this.txtSource = uGrid.FindChild<TextBox>("txtSource");

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

            obs.CombineLatest(templateApplied,(a,b)=>a).Subscribe(a =>
            {
                TitleTextBlock.Visibility = Visibility.Visible;
                transferButton.Visibility = Visibility.Collapsed;
            });

            obs
                .SelectMany(abc =>
                {
                    var (source, destination, transferer) = abc;
                    return transferer.Transfer(source, destination)
                        .Scan((date: DateTime.Now, default(TimeSpan), default(TransferProgress)),
                            (d, t) => (d.date, DateTime.Now - d.date, t));
                })
                .CombineLatest(templateApplied, (a, b) => a)
                .Subscribe(a =>
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


        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(FileProgressControl), new PropertyMetadata(null, SourceChanged));

        private readonly Subject<string> sourceChanges = new Subject<string>();

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressControl).sourceChanges.OnNext(e.NewValue as string);
        }


        public string Destination
        {
            get { return (string)GetValue(DestinationProperty); }
            set { SetValue(DestinationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Destination.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DestinationProperty =
            DependencyProperty.Register("Destination", typeof(string), typeof(FileProgressControl), new PropertyMetadata(null, DestinationChanged));

        private readonly Subject<string> destinationChanges = new Subject<string>();

        private static void DestinationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressControl).destinationChanges.OnNext(e.NewValue as string);
        }


    }
}
