using Carnac.Logic.Models;
using System.Collections.Generic;
using System.Linq;

namespace Carnac.Logic {
    public class KeyShortcut {
        private readonly KeyPressDefinition[] keyCombinations;

        public KeyShortcut(string name, params KeyPressDefinition[] keyCombinations) {
            Name = name;
            this.keyCombinations = keyCombinations;
        }

        public string Name { get; private set; }

        public bool StartsWith(IEnumerable<KeyPressDefinition> keyPresses) {
            int index = 0;
            return keyPresses.All(keyPress => keyCombinations.Length > index && keyCombinations[index++].Equals(keyPress));
        }

        public bool IsMatch(IEnumerable<KeyPress> keyPresses) {
            int index = 0;
            return keyPresses.All(keyPress => keyCombinations.Length > index && keyCombinations[index++].Equals(keyPress)) && index == keyCombinations.Length;
        }
    }
}