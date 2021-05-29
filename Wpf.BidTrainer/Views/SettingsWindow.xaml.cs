using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.BidTrainer.ViewModels;

namespace Wpf.BidTrainer.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
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
                var account = await CosmosDBHelper.GetAccount(settingsViewModel.Username);
                if (account.Value.id != null)
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
