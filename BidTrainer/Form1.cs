using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;

namespace BidTrainer
{
    public partial class Form1 : Form
    {
        private BiddingBox biddingBox;
        private AuctionControl auctionControl;
        private readonly BidManager bidManager = new BidManager();
        private readonly Pbn pbn = new Pbn();
        private int boardIndex = 0;
        private string[] deal => pbn.Boards[boardIndex].Deal;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowBiddingBox();
            ShowAuction();
            pbn.Boards.Add(new BoardDto { Deal = new string[] { "864,Q743,Q3,AQ95", "AJ32,J9,AJ65,K84", "KT5,652,KT4,JT63", "Q97,AKT8,9872,72" } });
            pbn.Boards.Add(new BoardDto { Deal = new string[] { "864,Q743,Q3,AQ95", "AKJ3,J9,AJ65,K84", "T52,652,KT4,JT63", "Q97,AKT8,9872,72" } });

            ShowBothHands();
            StartBidding();
        }

        private void ShowBothHands()
        {
            ShowHand(deal[(int)Player.North], panelNorth);
            ShowHand(deal[(int)Player.South], panelSouth);
        }

        private void ShowHand(string hand, Panel parent)
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
                var bid = bidManager.GetBid(auctionControl.auction, deal[(int)Player.South]);
                auctionControl.auction.AddBid(bid);

                if (biddingBoxButton.bid != bid)
                {
                    MessageBox.Show($"The correct bid is {bid}. Description: {bid.description}.", "Incorrect bid");
                }

                BidTillSouth(auctionControl.auction);
            }
            biddingBox = new BiddingBox(handler)
            {
                Parent = this,
                Left = 50,
                Top = 200
            };
            biddingBox.Show();
        }

        private void ShowAuction()
        {
            auctionControl = new AuctionControl
            {
                Parent = this,
                Left = 300,
                Top = 200,
                Width = 220,
                Height = 200
            };
            auctionControl.Show();
        }

        private void StartBidding()
        {
            auctionControl.auction.Clear();
            biddingBox.Clear();
            bidManager.Init();
            BidTillSouth(auctionControl.auction);
        }

        private void BidTillSouth(Auction auction)
        {
            while (auction.currentPlayer != Player.South && !auction.IsEndOfBidding())
            {
                var bid = bidManager.GetBid(auction, deal[(int)auction.currentPlayer]);
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
                if (boardIndex < pbn.Boards.Count() - 1)
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
    }
}
