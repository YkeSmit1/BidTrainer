using Common;
using System.Collections.Generic;

namespace EngineWrapper
{
    public class BidManager
    {
        private Phase phaseOpener = Phase.Opening;
        private Phase phaseDefensive = Phase.Opening;

        public Phase GetCurrentPhase(int position)
        {
            var isPhaseOpener = position % 2 == 1;
            return isPhaseOpener ? phaseOpener : phaseDefensive;
        }

        public Bid GetBid(Auction auction, string handsString)
        {
            var isPhaseOpener = auction.currentPosition % 2 == 1;
            var phase = GetCurrentPhase(auction.currentPosition);

            var (bidIdFromRule, nextPhase, description) = BidGenerator.GetBid(handsString, auction, phase);
            var bid = CalculateBid(bidIdFromRule, description, auction, phase);

            if (isPhaseOpener)
                phaseOpener = nextPhase;
            else
                phaseDefensive = nextPhase;
            return bid;
        }

        private static Bid CalculateBid(int bidIdFromRule, string description, Auction auction, Phase phase)
        {
            if (bidIdFromRule == 0)
                return Bid.PassBid;

            var currentBid = Bid.GetBid(bidIdFromRule);
            currentBid.description = description;

            if (bidIdFromRule != 0)
                (currentBid.minRecords, currentBid.maxRecords, currentBid.ids) = BidGenerator.GetRecords(currentBid, phase, auction);

            return currentBid;
        }

        public string GetInformation(Bid bid, Auction auction)
        {
            var (minRecords, maxRecords, _) = BidGenerator.GetRecords(bid, GetCurrentPhase(auction.currentPosition), auction);
            return Util.GetInformation(minRecords, maxRecords);
        }

        public void Init()
        {
            phaseOpener = Phase.Opening;
            phaseDefensive = Phase.Opening;
        }

        public Auction GetAuction(Dictionary<Player, string> deal, Player dealer)
        {
            Init();
            var auction = new Auction();
            auction.Clear(dealer);
            while (!auction.IsEndOfBidding())
            {
                var bid = GetBid(auction, deal[auction.CurrentPlayer]);
                auction.AddBid(bid);
            }
            return auction;
        }

    }
}