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
using Prism.Commands;


namespace IOExtensions.View
{
    /// <summary>
    /// Interaction logic for FileProgressControl.xaml
    /// </summary>
    public partial class FileProgressControl : ProgressControl
    {
        private PathBrowser fileBrowser;

        static FileProgressControl()
        {
        }

        public FileProgressControl() : base()
        {
            this.pathChanges.CombineLatest(this.templateApplied,(a,b)=>a).Subscribe(a =>
            {
                if(fileBrowser is PathBrowser pathBrowser)
                pathBrowser.SetPath.Execute(a);
            });
        }

        public override void OnApplyTemplate()
        {

      
            fileBrowser = PathType == PathType.File ? (PathBrowser)new FileBrowser() : new FolderBrowser();
            fileBrowser.TextChange += (s, e) => Path = e.Text;
            ConfigContent ??= fileBrowser;

            if (Path != null)
            {
                pathChanges.OnNext(Path);
            }

            base.OnApplyTemplate();
        }


        protected override void ScheduleProgress()
        {
            var obs = transferButtonsClicks
                .WithLatestFrom(this.pathChanges.DistinctUntilChanged()
                        .CombineLatest(
                            this.transfererChanges,
                            (a, b) => (a, b)),
                    (a, b) => b);

            obs.CombineLatest(templateApplied,(a,b)=>a).Subscribe(a =>
            {
                TitleTextBlock.Visibility = Visibility.Visible;
                transferButton.Visibility = Visibility.Collapsed;
            });

            obs
                .SelectMany(abc =>
                {
                    var (source, transferer) = abc;
                    return transferer.Transfer(source)
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


        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(FileProgressControl), new PropertyMetadata(null, PathChanged));

        private readonly Subject<string> pathChanges = new Subject<string>();

        private static void PathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressControl).pathChanges.OnNext(e.NewValue as string);
        }



        public PathType PathType
        {
            get { return (PathType)GetValue(PathTypeProperty); }
            set { SetValue(PathTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PathType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathTypeProperty =
            DependencyProperty.Register("PathType", typeof(PathType), typeof(FileProgressControl), new PropertyMetadata(PathType.Directory));



    }
}
