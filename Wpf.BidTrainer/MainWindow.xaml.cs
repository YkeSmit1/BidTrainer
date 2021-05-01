using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using EngineWrapper;
using Common;
using Wpf.BidControls.Commands;
using Newtonsoft.Json;

namespace Wpf.BidTrainer
{
    public partial class MainWindow : Window
    {
        // Bidding
        private readonly BidManager bidManager = new();
        private readonly Auction auction = new();
        private readonly Pbn pbn = new();

        // Lesson
        private static int CurrentBoardIndex => Settings1.Default.CurrentBoardIndex;
        private Dictionary<Player, string> Deal => pbn.Boards[CurrentBoardIndex].Deal;
        private Player Dealer => pbn.Boards[CurrentBoardIndex].Dealer;
        private Lesson lesson;
        private List<Lesson> lessons;

        // Results
        private Result currentResult;
        private DateTime startTimeBoard;
        private readonly Results results = new();

        public MainWindow()
        {
            InitializeComponent();

            MenuUseAlternateSuits.IsChecked = Settings1.Default.AlternateSuits;
            BiddingBoxView.BiddingBoxViewModel.DoBid = new DoBidCommand(ClickBiddingBoxButton);
            AuctionView.AuctionViewModel.Auction = auction;
            if (File.Exists("results.json"))
                results = JsonConvert.DeserializeObject<Results>(File.ReadAllText("results.json"));

            StartLesson();
        }

        public void StartLesson()
        {
            var startPage = new StartPage();
            startPage.ShowDialog();
            lessons = startPage.Lessons;
            Settings1.Default.CurrentBoardIndex = startPage.IsContinueWhereLeftOff ? Settings1.Default.CurrentBoardIndex : 0;
            lesson = startPage.Lesson;
            pbn.Load(System.IO.Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Pbn", lesson.PbnFile));

            StartNextBoard();
        }

        private void ClickBiddingBoxButton(object parameter)
        {
            var bid = (Bid)parameter;
            if (Cursor == Cursors.Help)
            {
                var (minRecords, maxRecords) = BidGenerator.GetRecords(bid,
                    bidManager.GetCurrentPhase(auction.currentPosition), auction.currentPosition);
                var information = Util.GetInformation(minRecords, maxRecords);
                Cursor = Cursors.Arrow;
                MessageBox.Show(information, "Information");
            }
            else
            {
                var engineBid = bidManager.GetBid(auction, Deal[Player.South]);
                auction.AddBid(engineBid);
                AuctionView.AuctionViewModel.UpdateAuction(auction);

                if (bid != engineBid)
                {
                    MessageBox.Show($"The correct bid is {engineBid}. Description: {engineBid.description}.", "Incorrect bid");
                    currentResult.AnsweredCorrectly = false;
                }

                BidTillSouth();
            }
        }

        private void StartNextBoard()
        {
            panelNorth.Visibility = Visibility.Hidden;
            if (CurrentBoardIndex > pbn.Boards.Count - 1)
            {
                var newLessons = lessons.Where(x => x.LessonNr == lesson.LessonNr + 1);
                if (newLessons.Any())
                {
                    lesson = newLessons.Single();
                    pbn.Load(System.IO.Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Pbn", lesson.PbnFile));
                    Settings1.Default.CurrentBoardIndex = 0;
                }
                else
                {
                    BiddingBoxView.IsEnabled = false;
                    ShowReport();
                    return;
                }
            }
            StartBidding();
        }

        private void StartBidding()
        {
            ShowBothHands();
            BiddingBoxView.BiddingBoxViewModel.ClearBiddingBox();
            auction.Clear(Dealer);
            AuctionView.AuctionViewModel.UpdateAuction(auction);
            bidManager.Init();
            StatusBar.Content = $"Lesson: {lesson.LessonNr} Board: {CurrentBoardIndex + 1}";
            auction.currentPlayer = Dealer;
            startTimeBoard = DateTime.Now;
            currentResult = new Result();
            BidTillSouth();
        }

        private void ShowBothHands()
        {
            panelNorth.HandViewModel.ShowHand(Deal[Player.North], MenuUseAlternateSuits.IsChecked);
            panelSouth.HandViewModel.ShowHand(Deal[Player.South], MenuUseAlternateSuits.IsChecked);
        }

        private void BidTillSouth()
        {
            while (auction.currentPlayer != Player.South && !auction.IsEndOfBidding())
            {
                var bid = bidManager.GetBid(auction, Deal[auction.currentPlayer]);
                auction.AddBid(bid);
                BiddingBoxView.BiddingBoxViewModel.UpdateButtons(bid, auction.currentPlayer);
            }

            AuctionView.AuctionViewModel.UpdateAuction(auction);
            var endOfBidding = auction.IsEndOfBidding();
            BiddingBoxView.IsEnabled = !endOfBidding;
            if (endOfBidding)
            {
                panelNorth.Visibility = Visibility.Visible;
                currentResult.TimeElapsed = DateTime.Now - startTimeBoard;
                MessageBox.Show($"Hand is done. Contract:{auction.currentContract}");
                results.AddResult(lesson.LessonNr, CurrentBoardIndex, currentResult);
                Settings1.Default.CurrentBoardIndex++;
                StartNextBoard();
            }
        }

        private void MenuOpenPbn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog().Value)
            {
                try
                {
                    pbn.Load(openFileDialog.FileName);
                    Settings1.Default.CurrentBoardIndex = 0;
                    StartBidding();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening PBN({ex.Message})", "Error");
                }
            }
        }

        private void MenuSavePbn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog().Value)
            {
                try
                {
                    pbn.Save(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving PBN({ex.Message})", "Error");
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            File.WriteAllText("results.json", JsonConvert.SerializeObject(results, Formatting.Indented));
            Settings1.Default.AlternateSuits = MenuUseAlternateSuits.IsChecked;
            Settings1.Default.Save();
        }

        private void MenuUseAlternateSuits_Click(object sender, RoutedEventArgs e)
        {
            MenuUseAlternateSuits.IsChecked = !MenuUseAlternateSuits.IsChecked;
            ShowBothHands();
        }

        private void ButtonHintClick(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Help;
            currentResult.UsedHint = true;
        }

        private void GoToLesson_Click(object sender, RoutedEventArgs e)
        {
            StartLesson();
        }

        private void MenuShowReport_Click(object sender, RoutedEventArgs e)
        {
            ShowReport();
        }

        private void ShowReport()
        {
            MessageBox.Show(results.GetReport());
        }

        private void MenuBidAgain_Click(object sender, RoutedEventArgs e)
        {
            StartBidding();
        }
    }
}
