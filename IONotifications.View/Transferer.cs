using System;
using System.Collections.Generic;
using System.Text;
using IOExtensions.Abstract;
using IOExtensions.Core;

namespace IOExtensions.View
{
    public interface IBasicTransferer : ITransferer
    {
        public void Transferer();
    }
}
