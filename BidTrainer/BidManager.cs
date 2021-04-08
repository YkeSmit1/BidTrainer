using Common;

namespace BidTrainer
{
    public class BidManager
    {
        private readonly BidGenerator bidGenerator = new BidGenerator();
        public Phase phase = Phase.Opening;

        public Bid GetBid(Auction auction, string handsString)
        {
            var (bidIdFromRule, nextPhase, description) = bidGenerator.GetBid(handsString, auction, phase);
            var bid = CalculateBid(bidIdFromRule, description, auction.currentPosition);
            phase = nextPhase;
            return bid;
        }

        private Bid CalculateBid(int bidIdFromRule, string description, int position)
        {
            if (bidIdFromRule == 0)
                return Bid.PassBid;

            var currentBid = Bid.GetBid(bidIdFromRule);
            currentBid.description = description;

            if (bidIdFromRule != 0)
                (currentBid.minRecords, currentBid.maxRecords) = BidGenerator.GetRecords(currentBid, phase, position);

            return currentBid;
        }

        public void Init()
        {
            phase = Phase.Opening;
        }
    }
}