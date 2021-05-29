using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.BidTrainer
{
    public struct Account
    {
        public string id { get; set; }
        public string username { get; set; }
        public int numberOfBoardsPlayed { get; set; }
        public int numberOfCorrectBoards { get; set; }
        public TimeSpan timeElapsed { get; set; }
    }
}
