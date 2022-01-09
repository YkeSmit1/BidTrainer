using Common;
using System.Collections.Generic;
using System.Linq;

namespace EngineWrapper
{
    public class BidManager
    {
        private Phase phaseOpener = Phase.Opening;
        private Phase phaseDefensive = Phase.Opening;
        public List<string> SlamBids = new();

        public Phase GetCurrentPhase(int position)
        {
            var isPhaseOpener = position % 2 == 1;
            return isPhaseOpener ? phaseOpener : phaseDefensive;
        }

        public Bid GetBid(Auction auction, string handsString)
        {
            var isPhaseOpener = auction.currentPosition % 2 == 1;
            var phase = GetCurrentPhase(auction.currentPosition);

            string slamBids = string.Join("", SlamBids);
            var (bidIdFromRule, nextPhase, description) = BidGenerator.GetBid(handsString, auction, phase);
            var bid = CalculateBid(bidIdFromRule, description, auction, phase, nextPhase, slamBids);
            if (nextPhase == Phase.SlamBidding)
                SlamBids.Add(bid.ToStringASCII());

            if (isPhaseOpener)
                phaseOpener = nextPhase;
            else
                phaseDefensive = nextPhase;
            return bid;
        }

        private static Bid CalculateBid(int bidIdFromRule, string description, Auction auction, Phase phase, Phase nextPhase, string slamBids)
        {
            if (bidIdFromRule == 0)
                return Bid.PassBid;

            var currentBid = Bid.GetBid(bidIdFromRule);
            currentBid.description = description;

            if (bidIdFromRule != 0)
                currentBid.extraInformation = BidGenerator.GetRecords(currentBid, phase, nextPhase, auction, string.Join("", slamBids));

            return currentBid;
        }

        public string GetInformation(Bid bid, Auction auction)
        {
            var bidInformation = BidGenerator.GetRecords(bid, GetCurrentPhase(auction.currentPosition), Phase.Unknown, auction, string.Join("", SlamBids));
            return GetInformation(bidInformation.minRecords, bidInformation.maxRecords);

            static string GetInformation(Dictionary<string, int> minRecords, Dictionary<string, int> maxRecords)
            {
                return minRecords.Count() == 0 ? "No information" : $"Spades: {minRecords["MinSpades"]} - {maxRecords["MaxSpades"]}" +
                    $"\nHearts: {minRecords["MinHearts"]} - {maxRecords["MaxHearts"]}" +
                    $"\nDiamonds: {minRecords["MinDiamonds"]} - {maxRecords["MaxDiamonds"]}" +
                    $"\nClubs: {minRecords["MinClubs"]} - {maxRecords["MaxClubs"]}" +
                    $"\nHcp: {minRecords["MinHcp"]} - {maxRecords["MaxHcp"]}";
            }
        }


        public void Init()
        {
            phaseOpener = Phase.Opening;
            phaseDefensive = Phase.Opening;
            SlamBids.Clear();
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