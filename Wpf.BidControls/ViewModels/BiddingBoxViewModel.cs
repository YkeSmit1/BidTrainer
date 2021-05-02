using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Common;
using MvvmHelpers;
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

        public ObservableCollection<Grouping<int, BidEnable>> SuitBids { get; set; } = new();
        public ObservableCollection<BidEnable> NonSuitBids { get; set; } = new();
        private BidType currentBidType = BidType.pass;
        private Player currentDeclarer = Player.UnKnown;

        public DoBidCommand DoBid { get; set; }
        public bool IsEnabled { get; set; }

        public BiddingBoxViewModel()
        {
            SuitBids = new ObservableCollection<Grouping<int, BidEnable>>(Enumerable.Range(1, 7)
                .Select(level => new Grouping<int, BidEnable>(level, Enum.GetValues(typeof(Suit)).Cast<Suit>()
                .Select(suit => new BidEnable { Bid = new Bid(level, suit), IsEnabled = true }))));
            NonSuitBids = new ObservableCollection<BidEnable> {
                new BidEnable { Bid = Bid.PassBid, IsEnabled = true },
                new BidEnable { Bid = Bid.Dbl, IsEnabled = true },
                new BidEnable { Bid = Bid.Rdbl, IsEnabled = true } };
            ClearBiddingBox();
        }

        public void ClearBiddingBox()
        {
            SuitBids.SelectMany(bid => bid).ToList().ForEach(x => x.IsEnabled = true);
            NonSuitBids.ToList().ForEach(x => x.IsEnabled = true);
            currentBidType = BidType.pass;
            currentDeclarer = Player.UnKnown;

            SuitBids = ObjectCloner.ObjectCloner.DeepClone(SuitBids);
            OnPropertyChanged(nameof(SuitBids));
            NonSuitBids = ObjectCloner.ObjectCloner.DeepClone(NonSuitBids);
            OnPropertyChanged(nameof(NonSuitBids));
        }

        public void UpdateButtons(Bid Newbid, Player auctionCurrentPlayer)
        {
            var lSuitBids = SuitBids;
            var lNonSuitBids = NonSuitBids;

            switch (Newbid.bidType)
            {
                case BidType.bid:
                    SetButtons(true, Bid.Dbl);
                    SetButtons(false, Bid.Rdbl);
                    foreach (var bids in lSuitBids.SelectMany(bidsPerRank => bidsPerRank.Where(bids => bids.Bid.bidType == BidType.bid && bids.Bid <= Newbid)))
                        bids.IsEnabled = false;

                    if (!Util.IsSameTeam(auctionCurrentPlayer, currentDeclarer))
                        currentDeclarer = auctionCurrentPlayer;

                    break;
                case BidType.pass:
                    if (Util.IsSameTeam(auctionCurrentPlayer, currentDeclarer))
                    {
                        switch (currentBidType)
                        {
                            case BidType.bid:
                                SetButtons(true, Bid.Dbl);
                                SetButtons(false, Bid.Rdbl);
                                break;
                            case BidType.dbl:
                                SetButtons(false, Bid.Dbl, Bid.Rdbl);
                                break;
                        }
                    }
                    else
                    {
                        switch (currentBidType)
                        {
                            case BidType.bid:
                                SetButtons(false, Bid.Dbl, Bid.Rdbl);
                                break;
                            case BidType.dbl:
                                SetButtons(true, Bid.Rdbl);
                                SetButtons(false, Bid.Dbl);
                                break;
                        }
                    }
                    break;
                case BidType.dbl:
                    SetButtons(true, Bid.Rdbl);
                    SetButtons(false, Bid.Dbl);
                    break;
                case BidType.rdbl:
                    SetButtons(false, Bid.Dbl, Bid.Rdbl);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Newbid.bidType.ToString());
            }
            currentBidType = Newbid.bidType;

            SuitBids = ObjectCloner.ObjectCloner.DeepClone(lSuitBids);
            OnPropertyChanged(nameof(SuitBids));
            NonSuitBids = ObjectCloner.ObjectCloner.DeepClone(lNonSuitBids);
            OnPropertyChanged(nameof(NonSuitBids));

            void SetButtons(bool enable, params Bid[] bids)
            {
                foreach (var bid in bids)
                    lNonSuitBids.First(x => x.Bid == bid).IsEnabled = enable;
            }
        }
    }
}
