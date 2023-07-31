using Carnac.Logic.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Carnac.Logic {
    public class ScreenManager: IScreenManager {
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("User32.dll")]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        public IEnumerable<DetailedScreen> GetScreens() {
            List<DetailedScreen> screens = new List<DetailedScreen>();

            int index = 1;
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            try {
                for (uint id = 0; EnumDisplayDevices(null, id, ref d, 0); id++) {
                    d.cb = Marshal.SizeOf(d);

                    DISPLAY_DEVICE x = new DISPLAY_DEVICE();
                    x.cb = Marshal.SizeOf(x);

                    //Get the actual monitor
                    _ = EnumDisplayDevices(d.DeviceName, 0, ref x, 0);

                    if (string.IsNullOrEmpty(x.DeviceName) || string.IsNullOrEmpty(x.DeviceString)) {
                        continue;
                    }

                    DetailedScreen screen = new DetailedScreen { FriendlyName = x.DeviceString, Index = index++ };

                    DEVMODE mode = new DEVMODE();
                    mode.dmSize = (ushort)Marshal.SizeOf(mode);
                    if (EnumDisplaySettings(d.DeviceName, -1, ref mode)) {
                        screen.Width = (int)mode.dmPelsWidth;
                        screen.Height = (int)mode.dmPelsHeight;
                        screen.Top = mode.dmPosition.y;
                        screen.Left = mode.dmPosition.x;
                    }

                    // skip this value if it doesn't appear to be a valid screen
                    if (screen.Width == 0 || screen.Height == 0) {
                        continue;
                    }

                    screens.Add(screen);
                }
            } catch (Exception) {
                //log this
            }

            DetailedScreen biggestScreen = screens.OrderByDescending(s => s.Width).FirstOrDefault();
            if (biggestScreen != null) {
                double maxWidth = biggestScreen.Width;
                foreach (DetailedScreen s in screens) {
                    s.RelativeWidth = 200 * (s.Width / maxWidth);
                    s.RelativeHeight = s.RelativeWidth * (s.Height / s.Width);
                }
            }

            screens = screens.OrderBy(s => s.Top).ThenBy(s => s.Left).ToList();

            return screens;
        }
    }
}