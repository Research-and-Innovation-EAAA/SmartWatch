using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDataReceiver
{
    interface IProgressObserver
    {
        void Notify(int progress);
    }

    interface IProgressSubject
    {
        void RegisterObserver(IProgressObserver observer);
        void UnregisterObserver(IProgressObserver observer);
        void NotifyObservers(int progress);
    }
}
