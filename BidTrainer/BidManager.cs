using Common;

namespace BidTrainer
{
    internal class BidManager
    {
        private readonly BidGenerator bidGenerator = new BidGenerator();
        private Phase phase = Phase.Opening;

        public Bid GetBid(Auction auction, string handsString)
        {
            var (bidIdFromRule, nextPhase, description) = bidGenerator.GetBid(handsString, auction, phase);
            phase = nextPhase;
            return CalculateBid(bidIdFromRule, description);
        }

        private Bid CalculateBid(int bidIdFromRule, string description)
        {
            if (bidIdFromRule == 0)
                return Bid.PassBid;

            var currentBid = Bid.GetBid(bidIdFromRule);
            currentBid.description = description;
            return currentBid;
        }

        public void Init()
        {
            phase = Phase.Opening;
        }
    }
}