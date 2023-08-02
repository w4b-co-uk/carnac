using Carnac.Logic.Internal;
using Carnac.Logic.KeyMonitor;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Carnac.Logic {
    public class PasswordModeService: IPasswordModeService {
        private readonly InterceptKeyEventArgsEqualityComparer comparer = new InterceptKeyEventArgsEqualityComparer();
        private readonly FixedQueue<InterceptKeyEventArgs> log;
        private InterceptKeyEventArgs[] passwordKeyCombination;
        private bool currentMode;

        public PasswordModeService() {
            log = new FixedQueue<InterceptKeyEventArgs>(PasswordKeyCombination.Count());
        }

        public bool CheckPasswordMode(InterceptKeyEventArgs key) {
            log.Enqueue(key);
            List<InterceptKeyEventArgs> sortedLog = log.ToList();
            sortedLog.Sort();
            bool isMatch = sortedLog.SequenceEqual(PasswordKeyCombination, comparer);
            if (isMatch) {
                currentMode = !currentMode;
                log.Clear();
                return true; //this way when the sequence is entered again to EXIT password mode, the key password keycombo doesn't show on screen
            }

            return currentMode;
        }

        public IEnumerable<InterceptKeyEventArgs> PasswordKeyCombination {
            get {
                if (passwordKeyCombination == null) {
                    passwordKeyCombination = new[]
                                                 {
                                                     new InterceptKeyEventArgs(Keys.P, KeyDirection.Down,true,true,false),
                                                 };
                }

                return passwordKeyCombination;
            }
        }

        private class InterceptKeyEventArgsEqualityComparer: IEqualityComparer<InterceptKeyEventArgs> {
            public bool Equals(InterceptKeyEventArgs x, InterceptKeyEventArgs y) {
                return (x == null && y == null) || (x != null && y != null && x.Key == y.Key
                       && x.ShiftPressed == y.ShiftPressed
                       && x.AltPressed == y.AltPressed
                       && x.ControlPressed == y.ControlPressed);
            }

            public int GetHashCode(InterceptKeyEventArgs obj) {
                return obj.Key.GetHashCode() << obj.AltPressed.GetHashCode()
                       << obj.ShiftPressed.GetHashCode() << obj.ControlPressed.GetHashCode();
            }
        }
    }
}