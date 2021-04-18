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
using Microsoft.Win32;
using EngineWrapper;
using Common;
using Wpf.BidControls.Commands;

namespace Wpf.BidTrainer
{
    public partial class MainWindow : Window
    {
        private readonly BidManager bidManager = new();
        private readonly Pbn pbn = new();
        private int boardIndex = 0;
        private Dictionary<Player, string> Deal => pbn.Boards[boardIndex].Deal;
        private Player Dealer => pbn.Boards[boardIndex].Dealer;
        private readonly Auction auction = new();

        public MainWindow()
        {
            InitializeComponent();

            LoadSampleBoards();
            BiddingBoxView.BiddingBoxViewModel.DoBid = new DoBidCommand(ClickBiddingBoxButton);
            AuctionView.AuctionViewModel.Auction = auction;
            StartNextBoard();
        }

        private void ClickBiddingBoxButton(object parameter)
        {
            var bid = (Bid)parameter;
            if (Cursor == Cursors.Help)
            {
                var (minRecords, maxRecords) = BidGenerator.GetRecords(bid, bidManager.phase, auction.currentPosition);
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
                    MessageBox.Show($"The correct bid is {engineBid}. Description: {engineBid.description}.", "Incorrect bid");

                BidTillSouth();
            }
        }

        private void StartNextBoard()
        {
            panelNorth.Visibility = Visibility.Hidden;
            if (boardIndex <= pbn.Boards.Count - 1)
                StartBidding();
        }

        private void StartBidding()
        {
            ShowBothHands();
            BiddingBoxView.BiddingBoxViewModel.ClearBiddingBox();
            auction.Clear(Dealer);
            AuctionView.AuctionViewModel.UpdateAuction(auction);
            bidManager.Init();
            StatusBar.Content = $"Board:{boardIndex + 1}";
            auction.currentPlayer = Dealer;
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
                MessageBox.Show($"Hand is done. Contract:{auction.currentContract}");
                boardIndex++;
                StartNextBoard();
            }
        }

        private void LoadSampleBoards()
        {
            pbn.Boards.AddRange(new[] { new BoardDto
            {
                Deal = new Dictionary<Player, string> {{ Player.West, "864,Q743,Q3,AQ95" },{ Player.North, "AJ32,J9,AJ65,K84" },
                    { Player.East, "KT5,652,KT4,JT63" },{ Player.South, "Q97,AKT8,9872,72" } }
            },
            new BoardDto
            {
                Deal = new Dictionary<Player, string> {{ Player.West, "864,Q743,Q3,AQ95" }, { Player.North,"AKJ3,J9,AJ65,K84" },
                    { Player.East, "T52,652,KT4,JT72" }, { Player.South,"Q97,AKT8,9872,63" } }
            }});
        }

        private void MenuOpenPbn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog().Value)
            {
                try
                {
                    pbn.Load(openFileDialog.FileName);
                    boardIndex = 0;
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
            Settings1.Default.AlternateSuits = MenuUseAlternateSuits.IsChecked;
            Settings1.Default.Save();
        }

        private void MenuUseAlternateSuits_Click(object sender, RoutedEventArgs e)
        {
            MenuUseAlternateSuits.IsChecked = !MenuUseAlternateSuits.IsChecked;
            ShowBothHands();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Help;
        }
    }
}
