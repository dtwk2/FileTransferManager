using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using IOExtensions.Abstract;
using IOExtensions.Core;

namespace IOExtensions.Reactive
{

    public class ReactiveAsyncCopy : Transferer
    {
        private Subject<TransferProgress> transferProgress = new Subject<TransferProgress>();

        public override IObservable<TransferProgress> Transfer(string source, string destination)
        {

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
