using MvvmHelpers;

namespace Wpf.BidTrainer.ViewModels
{
    public class ReportViewModel : ObservableObject
    {
        private Results results = new();

        public Results Results
        {
            get => results;
            set => SetProperty(ref results, value);
        }
    }
}
