using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using MoreLinq;
using Newtonsoft.Json;

namespace EngineWrapper
{
    public class BidGenerator
    {
        public static (int, Phase, string) GetBid(string handsString, Auction auction, Phase phase)
        {
            var description = new StringBuilder(128);
            var bidsPartner = auction.GetBids(Util.GetPartner(auction.CurrentPlayer));
            var minLengthPartner = GetMinSuitLength(bidsPartner);

            var bidsOpener = auction.GetBids(auction.GetDeclarer());
            var minLengthOpener = GetMinSuitLength(bidsOpener);

            var bidId = Pinvoke.GetBidFromRule(phase, handsString, Bid.GetBidId(auction.currentContract), auction.currentPosition,
                minLengthPartner, minLengthOpener, auction.GetBidsAsString(), auction.IsCompetitive(), out var nextPhase, description);
            if (bidId == 0)
            {
                var hcpPartner = GetHcp(bidsPartner);
                var bid = hcpPartner == 0 ? Bid.PassBid : GetCalculatedBid(handsString, minLengthPartner, hcpPartner);
                if (bid > auction.currentContract)
                    bidId = Bid.GetBidId(bid);
            }
            return (bidId, nextPhase, description.ToString());

            static int[] GetMinSuitLength(IEnumerable<Bid> bids)
            {
                return bids.Any() ? new int[] {
                    CombineMinRecords(bids, "MinSpades"),
                    CombineMinRecords(bids, "MinHearts"),
                    CombineMinRecords(bids, "MinDiamonds"),
                    CombineMinRecords(bids, "MinClubs")} : new[] { 0, 0, 0, 0 };
            }

            static int GetHcp(IEnumerable<Bid> bids)
            {
                return bids.Any() ? CombineMinRecords(bids, "MinHcp") : 0;
            }

            static int CombineMinRecords(IEnumerable<Bid> bidsPartner, string property)
            {
                return bidsPartner.Select(x => x.minRecords == null || !x.minRecords.Any() ? 0 : x.minRecords[property]).Max();
            }

            static Bid GetCalculatedBid(string handsString, int[] minLengthPartner, int hcpPartner)
            {
                var suits = handsString.Split(',');
                var majorFits = minLengthPartner
                    .Zip(suits, (x, y) => x + y.Length)
                    .Take(2)
                    .Select((x, index) => (x, (Suit)(3 - index)))
                    .Where(z => z.x >= 8);
                var hcp = Util.GetHcpCount(handsString);                
                var playingSuit = !majorFits.Any() ? Suit.NoTrump : majorFits.MaxBy(z => z.x).First().Item2;
                var hcpPartnership = hcp + hcpPartner;
                if (hcpPartnership < 23)
                    return Bid.PassBid;
                var rank = playingSuit == Suit.NoTrump ? 2 : 3;
                if (hcpPartnership >= 25)
                    rank++;
                return new Bid(rank, playingSuit);
            }
        }

        public static (Dictionary<string, int> minRecords, Dictionary<string, int> maxRecords, List<int> ids) GetRecords(Bid bid, Phase phase, Auction auction)
        {
            var informationJson = new StringBuilder(8192);
            Pinvoke.GetRulesByBid(phase, Bid.GetBidId(bid), auction.currentPosition, auction.GetBidsAsString(), auction.IsCompetitive(), informationJson);
            var records = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(informationJson.ToString());
            var minRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Min")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Min());
            var maxRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Max")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Max());
            var ids = records.SelectMany(x => x).Where(x => x.Key == "Id").Select(x => Convert.ToInt32(x.Value)).ToList();
            return (minRecords, maxRecords, ids);
        }
    }
}