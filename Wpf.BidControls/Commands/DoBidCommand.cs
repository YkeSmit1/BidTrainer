using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Wpf.BidControls.Commands
{
    public class DoBidCommand : ICommand
    {
        public event EventHandler CanExecuteChanged { add { } remove { } }
        private readonly Action<object> action;
        public DoBidCommand()
        {
            action = (bla) => MessageBox.Show("Hi");
        }
        public DoBidCommand(Action<object> action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action.Invoke(parameter);
        }
    }
}
