using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using EngineWrapper;
using System.IO;
using Common;
using Xunit.Abstractions;

namespace BidTrainer.Tests
{
    public class BidManagerIntegrationTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public BidManagerIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact()]
        public void BidTest()
        {
            var bidManager = new BidManager();
            var pbn = new Pbn();
            foreach (var pbnFile in Directory.GetFiles("..\\..\\..\\..\\Wpf.BidTrainer\\Pbn", "*.pbn"))
            {
                testOutputHelper.WriteLine($"Executing file {pbnFile}");
                var modules = Path.GetFileName(pbnFile) switch
                {
                    "lesson2.pbn" => 1,
                    "lesson3.pbn" => 1,
                    "lesson4.pbn" => 1,
                    "CursusSlotdrive.pbn" => 1,
                    "lesson5.pbn" => 6,
                    "lesson6.pbn" => 30,
                    "lesson7.pbn" => 126,
                    _ => 127
                };
                Pinvoke.SetModules(modules);
                pbn.Load(pbnFile);
                foreach (var board in pbn.Boards)
                {
                    var auction = bidManager.GetAuction(board.Deal, board.Dealer);
                    board.Auction = auction;
                    board.Declarer = auction.GetDeclarer();
                }

                var filePath = $"{pbnFile}.{DateTime.Now.ToShortDateString()}";
                pbn.Save(filePath);
                Assert.Equal(File.ReadAllText($"{pbnFile}.etalon"), File.ReadAllText(filePath));
            }
        }
    }
}