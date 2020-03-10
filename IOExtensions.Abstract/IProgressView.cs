using System;
using System.Windows.Input;

namespace IOExtensions.Abstract
{
    public interface IProgressView
    {
        IObservable<TimeSpan> CompleteEvents { get; }

        ICommand RunCommand { get;  }
    }
}