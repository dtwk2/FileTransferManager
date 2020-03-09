using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using IOExtensions.Abstract;
using IOExtensions.Core;

namespace IOExtensions.Reactive
{

    public class ReactiveAsyncCopy : ITransferer
    {
        private Subject<TransferProgress> transferProgress = new Subject<TransferProgress>();

        public IObservable<TransferProgress> Transfer(params string[] args)
        {
            string source = args[0];
            string destination = args[1];
            async void Init()
            {
                await FileTransferManager.CopyWithProgressAsync(
                   source,
                   destination,
                   transferProgress.OnNext, true)
                   .ContinueWith(a =>
               {
               });

            }

            Init();

            return transferProgress;
        }
    }
}
