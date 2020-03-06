using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using IOExtensions.Abstract;
using IOExtensions.Core;
using SevenZip;

namespace IOExtensions.Reactive
{

    public class ReactiveAsynExtract : Transferer
    {
        private string dll7z = AppDomain.CurrentDomain.BaseDirectory + "7z.dll";
        private string dllSevenZipSharp = AppDomain.CurrentDomain.BaseDirectory + "SevenZipSharp.dll";

        private Subject<TransferProgress> transferProgress = new Subject<TransferProgress>();

        public override IObservable<TransferProgress> Transfer(string source, string destinationDirectory)
        {

            async void Init()
            {
                if (System.IO.File.Exists(source) && System.IO.Directory.Exists(destinationDirectory))
                {
                    SevenZipExtractor.SetLibraryPath(dll7z);

                    var extractor = new SevenZipExtractor(source);
                    int max = extractor.ArchiveFileData.Count;
                    DateTime start = new DateTime();
                    int current = 0;
                    extractor.FileExtractionStarted += (a, b) =>
                    {
                        ++current;
                        transferProgress.OnNext(new TransferProgress(start, current) { Total = max });
                    };
                    extractor.BeginExtractArchive(destinationDirectory);
                }
            }


            Init();

            return transferProgress;
        }
    }
}

//fileCopyStartTimestamp, partialProgress.BytesTransferred)
//{
//Total = rootSourceSize.Size,
//Transferred = totalTransfered + partialProgress.Transferred,
//BytesTransferred = totalTransfered + partialProgress.Transferred,
//StreamSize = rootSourceSize.Size,
//ProcessedFile = file.FullName
//};



//void extr_FileExtractionStarted(object sender, FileInfoEventArgs e)
//{
////label1.Content = String.Format("更新文件 {0}", e.FileInfo.FileName);
//progressBar1.Value += 1;
//CurrentValue += 1;
//ImageProgressBar(CurrentValue, MaxValue, 193, image1);

//    if (progressBar1.Maximum == progressBar1.Value)
//label1.Content = "游戏更新完成";
//}

//private void Check7zAndSevenZipSharpDll()
//{
//bool b1 = System.IO.File.Exists(dll7z);
//bool b2 = System.IO.File.Exists(dllSevenZipSharp);

//    if (!b1)
//{
//    MessageBox.Show("7z.dll 不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
//    // Close();
//}

//if (!b2)
//{
//    MessageBox.Show("SevenZipSharp.dll 不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
//    // Close();
//}
//}