using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using IOExtensions.Abstract;
using IOExtensions.Core;

namespace IOExtensions.Reactive
{

    public class ReactiveAsyncDelete : ITransferer
    {
        private Subject<TransferProgress> transferProgress = new Subject<TransferProgress>();

        public IObservable<ITransferProgress> Transfer(params string[] args)
        {
            string source = args[0];
            async void Init()
            {
                await Task.Run(() =>
                {
                    if (IOExtensions.Core.Helpers.IsDirFile(source)==true )
                    {
                        string[] pathList = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                        pathList = pathList.Concat(Directory.GetDirectories(source)).Concat(new[] {source}).ToArray();
                        foreach (var item in DeleteProgress(pathList))
                            transferProgress.OnNext(item);

                    }
                    else if (IOExtensions.Core.Helpers.IsDirFile(source) == false)
                    {
                        var dtn = DateTime.Now;
                        new FileInfo(source).Delete();
                        transferProgress.OnNext(new TransferProgress(dtn, 1) { Total = 1 });
                    }
                });

                IEnumerable<TransferProgress> DeleteProgress(IList<string> fileList)
                {
                    DateTime dateTime = DateTime.Now;
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        try
                        {
                            if(IOExtensions.Core.Helpers.IsDirFile(source) ==false)
                                new FileInfo(fileList[i]).Delete();
                            if (IOExtensions.Core.Helpers.IsDirFile(source) == true)
                                new DirectoryInfo(fileList[i]).Delete();
                        }
                        catch (Exception e)
                        {
                            // TODO Need better logging
                            System.Console.WriteLine(e);
                            //throw;
                        }
                        yield return (new TransferProgress(dateTime, i + 1) { Total = fileList.Count });
                    }
                }
            }

            Init();

            return transferProgress.Cast<ITransferProgress>();
        }


    }
}
