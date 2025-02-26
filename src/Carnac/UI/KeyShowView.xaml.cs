﻿using Carnac.Logic;
using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Interop;

namespace Carnac.UI {
    public partial class KeyShowView {
        public KeyShowView(KeyShowViewModel keyShowViewModel) {
            DataContext = keyShowViewModel;
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            Win32Methods.SetWindowExTransparentAndNotInWindowList(hwnd);
            Timer timer = new Timer(100);
            timer.Elapsed +=
                (s, x) => SetWindowPos(hwnd,
                                 HWND.TOPMOST,
                                 0, 0, 0, 0,
                                 (uint)(SWP.NOMOVE | SWP.NOSIZE | SWP.SHOWWINDOW));

            timer.Start();

            KeyShowViewModel vm = (KeyShowViewModel)DataContext;
            Left = vm.Settings.Left;
            vm.Settings.LeftChanged += SettingsLeftChanged;
            Top = vm.Settings.Top;
            vm.Settings.TopChanged += SettingsTopChanged;
            WindowState = WindowState.Maximized;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);

        /// <summary>
        /// HWND values for hWndInsertAfter
        /// </summary>
        public static class HWND {
            public static readonly IntPtr
            NOTOPMOST = new IntPtr(-2),
            BROADCAST = new IntPtr(0xffff),
            TOPMOST = new IntPtr(-1),
            TOP = new IntPtr(0),
            BOTTOM = new IntPtr(1);
        }

        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        public static class SWP {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {

        }

        private void SettingsLeftChanged(object sender, EventArgs e) {
            WindowState = WindowState.Normal;
            KeyShowViewModel vm = (KeyShowViewModel)DataContext;
            Left = vm.Settings.Left;
            WindowState = WindowState.Maximized;
        }

        private void SettingsTopChanged(object sender, EventArgs e) {
            WindowState = WindowState.Normal;
            KeyShowViewModel vm = (KeyShowViewModel)DataContext;
            Top = vm.Settings.Top;
            WindowState = WindowState.Maximized;
        }
    }
}
