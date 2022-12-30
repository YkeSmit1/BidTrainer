using System;
using System.Diagnostics.CodeAnalysis;

namespace Wpf.BidTrainer
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct Account
    {
        public string id { get; set; }
        public string username { get; set; }
        public int numberOfBoardsPlayed { get; set; }
        public int numberOfCorrectBoards { get; set; }
        public TimeSpan timeElapsed { get; set; }
    }
}
