using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Carnac.UI;
using Carnac.Utilities;
using SettingsProviderNet;
using System.Net;
using System.Windows;

namespace Carnac {
    public partial class App {
        private readonly SettingsProvider settingsProvider;
        private readonly IMessageProvider messageProvider;
        private readonly PopupSettings settings;
        private KeyShowView keyShowView;
        private CarnacTrayIcon trayIcon;
        private KeysController carnac;

#if !DEBUG
        readonly string carnacUpdateUrl = "https://github.com/Code52/carnac";
#endif

        public App() {
            settingsProvider = new SettingsProvider(new RoamingAppDataStorage("Carnac"));
            settings = settingsProvider.GetSettings<PopupSettings>();
            KeyProvider keyProvider = new KeyProvider(InterceptKeys.Current, new PasswordModeService(), new DesktopLockEventService(), settingsProvider);
            messageProvider = new MessageProvider(new ShortcutProvider(), keyProvider, settings);
        }

        protected override void OnStartup(StartupEventArgs e) {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            // Check if there was instance before this. If there was-close the current one.  
            if (ProcessUtilities.ThisProcessIsAlreadyRunning()) {
                ProcessUtilities.SetFocusToPreviousInstance("Carnac");
                Shutdown();
                return;
            }

            trayIcon = new CarnacTrayIcon();
            trayIcon.OpenPreferences += TrayIconOnOpenPreferences;
            KeyShowViewModel keyShowViewModel = new KeyShowViewModel(settings);
            keyShowView = new KeyShowView(keyShowViewModel);
            keyShowView.Show();

            carnac = new KeysController(keyShowViewModel.Messages, messageProvider, new ConcurrencyService(), settingsProvider);
            carnac.Start();

#if !DEBUG
            if (settings.AutoUpdate)
            {
                Observable
                    .Timer(TimeSpan.FromMinutes(5))
                    .Subscribe(async x =>
                    {
                        try
                        {
                            using (var mgr = UpdateManager.GitHubUpdateManager(carnacUpdateUrl))
                            {
                                await mgr.Result.UpdateApp();
                            }
                        }
                        catch
                        {
                            // Do something useful with the exception
                        }
                    });
            }
#endif

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e) {
            trayIcon.Dispose();
            carnac.Dispose();
            ProcessUtilities.DestroyMutex();

            base.OnExit(e);
        }

        private void TrayIconOnOpenPreferences() {
            PreferencesViewModel preferencesViewModel = new PreferencesViewModel(settingsProvider, new ScreenManager());
            PreferencesView preferencesView = new PreferencesView(preferencesViewModel);
            preferencesView.Show();
        }
    }
}
