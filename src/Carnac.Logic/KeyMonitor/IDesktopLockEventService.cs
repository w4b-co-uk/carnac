using Microsoft.Win32;
using System;

namespace Carnac.Logic.KeyMonitor {
    public interface IDesktopLockEventService {
        IObservable<SessionSwitchEventArgs> GetSessionSwitchStream();
    }
}