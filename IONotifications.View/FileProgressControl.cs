﻿using System;
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
        private Button BrowseFile;
        private TextBox txtFile;


        static FileProgressControl()
        {
            //  DefaultStyleKeyProperty.OverrideMetadata(typeof(FileProgressControl), new FrameworkPropertyMetadata(typeof(FileProgressControl)));
        }

        public FileProgressControl() : base()
        {
            this.fileChanges.CombineLatest(this.templateApplied,(a,b)=>a).Subscribe(a => { txtFile.Text = a; });
        }

        public override void OnApplyTemplate()
        {

            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("/IOExtensions.View;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
            var uGrid = myResourceDictionary["FileControl"] as StackPanel;

            this.ConfigContent = uGrid;

            this.BrowseFile = uGrid.FindChild<Button>("BrowseFile");
            this.txtFile = uGrid.FindChild<TextBox>("txtFile");

            if (File != null)
            {
                fileChanges.OnNext(File);
            }


            base.OnApplyTemplate();
        }


        protected override void ScheduleProgress()
        {
            var obs = transferButtonsClicks
                .WithLatestFrom(this.fileChanges.DistinctUntilChanged()
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


        public string File
        {
            get { return (string)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for File.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileProperty =
            DependencyProperty.Register("File", typeof(string), typeof(FileProgressControl), new PropertyMetadata(null, SourceChanged));

        private readonly Subject<string> fileChanges = new Subject<string>();

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileProgressControl).fileChanges.OnNext(e.NewValue as string);
        }


     

    }
}
