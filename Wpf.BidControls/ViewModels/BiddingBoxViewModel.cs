using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Common;
using Wpf.BidControls.Commands;

namespace Wpf.BidControls.ViewModels
{
    public class BiddingBoxViewModel : ObservableObject
    {
        public class BidEnable
        {
            public Bid Bid { set; get; }
            public bool IsEnabled { get; set; } = true;
        }

        public List<List<BidEnable>> SuitBids { get; set; } = new();
        public List<BidEnable> NonSuitBids { get; set; } = new();
        private BidType currentBidType = BidType.pass;
        private Player currentDeclarer = Player.UnKnown;

        public DoBidCommand DoBid { get; set; }
        public bool IsEnabled { get; set; }

        public BiddingBoxViewModel()
        {
            ClearBiddingBox();
        }

        public void ClearBiddingBox()
        {
            SuitBids.Clear();
            SuitBids.AddRange(Enumerable.Range(1, 7)
                .Select(level => new List<BidEnable>(Enum.GetValues(typeof(Suit)).Cast<Suit>()
                .Select(suit => new BidEnable { Bid = new Bid(level, suit), IsEnabled = true }))));
            NonSuitBids.Clear();
            NonSuitBids.AddRange(new List<BidEnable> {
                new BidEnable { Bid = Bid.PassBid, IsEnabled = true },
                new BidEnable { Bid = Bid.Dbl, IsEnabled = true },
                new BidEnable { Bid = Bid.Rdbl, IsEnabled = true } });
        }

        public void UpdateButtons(Bid Newbid, Player auctionCurrentPlayer)
        {
            var lSuitBids = ObjectCloner.ObjectCloner.DeepClone(SuitBids);
            var lNonSuitBids = ObjectCloner.ObjectCloner.DeepClone(NonSuitBids);
            currentBidType = Newbid.bidType;

            switch (Newbid.bidType)
            {
                case BidType.bid:
                    EnableButtons(new[] { Bid.Dbl });
                    DisableButtons(new[] { Bid.Rdbl });
                    foreach (var bidsPerRank in lSuitBids)
                        foreach (var bids in bidsPerRank)
                        {
                            Bid localBid = bids.Bid;
                            if (localBid.bidType == BidType.bid && localBid <= Newbid)
                            {
                                bids.IsEnabled = false;
                            }
                        }
                    if (currentDeclarer == Player.UnKnown)
                    {
                        currentDeclarer = auctionCurrentPlayer;
                    }
                    break;
                case BidType.pass:
                    if (Util.IsSameTeam(auctionCurrentPlayer, currentDeclarer))
                    {
                        switch (currentBidType)
                        {
                            case BidType.bid:
                                EnableButtons(new[] { Bid.Dbl });
                                DisableButtons(new[] { Bid.Rdbl });
                                break;
                            case BidType.dbl:
                                DisableButtons(new[] { Bid.Dbl, Bid.Rdbl });
                                break;
                        }
                    }
                    else
                    {
                        switch (currentBidType)
                        {
                            case BidType.bid:
                                DisableButtons(new[] { Bid.Dbl, Bid.Rdbl });
                                break;
                            case BidType.dbl:
                                EnableButtons(new[] { Bid.Rdbl });
                                DisableButtons(new[] { Bid.Dbl });
                                break;
                        }

                    }
                    break;
                case BidType.dbl:
                    EnableButtons(new[] { Bid.Rdbl });
                    DisableButtons(new[] { Bid.Dbl });
                    break;
                case BidType.rdbl:
                    DisableButtons(new[] { Bid.Dbl, Bid.Rdbl });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Newbid.bidType.ToString());
            }
            SuitBids = ObjectCloner.ObjectCloner.DeepClone(lSuitBids);
            OnPropertyChanged(nameof(SuitBids));
            NonSuitBids = ObjectCloner.ObjectCloner.DeepClone(lNonSuitBids);
            OnPropertyChanged(nameof(NonSuitBids));

            void EnableButtons(IEnumerable<Bid> bids)
            {
                foreach (var bid in bids)
                {
                    FindButton(bid).IsEnabled = true;
                }
            }

            void DisableButtons(IEnumerable<Bid> bids)
            {
                foreach (var bid in bids)
                {
                    FindButton(bid).IsEnabled = false;
                }
            }
            BidEnable FindButton(Bid bid)
            {
                return lNonSuitBids.Find(x => x.Bid == bid);
            }
        }
    }
}
