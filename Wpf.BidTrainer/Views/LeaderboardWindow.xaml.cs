using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wpf.BidTrainer.ViewModels;

namespace Wpf.BidTrainer.Views
{
    /// <summary>
    /// Interaction logic for LeaderboardWindow.xaml
    /// </summary>
    public partial class LeaderboardWindow
    {
        public LeaderboardWindow(IEnumerable<Account> accounts)
        {
            InitializeComponent();
            ((LeaderboardViewModel)DataContext).Accounts = new ObservableCollection<Account>(accounts);
        }
    }
}
