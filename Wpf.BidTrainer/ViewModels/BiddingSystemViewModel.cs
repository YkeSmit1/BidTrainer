using EngineWrapper;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.BidTrainer.ViewModels
{
    public class EnabledModules
    {
        public Modules Module { get; set; }
        public bool Enabled { get; set; }
    }
    public class BiddingSystemViewModel : ObservableObject
    {
        private ObservableCollection<EnabledModules> modules;

        public ObservableCollection<EnabledModules> Modules
        {
            get => modules;
            set => SetProperty(ref modules, value);
        }

        public void Load()
        {
            var enabledModules = Settings1.Default.EnabledModules;
            Modules = new(Enum.GetValues(typeof(Modules)).Cast<Modules>().Select(x => new EnabledModules() { Module = x, Enabled = (enabledModules & (int)x) == (int)x }));
        }

        public void Save()
        {
            Settings1.Default.EnabledModules = Modules.Select(x => x.Enabled ? (int)x.Module: 0).Sum();
            Settings1.Default.Save();
        }
    }
}
