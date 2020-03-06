using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Controls;

namespace IOExtensions.View
{
    public class MultiStageProgress : MultiFileProgress
    {
        private StepBar stepBar;
        private CircleProgressBar circleProgressBar;

        static MultiStageProgress()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiStageProgress), new FrameworkPropertyMetadata(typeof(MultiStageProgress)));
        }

        public override void OnApplyTemplate()
        {
            this.stepBar = this.GetTemplateChild("StepBar1") as StepBar;
            this.circleProgressBar = this.GetTemplateChild("CircleProgressBar1") as CircleProgressBar;
            circleProgressBar.Value = 100d;
            stepBar.ItemsSource = SelectStepBars().ToArray();
            stepBar.Visibility = Visibility.Collapsed;
        }

        protected override async Task<Unit> StepChange(FileProgressView progressView)
        {

            stepBar.Visibility = Visibility.Visible;
            circleProgressBar.Visibility = Visibility.Collapsed;
            var task = await base.StepChange(progressView);
            stepBar.Next();
            return task;

        }

        protected override void CountDownChange(int a)
        {
            circleProgressBar.Value = (100d * a) / CountDown;
        }


        private IEnumerable<StepBarModel> SelectStepBars()
        {
            return ProgressViews.Cast<FileProgressView>()
                .Select(a =>
                    new StepBarModel
                    {
                        Header = a.Title,
                        Content = a.Details,
                    });
        }


    }
    public class StepBarModel
    {
        public string Header { get; set; }

        public string Content { get; set; }
    }

}
