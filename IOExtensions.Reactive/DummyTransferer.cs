using IOExtensions.Abstract;
using IOExtensions.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IOExtensions.Reactive
{
    public class DummyTransferer : ITransferer
    {
        public IObservable<ITransferProgress> Transfer(params string[] args)
        {

            var obs = Task.Delay(TimeSpan.FromSeconds(2)).ToObservable()
                .Select(a => DateTime.Now)
                .StartWith(DateTime.Now).Select(a => new TransferProgress(a, 50) { Total = 100 });

            var obs2 = Task.Delay(TimeSpan.FromSeconds(2)).ToObservable()
                .Select(a => DateTime.Now)
                .StartWith(DateTime.Now)
                .Select(a => new TransferProgress(a, 100) { Total = 100 });

            return obs.Concat(obs2).Cast<ITransferProgress>();
        }
    }
}