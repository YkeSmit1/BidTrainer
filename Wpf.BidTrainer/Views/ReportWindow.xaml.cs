using Wpf.BidTrainer.ViewModels;

namespace Wpf.BidTrainer.Views
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow
    {
        public ReportWindow(Results results)
        {
            InitializeComponent();
            ((ReportViewModel)DataContext).Results = results;
        }
    }
}
