using System.Windows;
using Wpf.BidTrainer.ViewModels;

namespace Wpf.BidTrainer.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            ((SettingsViewModel)DataContext).Load();
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsViewModel = ((SettingsViewModel)DataContext);
            if (Settings1.Default.Username != settingsViewModel.Username)
            {
                var account = await CosmosDbHelper.GetAccount(settingsViewModel.Username);
                if (account != null && account.Value.id != null)
                {
                    MessageBox.Show("Username already exists");
                    return;
                }
            }
            settingsViewModel.Save();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
