using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Wpf.BidTrainer
{
    public class StartLessonCommand : ICommand
    {
        Action<int> action;

        public StartLessonCommand(Action<int> action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action.Invoke((int)parameter);
        }
    }
}
