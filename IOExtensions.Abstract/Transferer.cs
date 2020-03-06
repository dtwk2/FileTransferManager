using IOExtensions.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOExtensions.Abstract
{
    public abstract class Transferer
    {
        public abstract IObservable<TransferProgress> Transfer(string source, string destination);
    }
}
