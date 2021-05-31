using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
