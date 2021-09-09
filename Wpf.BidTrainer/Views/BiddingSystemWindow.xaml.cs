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

namespace Wpf.BidTrainer
{
    /// <summary>
    /// Interaction logic for BiddingSystemWindow.xaml
    /// </summary>
    public partial class BiddingSystemWindow : Window
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
