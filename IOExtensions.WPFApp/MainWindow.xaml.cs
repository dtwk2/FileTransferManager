using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IOExtensions.Reactive;

namespace IOExtensions.WPFApp
{
    /// <summary>
    /// Interaction logic for SecondaryWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private readonly Subject<bool> subject = new Subject<bool>();

        public MainWindow()
        {
            InitializeComponent();

            FileProgressView1.Source = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile) + @"\Downloads";
            FileProgressView1.Transferer = new ReactiveAsyncCopy();
            FileProgressView1.Destination = "..\\..\\..\\bin\\Debug\\";


            FileProgressView3.Source = "..\\..\\..\\Data\\Book.7z";
            FileProgressView3.Destination = "..\\..\\..\\Data\\Destination";
            System.IO.Directory.CreateDirectory(FileProgressView3.Destination);
            FileProgressView3.Transferer = new ReactiveAsynExtract();

          //  CreateDummyFile();

        }

        private void CreateDummyFile()
        {
            if (!File.Exists(@"../../../Data/huge_dummy_file"))
            {
                FileStream fs = new FileStream(@"../../../Data/huge_dummy_file", FileMode.CreateNew);
                fs.Seek(500L * 1024 * 1024, SeekOrigin.Begin);
                fs.WriteByte(0);
                fs.Close();
            }
        }

        private void ButtonBase2_OnClick(object sender, RoutedEventArgs e)
        {
            subject.OnNext(true);
        }

        private void Show_Default_OnClick(object sender, RoutedEventArgs e)
        {
            MultiStage.Content = new MultiStageProgressView();
        }
    }
}
