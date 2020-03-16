using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace IOExtensions.View
{
    public class FolderBrowser : PathBrowser
    {
        public FolderBrowser()
        {
            _ = applyTemplateSubject.SelectMany(a => ButtonOne.ToClicks())
                .Select(a =>
                    OpenDialog(string.Empty, string.Empty))
                .Where(output => output.result ?? false)
                .ObserveOnDispatcher()
                .Select(output => output.path)
                .Subscribe(textChanges.OnNext);
        }



        public bool IsFolderPicker
        {
            get { return (bool)GetValue(IsFolderPickerProperty); }
            set { SetValue(IsFolderPickerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFolderPicker.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFolderPickerProperty =
            DependencyProperty.Register("IsFolderPicker", typeof(bool), typeof(FolderBrowser), new PropertyMetadata(true));



        //protected override (bool? result, string path) OpenDialog(string filter, string extension)
        //{
        //    var dlg = new CommonOpenFileDialog();
        //    dlg.Title = "My Title";
        //    dlg.IsFolderPicker = true;
        //    dlg.InitialDirectory = extension;

        //    dlg.AddToMostRecentlyUsedList = false;
        //    dlg.AllowNonFileSystemItems = false;
        //    dlg.DefaultDirectory = extension;
        //    dlg.EnsureFileExists = true;
        //    dlg.EnsurePathExists = true;
        //    dlg.EnsureReadOnly = false;
        //    dlg.EnsureValidNames = true;
        //    dlg.Multiselect = false;
        //    dlg.ShowPlacesList = true;


        //    if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
        //    {
        //        return (true, dlg.FileName);
        //    }

        //    return (false, string.Empty);
        //}

        //protected override (bool? result, string path) OpenDialog(string filter, string extension)
        //{
        //    FolderBrowserDialog dlg = new FolderBrowserDialog();
        //    DialogResult result = default;
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        result = dlg.ShowDialog();
        //    });
        //    return (result == DialogResult.OK || result == DialogResult.Yes, dlg.SelectedPath);

        //}

        protected override (bool? result, string path) OpenDialog(string filter, string extension)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = IsFolderPicker;
            CommonFileDialogResult result = dialog.ShowDialog();

            return result == CommonFileDialogResult.Ok ? (true, dialog.FileName) : (false, null);
        }
    }
}