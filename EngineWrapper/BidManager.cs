using Common;

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
            var bid = CalculateBid(bidIdFromRule, description, auction.currentPosition, phase);

            if (isPhaseOpener)
                phaseOpener = nextPhase;
            else
                phaseDefensive = nextPhase;
            return bid;
        }

        private static Bid CalculateBid(int bidIdFromRule, string description, int position, Phase phase)
        {
            if (bidIdFromRule == 0)
                return Bid.PassBid;

            var currentBid = Bid.GetBid(bidIdFromRule);
            currentBid.description = description;

            if (bidIdFromRule != 0)
                (currentBid.minRecords, currentBid.maxRecords) = BidGenerator.GetRecords(currentBid, phase, position);

            return currentBid;
        }

        public string GetInformation(Bid bid, int position)
        {
            var (minRecords, maxRecords) = BidGenerator.GetRecords(bid, GetCurrentPhase(position), position);
            return Util.GetInformation(minRecords, maxRecords);
        }

        public void Init()
        {
            phaseOpener = Phase.Opening;
            phaseDefensive = Phase.Opening;
        }
    }
}