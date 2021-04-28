using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Newtonsoft.Json;

namespace EngineWrapper
{
    public class BidGenerator
    {
        public static (int, Phase, string) GetBid(string handsString, Auction auction, Phase phase)
        {
            var description = new StringBuilder(128);
            var bidsPartner = auction.GetBids(Util.GetPartner(auction.currentPlayer));
            var minLengthPartner = bidsPartner.Any() ? bidsPartner.Last().GetMinSuitLength() : new[] { 0, 0, 0, 0 };
            var bidId = Pinvoke.GetBidFromRule(phase, handsString, Bid.GetBidId(auction.currentContract), auction.currentPosition,
                minLengthPartner[0], minLengthPartner[1], minLengthPartner[2], minLengthPartner[3], out var nextPhase, description);
            return (bidId, nextPhase, description.ToString());
        }

        public static (Dictionary<string, int> minRecords, Dictionary<string, int> maxRecords) GetRecords(Bid bid, Phase phase, int position)
        {
            var informationJson = new StringBuilder(4096);
            Pinvoke.GetRulesByBid(phase, Bid.GetBidId(bid), position, informationJson);
            var records = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(informationJson.ToString());
            var minRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Min")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Min());
            var maxRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Max")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Max());
            return (minRecords, maxRecords);
        }
    }
}