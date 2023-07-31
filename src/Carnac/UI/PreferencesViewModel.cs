using Carnac.Logic;
using Carnac.Logic.Enums;
using Carnac.Logic.Models;
using Carnac.Logic.Native;
using SettingsProviderNet;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;

namespace Carnac.UI {
    public class PreferencesViewModel: NotifyPropertyChanged {
        private readonly ISettingsProvider settingsProvider;

        public PreferencesViewModel(ISettingsProvider settingsProvider, IScreenManager screenManager) {
            this.settingsProvider = settingsProvider;

            Screens = new ObservableCollection<DetailedScreen>(screenManager.GetScreens());

            Settings = settingsProvider.GetSettings<PopupSettings>();

            PlaceScreen();

            AvailableColors = new ObservableCollection<AvailableColor>();
            PropertyInfo[] properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (PropertyInfo prop in properties) {
                string name = prop.Name;
                Color value = (Color)prop.GetValue(null, null);

                AvailableColor availableColor = new AvailableColor(name, value);
                if (Settings.FontColor == name) {
                    FontColor = availableColor;
                }

                if (Settings.ItemBackgroundColor == name) {
                    ItemBackgroundColor = availableColor;
                }

                AvailableColors.Add(availableColor);
            }

            SaveCommand = new DelegateCommand(SaveSettings);
            ResetToDefaultsCommand = new DelegateCommand(() => settingsProvider.ResetToDefaults<PopupSettings>());
            VisitCommand = new DelegateCommand(Visit);
        }

        public ICommand VisitCommand { get; private set; }

        public ICommand ResetToDefaultsCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        public ObservableCollection<AvailableColor> AvailableColors { get; private set; }

        public ObservableCollection<DetailedScreen> Screens { get; set; }

        public DetailedScreen SelectedScreen { get; set; }

        public PopupSettings Settings { get; set; }

        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private readonly List<string> authors = new List<string>
                                                    {
                                                         "Brendan Forster",
                                                         "Alex Friedman",
                                                         "Jon Galloway",
                                                         "Jake Ginnivan",
                                                         "Paul Jenkins",
                                                         "Dmitry Pursanov",
                                                         "Chris Sainty",
                                                         "Andrew Tobin",
                                                         "Henrik Andersson"
                                                     };
        private readonly List<string> components = new List<string>
                                                       {
                                                         "MahApps.Metro",
                                                         "Fody",
                                                         "NSubstitute",
                                                         "Reactive Extensions",
                                                         "Squirrel.Windows"
                                                     };
        public string Authors => string.Join(", ", authors);

        public string Components => string.Join(", ", components);

        public AvailableColor FontColor { get; set; }

        public AvailableColor ItemBackgroundColor { get; set; }

        private void Visit() {
            try {
                _ = Process.Start("http://code52.org/carnac/");
            } catch {
                //I forget what exceptions can be raised if the browser is crashed?
            }
        }

        private void SaveSettings() {
            if (Screens.Count < 1) {
                return;
            }

            if (SelectedScreen == null) {
                SelectedScreen = Screens.First();
            }

            Settings.Screen = SelectedScreen.Index;

            Settings.Placement = SelectedScreen.NotificationPlacementTopLeft
                ? NotificationPlacement.TopLeft
                : SelectedScreen.NotificationPlacementBottomLeft
                    ? NotificationPlacement.BottomLeft
                    : SelectedScreen.NotificationPlacementTopRight
                                    ? NotificationPlacement.TopRight
                                    : SelectedScreen.NotificationPlacementBottomRight ? NotificationPlacement.BottomRight : NotificationPlacement.BottomLeft;
            PlaceScreen();

            Settings.SettingsConfigured = true;
            Settings.FontColor = FontColor.Name;
            Settings.ItemBackgroundColor = ItemBackgroundColor.Name;
            settingsProvider.SaveSettings(Settings);
        }

        private void PlaceScreen() {
            if (Screens == null) {
                return;
            }

            SelectedScreen = Screens.FirstOrDefault(s => s.Index == Settings.Screen);

            if (SelectedScreen == null) {
                return;
            }

            switch (Settings.Placement) {
                case NotificationPlacement.TopLeft:
                    SelectedScreen.NotificationPlacementTopLeft = true;
                    break;
                case NotificationPlacement.BottomLeft:
                    SelectedScreen.NotificationPlacementBottomLeft = true;
                    break;
                case NotificationPlacement.TopRight:
                    SelectedScreen.NotificationPlacementTopRight = true;
                    break;
                case NotificationPlacement.BottomRight:
                    SelectedScreen.NotificationPlacementBottomRight = true;
                    break;
                default:
                    SelectedScreen.NotificationPlacementBottomLeft = true;
                    break;
            }

            Settings.Left = SelectedScreen.Left;
            Settings.Top = SelectedScreen.Top;
        }
    }
}