using Carnac.Logic;
using Carnac.Logic.Models;
using System.Collections.ObjectModel;

namespace Carnac.UI {
    public class KeyShowViewModel: NotifyPropertyChanged {
        public KeyShowViewModel(PopupSettings popupSettings) {
            Messages = new ObservableCollection<Message>();
            Settings = popupSettings;
        }

        public ObservableCollection<Message> Messages { get; private set; }

        public PopupSettings Settings { get; set; }
    }
}
