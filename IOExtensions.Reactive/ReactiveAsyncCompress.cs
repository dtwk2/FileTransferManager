using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using IOExtensions.Abstract;
using IOExtensions.Core;
using SevenZip;

namespace IOExtensions.Reactive
{

    public class ReactiveAsynCompress : ITransferer
    {
        private string dll7z = AppDomain.CurrentDomain.BaseDirectory + "7z.dll";

        private Subject<TransferProgress> transferProgress = new Subject<TransferProgress>();

        public IObservable<ITransferProgress> Transfer(params string[] args)
        {
            string source = args[0];
            string destination = args[1];

            void Init()
            {
                if (System.IO.Directory.Exists(source) && Directory.GetParent(destination).Exists)
                {
                    SevenZipBase.SetLibraryPath(dll7z);
                    var compressor = new SevenZipCompressor(source);
                    DateTime start = new DateTime();
                    compressor.CompressionMode = CompressionMode.Create;
                    compressor.TempFolderPath = System.IO.Path.GetTempPath();
                    compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
                    compressor.Compressing += (a, b) =>
                    {
                        transferProgress.OnNext(new TransferProgress(start, b.PercentDone) { Total = 100 });
                    };
                    //var path = Path.Combine(destination, new DirectoryInfo(source).Name + ".7z");
                    //System.IO.File.Delete(path);
                    compressor.BeginCompressDirectory(source, destination);
                }
                else
                {
                    var sourceExists = System.IO.Directory.Exists(source) ? string.Empty : "not";
                    var destinationExists = System.IO.File.Exists(destination) ? string.Empty : "not";
                    throw new Exception($"Source does {sourceExists} exist & destination's parent does {destinationExists} exist.");
                }
            }


            Init();

            return transferProgress.Cast<ITransferProgress>();
        }
    }
}

