using System.Windows;
using Wpf.BidTrainer.ViewModels;

namespace Wpf.BidTrainer.Views
{
    /// <summary>
    /// Interaction logic for BiddingSystemWindow.xaml
    /// </summary>
    public partial class BiddingSystemWindow
    {
        public BiddingSystemWindow()
        {
            InitializeComponent();
            var biddingSystemVIewModel = (BiddingSystemViewModel)DataContext;
            biddingSystemVIewModel.Load();

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var biddingSystemVIewModel = (BiddingSystemViewModel)DataContext;
            biddingSystemVIewModel.Save();
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
