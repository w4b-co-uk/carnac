using Carnac.Logic.Models;
using System.Collections.Generic;

namespace Carnac.Logic {
    public interface IShortcutProvider {
        List<KeyShortcut> GetShortcutsStartingWith(KeyPress keyPress);
    }
}