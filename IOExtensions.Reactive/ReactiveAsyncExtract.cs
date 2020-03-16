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

    public class ReactiveAsynExtract : ITransferer
    {
        private string dll7z = AppDomain.CurrentDomain.BaseDirectory + "7z.dll";

        private Subject<TransferProgress> transferProgress = new Subject<TransferProgress>();

        public  IObservable<ITransferProgress> Transfer(params string[] args)
        {
            string source = args[0];
            string destination = args[1];

            void Init()
            {
                if (System.IO.File.Exists(source) && System.IO.Directory.GetParent(destination).Exists)
                {
                    System.IO.Directory.CreateDirectory(destination);

                    SevenZipBase.SetLibraryPath(dll7z);

                    var extractor = new SevenZipExtractor(source);
                    int max = extractor.ArchiveFileData.Count;
                    DateTime start = new DateTime();
                    int current = 0;
                    extractor.FileExtractionStarted += (a, b) =>
                    {
                        ++current;
                        transferProgress.OnNext(new TransferProgress(start, current) { Total = max });
                    };
                    extractor.BeginExtractArchive(destination);
                }
                else
                {
                    var sourceExists = System.IO.File.Exists(source) ? string.Empty : "not";
                    var destinationExists = System.IO.Directory.GetParent(destination).Exists ? string.Empty : "not";
                    throw new Exception($"Source does {sourceExists } exist & destination parent does {destinationExists} exist.");
                }
            }


            Init();

            return transferProgress.Cast<ITransferProgress>();
        }
    }
}

