using System;
using System.Collections.Generic;
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
using IOExtensions.Reactive;
using IOExtensions.View;

namespace IOExtensions.WPFApp
{
    /// <summary>
    /// Interaction logic for MultiStageProgressView.xaml
    /// </summary>
    public partial class MultiStageProgressView : UserControl
    {
        private readonly Subject<bool> subject = new Subject<bool>();

        public MultiStageProgressView()
        {
            InitializeComponent();

 
            this.Loaded += MultiStageProgressView_Loaded;
        }

        private void MultiStageProgressView_Loaded(object sender, RoutedEventArgs e)
        {
        }


        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            string location = @"C:\Users\declan.taylor\source\repos\UpdateApplication\UsbDetectorWpf\UsbDetectorWpf3.Core\bin\Debug\netcoreapp3.1\UsbDetectorWpf3.Core.exe";
            System.Diagnostics.Process.Start(location);
            Application.Current.Shutdown();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            subject.OnNext(true);
        }
    }
}
