using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Carnac {
    public class CarnacTrayIcon: IDisposable {
        private readonly NotifyIcon trayIcon;

        public CarnacTrayIcon() {
            MenuItem exitMenuItem = new MenuItem {
                Text = Properties.Resources.ShellView_Exit
            };

            System.IO.Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Carnac.icon.embedded.ico");

            trayIcon = new NotifyIcon {
                Icon = new Icon(iconStream),
                ContextMenu = new ContextMenu(new[] { exitMenuItem })
            };

            exitMenuItem.Click += (sender, args) => {
                trayIcon.Visible = false;
                Application.Current.Shutdown();
            };
            trayIcon.MouseClick += NotifyIconClick;
            trayIcon.Visible = true;
        }

        public event Action OpenPreferences = () => { };

        private void NotifyIconClick(object sender, MouseEventArgs mouseEventArgs) {
            if (mouseEventArgs.Button == MouseButtons.Left) {
                Window preferencesWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.Name == "PreferencesViewWindow");
                if (preferencesWindow != null) {
                    _ = preferencesWindow.Activate();
                } else {
                    OpenPreferences();
                }
            }
        }

        public void Dispose() {
            trayIcon.Dispose();
        }
    }
}