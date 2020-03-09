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
        public string DownloadsFolder => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
    }
}
