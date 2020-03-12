﻿using System;
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
                    string[] fileList = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                    foreach (var item in DeleteProgress(fileList))
                        transferProgress.OnNext(item);
                });

                IEnumerable<TransferProgress> DeleteProgress(IList<string> fileList)
                {
                    DateTime dateTime = DateTime.Now;
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        try
                        {
                            new FileInfo(fileList[i]).Delete();
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
