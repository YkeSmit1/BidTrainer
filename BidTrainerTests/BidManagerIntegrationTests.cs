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

        [Fact]
        public void BidTest()
        {
            var bidManager = new BidManager();
            var pbn = new Pbn();
            foreach (var pbnFile in Directory.GetFiles("..\\..\\..\\..\\Wpf.BidTrainer\\Pbn", "*.pbn"))
            {
                BidPbnFile(bidManager, pbn, pbnFile);
            }
        }

        private void BidPbnFile(BidManager bidManager, Pbn pbn, string pbnFile)
        {
            testOutputHelper.WriteLine($"Executing file {pbnFile}");
            LoadPbnFile(pbn, pbnFile);
            foreach (var board in pbn.Boards)
            {
                BidBoard(bidManager, board);
            }

            var filePath = $"{pbnFile}.{DateTime.Now.ToShortDateString()}";
            pbn.Save(filePath);
            Assert.Equal(File.ReadAllText($"{pbnFile}.etalon"), File.ReadAllText(filePath));
        }

        private static void LoadPbnFile(Pbn pbn, string pbnFile)
        {
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
        }

        private void BidBoard(BidManager bidManager, BoardDto board)
        {
            var auction = bidManager.GetAuction(board.Deal, board.Dealer);
            board.Auction = auction;
            board.Declarer = auction.GetDeclarer();
            testOutputHelper.WriteLine(auction.GetPrettyAuction("|"));
        }

        [Fact]
        public void BidSpecificBoard()
        {
            var bidManager = new BidManager();
            var pbn = new Pbn();
            var pbnFile = "..\\..\\..\\..\\Wpf.BidTrainer\\Pbn\\lesson7.pbn";
            LoadPbnFile(pbn, pbnFile);
            BidBoard(bidManager, pbn.Boards[0]);
        }

        [Fact]
        public void BidSpecificPbnFile()
        {
            var bidManager = new BidManager();
            var pbn = new Pbn();
            var pbnFile = "..\\..\\..\\..\\Wpf.BidTrainer\\Pbn\\lesson7.pbn";
            LoadPbnFile(pbn, pbnFile);
            BidPbnFile(bidManager, pbn, pbnFile);
        }
    }
}