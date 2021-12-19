using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using EngineWrapper;
using System.IO;
using Common;

namespace BidTrainer.Tests
{
    public class BidManagerIntegrationTests
    {
        [Fact()]
        public void BidTest()
        {
            var bidManager = new BidManager();
            var _ = Pinvoke.Setup("four_card_majors.db3");
            var pbn = new Pbn();
            foreach (var pbnFile in Directory.GetFiles("..\\..\\..\\..\\Wpf.BidTrainer\\Pbn", "*.pbn"))
            {
                pbn.Load(pbnFile);
                foreach (var board in pbn.Boards)
                    board.Auction = bidManager.GetAuction(board.Deal, board.Dealer);

                var filePath = $"{pbnFile}.{DateTime.Now:d MMM yyyy}";
                pbn.Save(filePath);
                Assert.Equal(File.ReadAllText($"{pbnFile}.etalon"), File.ReadAllText(filePath));
            }
        }
    }
}