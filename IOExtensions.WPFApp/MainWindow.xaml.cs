using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
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
using SevenZip;
using Path = System.IO.Path;

namespace IOExtensions.WPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Subject<bool> subject = new Subject<bool>();

        public MainWindow(string source, string destination)
        {
            InitializeComponent();

            var arr = this.Resources["FileProgressViewArray"] as Array;
            var dict = arr.Cast<FileProgressView>().ToDictionary(a=>a.Title,a=>a);
            var temp = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(source));
            dict["Unzipping"].Source = source;
            dict["Unzipping"].Destination = temp;
            dict["Copying"].Source = temp;
            dict["Copying"].Destination = destination;
        }



        private void ButtonBase2_OnClick(object sender, RoutedEventArgs e)
        {
            subject.OnNext(true);
        }
    }
}
