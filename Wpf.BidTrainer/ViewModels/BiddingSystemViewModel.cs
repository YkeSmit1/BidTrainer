using EngineWrapper;
using MvvmHelpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Wpf.BidTrainer.ViewModels
{
    public class EnabledModules
    {
        public Modules Module { get; init; }
        public bool Enabled { get; set; }
    }
    public class BiddingSystemViewModel : ObservableObject
    {
        private ObservableCollection<EnabledModules> modules;

        public ObservableCollection<EnabledModules> Modules
        {
            get => modules;
            private set => SetProperty(ref modules, value);
        }

        public void Load()
        {
            var enabledModules = Settings1.Default.EnabledModules;
            Modules = new ObservableCollection<EnabledModules>(Enum.GetValues<Modules>().Select(x =>
                new EnabledModules() { Module = x, Enabled = (enabledModules & (int)x) == (int)x }));
        }

        public void Save()
        {
            Settings1.Default.EnabledModules = Modules.Select(x => x.Enabled ? (int)x.Module: 0).Sum();
            Settings1.Default.Save();
        }
    }
}
