using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;

namespace BidTrainer
{
    public partial class Form1 : Form
    {
        private BiddingBox biddingBox;
        private AuctionControl auctionControl;
        private readonly BidManager bidManager = new();
        private readonly Pbn pbn = new();
        private int boardIndex = 0;
        private Dictionary<Player, string> Deal => pbn.Boards[boardIndex].Deal;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowBiddingBox();
            ShowAuction();
            pbn.Boards.Add(new BoardDto { Deal = new Dictionary<Player, string> {
                { Player.West, "864,Q743,Q3,AQ95" },
                { Player.North, "AJ32,J9,AJ65,K84" },
                { Player.East, "KT5,652,KT4,JT63" },
                { Player.South, "Q97,AKT8,9872,72" } 
            }});
            pbn.Boards.Add(new BoardDto {Deal = new Dictionary<Player, string> {
                { Player.West, "864,Q743,Q3,AQ95" },
                { Player.North,"AKJ3,J9,AJ65,K84" },
                { Player.East, "T52,652,KT4,JT63" },
                { Player.South,"Q97,AKT8,9872,72" } 
            }});

            ShowBothHands();
            StartBidding();
            auctionControl.Select();
        }

        private void ShowBothHands()
        {
            ShowHand(Deal[Player.North], panelNorth);
            ShowHand(Deal[Player.South], panelSouth);
        }

        private static void ShowHand(string hand, Panel parent)
        {
            parent.Controls.OfType<PictureBox>().ToList().ForEach((card) =>
            {
                parent.Controls.Remove(card);
                card.Dispose();
            });
            var suits = hand.Split(',');
            var suit = Suit.Clubs;
            var left = 20 * 12;
            foreach (var suitStr in suits.Reverse())
            {
                foreach (var card in suitStr.Reverse())
                {
                    var pictureBox = new PictureBox
                    {
                        Image = CardControl.GetFaceImageForCard(suit, Util.GetFaceFromDescription(card)),
                        Left = left,
                        Parent = parent,
                        Height = 97,
                        Width = 73,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                    };
                    pictureBox.Show();
                    left -= 20;
                }
                suit++;
            }
        }

        private void ShowBiddingBox()
        {
            void handler(object x, EventArgs y)
            {
                var biddingBoxButton = (BiddingBoxButton)x;
                if (Cursor == Cursors.Help)
                {
                    var (minRecords, maxRecords) = BidGenerator.GetRecords(biddingBoxButton.bid, bidManager.phase, auctionControl.auction.currentPosition);
                    var information = Util.GetInformation(minRecords, maxRecords);
                    Cursor = Cursors.Default;
                    MessageBox.Show(information, "Information");
                }
                else
                {
                    var bid = bidManager.GetBid(auctionControl.auction, Deal[Player.South]);
                    auctionControl.auction.AddBid(bid);

                    if (biddingBoxButton.bid != bid)
                    {
                        MessageBox.Show($"The correct bid is {bid}. Description: {bid.description}.", "Incorrect bid");
                    }

                    BidTillSouth(auctionControl.auction);
                }
            }
            biddingBox = new BiddingBox(handler) {Parent = this};
            biddingBox.Show();
        }

        private void ShowAuction()
        {
            auctionControl = new AuctionControl {Parent = this};
                auctionControl.Show();
        }

        private void StartBidding()
        {
            auctionControl.auction.Clear();
            biddingBox.Clear();
            bidManager.Init();
            toolStripStatusLabel1.Text = $"Board:{boardIndex + 1}";
            auctionControl.auction.currentPlayer = pbn.Boards[boardIndex].Dealer;
            BidTillSouth(auctionControl.auction);
        }

        private void BidTillSouth(Auction auction)
        {
            while (auction.currentPlayer != Player.South && !auction.IsEndOfBidding())
            {
                var bid = bidManager.GetBid(auction, Deal[auction.currentPlayer]);
                auction.AddBid(bid);
                biddingBox.UpdateButtons(bid, auction.currentPlayer);
            }

            auctionControl.ReDraw();
            var endOfBidding = auction.IsEndOfBidding();
            biddingBox.Enabled = !endOfBidding;
            if (endOfBidding)
            {
                panelNorth.Show();
                MessageBox.Show($"Hand is done. Contract:{auction.currentContract}");
                panelNorth.Hide();
                if (boardIndex < pbn.Boards.Count - 1)
                {
                    boardIndex++;
                    ShowBothHands();
                    StartBidding();
                }
            }
        }

        private void ToolStripMenuItemOpenPBNClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pbn.Load(openFileDialog1.FileName);
                    boardIndex = 0;
                    ShowBothHands();
                    StartBidding();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening PBN({ex.Message})", "Error");
                }
            }
        }

        private void ToolStripMenuItemSavePBNClick(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pbn.Save(saveFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving PBN({ex.Message})", "Error");
                }
            }
        }

        private void ButtonHintClick(object sender, EventArgs e)
        {
            Cursor = Cursors.Help;
        }
    }
}
