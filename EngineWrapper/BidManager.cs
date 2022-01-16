using Common;
using System.Collections.Generic;

namespace EngineWrapper
{
    public class BidManager
    {
        public static Bid GetBid(Auction auction, string handsString)
        {
            var (bidIdFromRule, description) = BidGenerator.GetBid(handsString, auction);
            var bid = CalculateBid(bidIdFromRule, description);

            return bid;
        }

        private static Bid CalculateBid(int bidIdFromRule, string description)
        {
            if (bidIdFromRule == 0)
                return Bid.PassBid;

            var currentBid = Bid.GetBid(bidIdFromRule);
            currentBid.description = description;

            return currentBid;
        }

        public static string GetInformation(Bid bid, Auction auction)
        {
            var bidInformation = BidGenerator.GetRecords(bid, auction);
            return GetInformation(bidInformation.minRecords, bidInformation.maxRecords);

            static string GetInformation(Dictionary<string, int> minRecords, Dictionary<string, int> maxRecords)
            {
                return minRecords.Count == 0 ? "No information" : $"Spades: {minRecords["MinSpades"]} - {maxRecords["MaxSpades"]}" +
                    $"\nHearts: {minRecords["MinHearts"]} - {maxRecords["MaxHearts"]}" +
                    $"\nDiamonds: {minRecords["MinDiamonds"]} - {maxRecords["MaxDiamonds"]}" +
                    $"\nClubs: {minRecords["MinClubs"]} - {maxRecords["MaxClubs"]}" +
                    $"\nHcp: {minRecords["MinHcp"]} - {maxRecords["MaxHcp"]}";
            }
        }

        public static Auction GetAuction(Dictionary<Player, string> deal, Player dealer)
        {
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