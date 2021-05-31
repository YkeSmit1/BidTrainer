using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.BidTrainer.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private string username;
        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        private bool useAlternateSuits;
        public bool UseAlternateSuits
        {
            get => useAlternateSuits;
            set => SetProperty(ref useAlternateSuits, value);
        }

        public void Load()
        {
            UseAlternateSuits = Settings1.Default.AlternateSuits;
            Username = Settings1.Default.Username;
        }

        public void Save()
        {
            Settings1.Default.AlternateSuits = UseAlternateSuits;
            Settings1.Default.Username = Username;
        }
    }
}
