using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Common
{
    public partial class AuctionControl : UserControl
    {
        public Auction auction = new Auction();
        private readonly ToolTip toolTip = new ToolTip();

        public AuctionControl()
        {
            InitializeComponent();
            Width = TextRenderer.MeasureText("West        North        East        South", richTextBox1.Font).Width + 20;
            Left = 300;
            Top = 200;
            Height = 200;
            richTextBox1.ReadOnly = true;

            void handler(object x, MouseEventArgs y)
            {
                var index = richTextBox1.GetCharIndexFromPosition(y.Location);
                var biddingRoundIndex = richTextBox1.GetLineFromCharIndex(index);
                if (auction.bids.TryGetValue(biddingRoundIndex, out var biddingRound) && biddingRound.TryGetValue(Player.South, out var bid))
                    toolTip.Show(bid.description, richTextBox1);
            }
            richTextBox1.MouseMove += handler;
            auction.Clear();
            ReDraw();
        }

        public void AddBid(Bid bid)
        {
            auction.AddBid(bid);
            ReDraw();
        }

        public void ReDraw()
        {
            richTextBox1.Clear();
            richTextBox1.AppendText("West\tNorth\tEast\tSouth");

            foreach (var biddingRound in auction.bids)
            {
                richTextBox1.AppendText(Environment.NewLine);
                string strBiddingRound = string.Empty;

                AddLeadingTabs(biddingRound);

                foreach (var bid in biddingRound.Value)
                {
                    if (bid.Value.suit == Suit.Hearts || bid.Value.suit == Suit.Diamonds)
                    {
                        richTextBox1.AppendText(bid.Value.rank.ToString());
                        richTextBox1.AppendText(Util.GetSuitDescription(bid.Value.suit), Color.Red);
                    }
                    else
                    {
                        richTextBox1.AppendText(bid.Value.ToString());
                    }
                    richTextBox1.AppendText("\t");
                }
            }

            void AddLeadingTabs(KeyValuePair<int, Dictionary<Player, Bid>> biddingRound)
            {
                if (biddingRound.Key == 1)
                {
                    var player = Player.West;
                    while (player != biddingRound.Value.First().Key)
                    {
                        richTextBox1.AppendText("\t");
                        player++;
                    }
                }
            }
        }
    }
}
