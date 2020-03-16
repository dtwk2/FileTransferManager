using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace IOExtensions.View
{
    public class FileBrowser : PathBrowser
    {
        public FileBrowser()
        {
            _ = applyTemplateSubject.Select(a => ButtonOne.ToClicks())
                .SelectMany(a => a)
                .CombineLatest(filterChanges.StartWith(Filter).DistinctUntilChanged(),
                    extensionChanges.StartWith(Extension).DistinctUntilChanged(),
                    (a, b, c) => OpenDialog(b, c))
                .Where(output => output.result ?? false)
                .ObserveOnDispatcher()
                .Select(output => output.path)
                .Subscribe(textChanges.OnNext);
        }

        protected override (bool? result, string path) OpenDialog(string filter, string extension)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            //  dlg.DefaultExt = ".png";
            //   dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            if (extension != null)
                dlg.DefaultExt = extension.StartsWith(".") ? extension : "." + extension;
            if (filter != null)
                dlg.Filter = filter;

            bool? result = dlg.ShowDialog();

            // if dialog closed dlg.FileName may become inaccessible; hence check for result equal to true
            return result == true ? (result, dlg.FileName) : (result, null);
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(PathBrowser), new PropertyMetadata(null, FilterChanged));

        private Subject<string> filterChanges = new Subject<string>();

        private static void FilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileBrowser).filterChanges.OnNext((string)e.NewValue);
        }

        public string Extension
        {
            get { return (string)GetValue(ExtensionProperty); }
            set { SetValue(ExtensionProperty, value); }
        }

        public static readonly DependencyProperty ExtensionProperty =
            DependencyProperty.Register("Extension", typeof(string), typeof(PathBrowser), new PropertyMetadata(null, ExtensionChanged));

        private Subject<string> extensionChanges = new Subject<string>();

        private static void ExtensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FileBrowser).extensionChanges.OnNext((string)e.NewValue);
        }
    }
}