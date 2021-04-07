using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Newtonsoft.Json;

namespace BidTrainer
{
    internal class BidGenerator
    {
        internal (int, Phase, string) GetBid(string handsString, Auction auction, Phase phase)
        {
            var description = new StringBuilder(128);
            int lastBidId = Bid.GetBidId(auction.currentContract);
            var bidId = Pinvoke.GetBidFromRule(phase, handsString, lastBidId, auction.currentPosition, out var nextPhase, description);
            return (bidId, nextPhase, description.ToString());
        }

        internal static (Dictionary<string, int> minRecords, Dictionary<string, int> maxRecords) GetRecords(Bid bid, Phase phase, int position)
        {
            var informationJson = new StringBuilder(1024);
            Pinvoke.GetRulesByBid(phase, Bid.GetBidId(bid), position, informationJson);
            var records = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(informationJson.ToString());
            var minRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Min")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Min());
            var maxRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Max")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Max());
            return (minRecords, maxRecords);
        }

    }
}