using System;
using System.Text;
using Common;

namespace BidTrainer
{
    internal class BidGenerator
    {
        internal (int, Phase, string) GetBid(string handsString, Auction auction, Phase phase)
        {
            var description = new StringBuilder(128);
            var bidId = Pinvoke.GetBidFromRule(phase, handsString, Bid.GetBidId(auction.currentContract), auction.currentPosition, out var nextPhase, description);
            return (bidId, nextPhase, description.ToString());
        }
    }
}