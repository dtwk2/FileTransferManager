using IOExtensions.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOExtensions.Abstract
{
    public interface ITransferer 
    {
        IObservable<TransferProgress> Transfer(params string[] args);
    }

}
