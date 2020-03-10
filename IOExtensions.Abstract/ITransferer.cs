using System;
using System.Collections.Generic;
using System.Text;

namespace IOExtensions.Abstract
{
    public interface ITransferer 
    {
        IObservable<ITransferProgress> Transfer(params string[] args);
    }

}
