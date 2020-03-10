using System;
using System.Collections.Generic;
using System.Text;

namespace IOExtensions.Abstract
{

    public interface ITransferProgress
    {
        long Total { get; set; }
        long Transferred { get; set; }
        long BytesTransferred { get; set; }
        long StreamSize { get; set; }
        string ProcessedFile { get; set; }
        double BytesPerSecond { get; }
        double Fraction { get; }
        double Percentage { get; }
    }

}
