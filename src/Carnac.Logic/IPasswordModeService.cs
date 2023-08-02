using Carnac.Logic.KeyMonitor;
using System.Collections.Generic;

namespace Carnac.Logic {
    public interface IPasswordModeService {
        bool CheckPasswordMode(InterceptKeyEventArgs key);
        IEnumerable<InterceptKeyEventArgs> PasswordKeyCombination { get; }
    }
}