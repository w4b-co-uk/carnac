using Carnac.Logic.Native;
using Carnac.UI;
using System.Windows;
using System.Windows.Controls;

namespace Carnac {
    public partial class PositionOnMonitorSelector {
        public PositionOnMonitorSelector() {
            InitializeComponent();
        }

        private void RadioChecked(object sender, RoutedEventArgs e) {
            if (!(DataContext is PreferencesViewModel dc)) {
                return;
            }

            if (!(sender is RadioButton rb)) {
                return;
            }

            if (!(rb.Tag is DetailedScreen tag)) {
                return;
            }

            dc.SelectedScreen = tag;
        }
    }
}