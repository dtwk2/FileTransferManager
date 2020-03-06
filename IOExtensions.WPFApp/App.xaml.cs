using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IOExtensions.WPFApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        void app_Startup(object sender, StartupEventArgs e)
        {
            var args = e.Args;

            if (args.Length > 0 && args[0] == "No_Test")
            {
                MainWindow = new MainWindow(args[1], args[2]);
            }
            if (args.Length == 0 || args[0] == "Test")
            {
                MainWindow = new SecondaryWindow();
            }
            MainWindow.Show();
        }
    }
}
