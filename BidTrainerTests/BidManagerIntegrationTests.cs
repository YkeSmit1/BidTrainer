using Xunit;
using System;
using EngineWrapper;
using System.IO;
using Common;
using Xunit.Abstractions;

namespace BidTrainerTests
{
    public class BidManagerIntegrationTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public BidManagerIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            var _ = PInvoke.Setup("four_card_majors.db3");
        }

        [Fact]
        public void BidTest()
        {
            var pbn = new Pbn();
            foreach (var pbnFile in Directory.GetFiles("..\\..\\..\\..\\Wpf.BidTrainer\\Pbn", "*.pbn"))
            {
                BidPbnFile(pbn, pbnFile);
            }
        }

        private void BidPbnFile(Pbn pbn, string pbnFile)
        {
            testOutputHelper.WriteLine($"Executing file {pbnFile}");
            LoadPbnFile(pbn, pbnFile);
            foreach (var board in pbn.Boards)
            {
                BidBoard(board);
            }

            var filePath = $"{pbnFile}.{DateTime.Now:d MMM yyyy}";
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
            PInvoke.SetModules(modules);
            pbn.Load(pbnFile);
        }

        private void BidBoard(BoardDto board)
        {
            var auction = BidManager.GetAuction(board.Deal, board.Dealer);
            board.Auction = auction;
            board.Declarer = auction.GetDeclarer();
            testOutputHelper.WriteLine($"Board:{board.BoardNumber}");
            testOutputHelper.WriteLine(auction.GetAuctionAll("|"));
        }

        [Fact]
        public void BidSpecificBoard()
        {
            var pbn = new Pbn();
            var pbnFile = "..\\..\\..\\..\\Wpf.BidTrainer\\Pbn\\lesson7.pbn";
            LoadPbnFile(pbn, pbnFile);
            BidBoard(pbn.Boards[0]);
        }

        [Fact]
        public void BidSpecificPbnFile()
        {
            var pbn = new Pbn();
            var pbnFile = "..\\..\\..\\..\\Wpf.BidTrainer\\Pbn\\lesson7.pbn";
            LoadPbnFile(pbn, pbnFile);
            BidPbnFile(pbn, pbnFile);
        }
    }
}