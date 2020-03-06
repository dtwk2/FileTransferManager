using System;
using System.Collections.Generic;
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

namespace IOExtensions.WPFApp
{
    /// <summary>
    /// Interaction logic for MultiStageDefaultProgress.xaml
    /// </summary>
    public partial class MultiStageDefaultProgressView : UserControl
    {
        public MultiStageDefaultProgressView()
        {
            InitializeComponent();

            //FileProgressView1.Source = "..\\..\\..\\Data\\HFRR2020.7z";
            //FileProgressView1.Destination = "..\\..\\..\\..\\Destination";
            //FileProgressView1.Transferer = new ReactiveAsynExtract();
            //FileProgressView1.Title = "Unzipping";

            //FileProgressView2.Source = "C:\\Users\\declan.taylor\\Downloads";
            //FileProgressView2.Destination = "..\\..\\..\\bin\\Debug\\";
            //FileProgressView2.Transferer = new ReactiveAsyncCopy();
            //FileProgressView2.Title = "Copying";

        }

    }
}
