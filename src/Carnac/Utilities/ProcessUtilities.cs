using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Carnac.Utilities {
    public static class ProcessUtilities {
        private static Mutex mutex;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowEnabled(IntPtr hWnd);

        /// Determine if the current process is already running
        public static bool ThisProcessIsAlreadyRunning() {
            string fullName = Application.Current.GetType().Assembly.FullName;
            mutex = new Mutex(false, fullName, out bool createdNew);
            return !createdNew;
        }

        public static void DestroyMutex() {
            if (mutex == null) {
                return;
            }

            mutex.Dispose();
            mutex = null;
        }

        /// Set focus to the previous instance of the specified program.
        public static void SetFocusToPreviousInstance(string windowCaptionPart) {
            IntPtr hWnd = IntPtr.Zero;
            foreach (Process process in Process.GetProcesses()) {
                if (process.MainWindowTitle.Contains(windowCaptionPart)) {
                    hWnd = process.MainWindowHandle;
                }
            }
            // Look for previous instance of this program.
            //IntPtr  = FindWindow(null, windowCaption);
            // If a previous instance of this program was found...
            if (hWnd != null) {
                // Is it displaying a popup window?
                IntPtr hPopupWnd = GetLastActivePopup(hWnd);
                // If so, set focus to the popup window. Otherwise set focus
                // to the program's main window.
                if (hPopupWnd != null && IsWindowEnabled(hPopupWnd)) {
                    hWnd = hPopupWnd;
                }

                _ = SetForegroundWindow(hWnd);
                // If program is minimized, restore it.
                if (IsIconic(hWnd)) {
                    _ = ShowWindow(hWnd, SW_RESTORE);
                }
            }
        }
    }
}
