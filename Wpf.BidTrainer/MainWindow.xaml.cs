using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using EngineWrapper;
using Common;
using Newtonsoft.Json;
using Wpf.BidControls.ViewModels;
using MvvmHelpers.Commands;
using Wpf.BidTrainer.Views;

namespace Wpf.BidTrainer
{
    public partial class MainWindow
    {
        // Bidding
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

        // ViewModels
        private BiddingBoxViewModel BiddingBoxViewModel => (BiddingBoxViewModel)BiddingBoxView.DataContext;
        private AuctionViewModel AuctionViewModel => (AuctionViewModel)AuctionView.DataContext;
        private HandViewModel HandViewModelNorth => (HandViewModel)PanelNorth.DataContext;
        private HandViewModel HandViewModelSouth => (HandViewModel)PanelSouth.DataContext;

        public MainWindow()
        {
            InitializeComponent();

            StatusBarUsername.Content = $"Username: {Settings1.Default.Username}";
            BiddingBoxViewModel.DoBid = new Command(ClickBiddingBoxButton, ButtonCanExecute);
            AuctionViewModel.Auction = auction;
            if (File.Exists("results.json"))
                results = JsonConvert.DeserializeObject<Results>(File.ReadAllText("results.json"));
            var _ = PInvoke.Setup("four_card_majors.db3");
            StartLesson();
        }

        private void StartLesson()
        {
            var startPage = new StartPage();
            startPage.ShowDialog();
            lessons = startPage.Lessons;
            if (!startPage.IsContinueWhereLeftOff)
                Settings1.Default.CurrentBoardIndex = 0;
            lesson = startPage.Lesson;
            pbn.Load(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName ?? "", "Pbn", lesson.PbnFile));
            PInvoke.SetModules(lesson.Modules);
            if (!startPage.IsContinueWhereLeftOff)
                results.AllResults.Remove(lesson.LessonNr);

            StartNextBoard();
        }

        private void ClickBiddingBoxButton(object parameter)
        {
            var bid = (Bid)parameter;
            if (!ToggleSwitchMode.IsChecked)
            {
                currentResult.UsedHint = true;
                Cursor = Cursors.Arrow;
                MessageBox.Show(BidManager.GetInformation(bid, auction), "Information");
            }
            else
            {
                var engineBid = BidManager.GetBid(auction, Deal[Player.South]);
                UpdateBidControls(engineBid);

                if (bid != engineBid)
                {
                    MessageBox.Show($"The correct bid is {engineBid}. Description: {engineBid.description}.", "Incorrect bid");
                    currentResult.AnsweredCorrectly = false;
                }

                BidTillSouth();
            }
        }

        private bool ButtonCanExecute(object param)
        {
            var bid = (Bid)param;
            return auction.BidIsPossible(bid);
        }

        private void UpdateBidControls(Bid bid)
        {
            auction.AddBid(bid);
            AuctionViewModel.UpdateAuction(auction);
            BiddingBoxViewModel.DoBid.RaiseCanExecuteChanged();
        }

