using Carnac.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carnac.Logic {
    public class ShortcutAccumulator {
        private List<KeyShortcut> possibleKeyShortcuts;
        private Message[] messages;
        private readonly List<KeyPress> keys;

        public ShortcutAccumulator() {
            keys = new List<KeyPress>();
        }

        public IEnumerable<KeyPress> Keys => keys;

        public ShortcutAccumulator ProcessKey(IShortcutProvider shortcutProvider, KeyPress key) {
            if (HasCompletedValue) {
                return new ShortcutAccumulator().ProcessKey(shortcutProvider, key);
            }

            if (!keys.Any()) {
                List<KeyShortcut> possibleShortcuts = shortcutProvider.GetShortcutsStartingWith(key);
                if (possibleShortcuts.Any()) {
                    BeginShortcut(key, possibleShortcuts);
                } else {
                    Complete(key);
                }

                return this;
            }

            Add(key);
            return this;
        }

        public Message[] GetMessages() {
            return !HasCompletedValue ? throw new InvalidOperationException() : messages;
        }

        public bool HasCompletedValue { get; private set; }

        private void Add(KeyPress key) {
            bool isFirstKey = keys.Count == 0;

            if (!isFirstKey && keys[0].Process.ProcessName != key.Process.ProcessName) {
                NoMatchingShortcut();
                return;
            }

            keys.Add(key);
            List<KeyShortcut> newPossibleShortcuts = possibleKeyShortcuts.Where(s => s.StartsWith(keys)).ToList();

            EvaluateShortcuts(newPossibleShortcuts);
        }

        private void EvaluateShortcuts(List<KeyShortcut> newPossibleShortcuts) {
            if (!newPossibleShortcuts.Any()) {
                NoMatchingShortcut();
            } else if (newPossibleShortcuts.Any(s => s.IsMatch(keys))) {
                ShortcutCompleted(newPossibleShortcuts.First(s => s.IsMatch(keys)));
            } else {
                possibleKeyShortcuts = newPossibleShortcuts;
            }
        }

        private void BeginShortcut(KeyPress key, List<KeyShortcut> possibleShortcuts) {
            keys.Add(key);
            EvaluateShortcuts(possibleShortcuts);
        }

        private void ShortcutCompleted(KeyShortcut shortcut) {
            if (HasCompletedValue) {
                throw new InvalidOperationException();
            }

            messages = new[] { new Message(Keys, shortcut, true) };
            HasCompletedValue = true;
        }

        private void NoMatchingShortcut() {
            if (HasCompletedValue) {
                throw new InvalidOperationException();
            }

            // When we have no matching shortcut just break all key presses into individual messages
            HasCompletedValue = true;
            messages = keys.Select(k => new Message(k)).ToArray();
        }

        private void Complete(KeyPress key) {
            if (HasCompletedValue) {
                throw new InvalidOperationException();
            }

            HasCompletedValue = true;
            messages = new[] { new Message(key) };
        }
    }
}