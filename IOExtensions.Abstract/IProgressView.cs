using System;
using System.Windows;
using System.Windows.Input;

namespace IOExtensions.View
{
    public interface IProgressView
    {
        IObservable<TimeSpan> CompleteEvents { get; }

        ICommand RunCommand { get;  }
    }
}