        private void StartNextBoard()
        {
            PanelNorth.Visibility = Visibility.Hidden;
            LabelNorth.Visibility = Visibility.Hidden;
            BiddingBoxView.IsEnabled = true;
            if (CurrentBoardIndex > pbn.Boards.Count - 1)
            {
                var newLessons = lessons.Where(x => x.LessonNr == lesson.LessonNr + 1).ToList();
                if (newLessons.Any())
                {
                    lesson = newLessons.Single();
                    pbn.Load(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName ?? "", "Pbn", lesson.PbnFile));
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
            auction.Clear(Dealer);
            BiddingBoxViewModel.DoBid.RaiseCanExecuteChanged();
            AuctionViewModel.UpdateAuction(auction);
            StatusBarLesson.Content = $"Lesson: {lesson.LessonNr} Board: {CurrentBoardIndex + 1}";
            startTimeBoard = DateTime.Now;
            currentResult = new Result();
            BidTillSouth();
        }

        private void ShowBothHands()
        {
            HandViewModelNorth.ShowHand(Deal[Player.North], Settings1.Default.AlternateSuits, Settings1.Default.CardImageSettings);
            HandViewModelSouth.ShowHand(Deal[Player.South], Settings1.Default.AlternateSuits, Settings1.Default.CardImageSettings);
        }

        private void BidTillSouth()
        {
            while (auction.CurrentPlayer != Player.South && !auction.IsEndOfBidding())
            {
                var bid = BidManager.GetBid(auction, Deal[auction.CurrentPlayer]);
                UpdateBidControls(bid);
            }

            if (auction.IsEndOfBidding())
            {
                auction.CurrentPlayer = Player.UnKnown;
                AuctionViewModel.UpdateAuction(auction);
                BiddingBoxView.IsEnabled = false;
                PanelNorth.Visibility = Visibility.Visible;
                LabelNorth.Visibility = Visibility.Visible;
                currentResult.TimeElapsed = DateTime.Now - startTimeBoard;
                MessageBox.Show($"Hand is done. Contract:{auction.currentContract}");
                results.AddResult(lesson.LessonNr, CurrentBoardIndex, currentResult);
                UploadResults();
                Settings1.Default.CurrentBoardIndex++;
                StartNextBoard();
            }
        }

        private void UploadResults()
        {
            var username = Settings1.Default.Username;
            if (username != "")
            {
                var res = results.AllResults.Values.SelectMany(x => x.Results.Values).ToList();
                Task.Run(() => UpdateOrCreateAccount(username, res.Count, res.Count(x => x.AnsweredCorrectly), res.Sum(x => x.TimeElapsed.Ticks)));
            }

            static async Task UpdateOrCreateAccount(string username, int boardPlayed, int correctBoards, long timeElapsed)
            {
                var account = new Account
                {
                    username = username,
                    numberOfBoardsPlayed = boardPlayed,
                    numberOfCorrectBoards = correctBoards,
                    timeElapsed = new TimeSpan(timeElapsed)
                };

                var user = await CosmosDbHelper.GetAccount(username);
                if (user == null)
                {
                    account.id = Guid.NewGuid().ToString();
                    await CosmosDbHelper.InsertAccount(account);
                }
                else
                {
                    account.id = user.Value.id;
                    await CosmosDbHelper.UpdateAccount(account);
                }
            }
        }

        private void MenuOpenPbn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (!openFileDialog.ShowDialog().GetValueOrDefault()) return;
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

        private void MenuSavePbn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (!saveFileDialog.ShowDialog().GetValueOrDefault()) return;
            try
            {
                pbn.Save(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving PBN({ex.Message})", "Error");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            File.WriteAllText("results.json", JsonConvert.SerializeObject(results, Formatting.Indented));
            Settings1.Default.Save();
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
            new ReportWindow(results).ShowDialog();
        }

        private void MenuBidAgain_Click(object sender, RoutedEventArgs e)
        {
            StartBidding();
        }

        private void MenuGoBack_Click(object sender, RoutedEventArgs e)
        {
            if (Settings1.Default.CurrentBoardIndex > 0)
            {
                Settings1.Default.CurrentBoardIndex--;
                StartNextBoard();
            }
        }

        private async void MenuShowLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var accounts = await CosmosDbHelper.GetAllAccounts();
                new LeaderboardWindow(accounts.OrderByDescending(x => (double)x.numberOfCorrectBoards / x.numberOfBoardsPlayed)).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuOptions_Click(object sender, RoutedEventArgs e)
        {
            if (!new SettingsWindow().ShowDialog().GetValueOrDefault()) return;
            if ((string)StatusBarUsername.Content != Settings1.Default.Username)
                results.AllResults.Clear();
            ShowBothHands();
            StatusBarUsername.Content = $"Username: {Settings1.Default.Username}";
        }

        private void MenuBiddingSystem_Click(object sender, RoutedEventArgs e)
        {
            if (new BiddingSystemWindow().ShowDialog().GetValueOrDefault())
                PInvoke.SetModules(Settings1.Default.EnabledModules);
        }
    }
}
