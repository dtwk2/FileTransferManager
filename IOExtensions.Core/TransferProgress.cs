﻿using System;
using IOExtensions.Abstract;

namespace IOExtensions.Core
{
    public class TransferProgress : ITransferProgress
    {
        public TransferProgress(DateTime startedTimestamp, long bytesTransfered)
        {
            BytesTransferred = bytesTransfered;
            BytesPerSecond = BytesTransferred / DateTime.Now.Subtract(startedTimestamp).TotalSeconds;
        }

        public long Total { get; set; }

        public long Transferred { get; set; }

        public long BytesTransferred { get; set; }

        public long StreamSize { get; set; }

        public string ProcessedFile { get; set; }

        public double BytesPerSecond { get; }

        public double Fraction => BytesTransferred / (double)Total;


        public double Percentage => 100.0 * Fraction;


        public string GetBytesTransferedFormatted(SuffixStyle suffixStyle, int decimalPlaces)
        {
            return Helpers.ToSizeWithSuffix(BytesTransferred, suffixStyle, decimalPlaces);
        }

        public string GetDataPerSecondFormatted(SuffixStyle suffixStyle, int decimalPlaces)
        {
            return string.Format("{0}/sec", Helpers.ToSizeWithSuffix((long)BytesPerSecond, suffixStyle, decimalPlaces));
        }

        public override string ToString()
        {
            return string.Format("Total: {0}, BytesTransferred: {1}, Percentage: {2}", Total, BytesTransferred, Percentage);
        }

        public string AsPercentage() => Fraction.ToString("00 %");

    }
}
