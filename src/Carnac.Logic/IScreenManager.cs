using Carnac.Logic.Native;
using System.Collections.Generic;

namespace Carnac.Logic {
    public interface IScreenManager {
        IEnumerable<DetailedScreen> GetScreens();
    }
}
