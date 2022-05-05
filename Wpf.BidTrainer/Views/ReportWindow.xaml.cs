using System.Windows;
using Wpf.BidTrainer.ViewModels;

namespace Wpf.BidTrainer
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public ReportWindow(Results results)
        {
            InitializeComponent();
            ((ReportViewModel)DataContext).Results = results;
        }
    }
}
