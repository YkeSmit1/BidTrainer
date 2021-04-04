using Common;

namespace BidTrainer
{
    internal class BiddingState
    {
        public Bid CurrentBid { get; set; }
        public bool EndOfBidding { get; set; }

        public void Init()
        {
            CurrentBid = Bid.PassBid;
            EndOfBidding = false;
        }

        public void CalculateBid(int bidIdFromRule, string description)
        {
            if (bidIdFromRule == 0)
            {
                CurrentBid = Bid.PassBid;
                //EndOfBidding = true;
            }

            CurrentBid = Bid.GetBid(bidIdFromRule);
            CurrentBid.description = description;
        }
    }
}