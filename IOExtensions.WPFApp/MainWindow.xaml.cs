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
using SevenZip;

namespace IOExtensions.WPFApp
{

    /// </summary>
    public partial class MainWindow : Window
    {

        private string dll7z = AppDomain.CurrentDomain.BaseDirectory + "7z.dll";

        private readonly Subject<bool> subject = new Subject<bool>();

        public MainWindow()
        {
            InitializeComponent();


            FileProgressView1.Source = @"../../../Data/DummyFile/huge_dummy_file.7z";
            FileProgressView1.Transferer = new ReactiveAsyncCopy();

            FileProgressView1.Destination = "../../../Data/Destination";



            FileProgressView3.Destination = "..\\..\\..\\Data\\Destination";
            System.IO.Directory.CreateDirectory(FileProgressView3.Destination);
            FileProgressView3.Transferer = new ReactiveAsynExtract();


            ProgressView1.Transferer = new DummyTransferer();
            CreateDummyFile();
            CreateDummyZipFile();

        }

        private void CreateDummyZipFile()
        {
            if (File.Exists(@"../../../Data/DummyFile/huge_dummy_file.7z") == false)
            {
                SevenZipBase.SetLibraryPath(dll7z);

                var compressor = new SevenZipCompressor();

                compressor.CompressionMode = CompressionMode.Create;
                compressor.TempFolderPath = System.IO.Path.GetTempPath();
                compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
                compressor.CompressDirectory(@"../../../Data/DummyFile", @"../../../Data/DummyFile/huge_dummy_file.7z"); //Error
            }
        }

        private void CreateDummyFile()
        {

            if (!File.Exists(@"../../../Data/DummyFile/huge_dummy_file"))
            {

                Directory.CreateDirectory(@"../../../Data/DummyFile");
                FileStream fs = new FileStream(@"../../../Data/DummyFile/huge_dummy_file", FileMode.CreateNew);
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
