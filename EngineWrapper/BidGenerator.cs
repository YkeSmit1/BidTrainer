using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EngineWrapper
{
    public class BidGenerator
    {
        public static (int, Phase, string) GetBid(string handsString, Auction auction, Phase phase)
        {
            var description = new StringBuilder(128);
            var bidId = Pinvoke.GetBidFromRule(phase, handsString, auction.GetBidsAsStringASCII(), out var nextPhase, description);

            if (bidId == 0)
            {
                var info = GetInformationFromAuction(auction);
                var bid = (long)info["minHcpPartner"] == 0 ? Bid.PassBid : GetCalculatedBid(info);
                if (bid > auction.currentContract)
                    bidId = Bid.GetBidId(bid);
            }
            return (bidId, nextPhase, description.ToString());

            Bid GetCalculatedBid(Dictionary<string, object> info)
            {
                var suits = handsString.Split(',');
                var minSuitLengthsPartner = ((JArray)info["minSuitLengthsPartner"]).ToObject<int[]>();
                var majorFits = minSuitLengthsPartner
                    .Zip(suits, (x, y) => x + y.Length)
                    .Take(2)
                    .Select((x, index) => (x, (Suit)(3 - index)))
                    .Where(z => z.x >= 8);
                var hcp = Util.GetHcpCount(handsString);                
                var playingSuit = !majorFits.Any() ? Suit.NoTrump : majorFits.MaxBy(z => z.x).First().Item2;
                var hcpPartnership = hcp + (long)info["minHcpPartner"];
                if (hcpPartnership < 23)
                    return Util.IsSameTeam(auction.CurrentPlayer, auction.GetDeclarer()) && majorFits.Any()
                        ? Bid.CheapestContract(auction.currentContract, majorFits.First().Item2)
                        : Bid.PassBid;
                 
                var rank = playingSuit == Suit.NoTrump ? 2 : 3;
                if (hcpPartnership >= 25)
                    if (hcpPartnership < 29)
                        rank++;
                    else 
                        rank = SlamIsPossible() ? 6 :new Bid(4, playingSuit) >= auction.currentContract ? 4 : 5;

                return new Bid(rank, playingSuit);

                bool SlamIsPossible()
                {
                    var totalKeyCards = Util.GetKeyCards(handsString, playingSuit) + (long)info["keyCardsPartner"];
                    var totalTrumpQueen = Util.GetHasTrumpQueen(handsString, playingSuit) || (bool)info["trumpQueenPartner"];

                    return totalKeyCards >= 4 && totalTrumpQueen && GetAllControlsPresent();
                }

                bool GetAllControlsPresent()
                {
                    var controlsPartner = ((JArray)info["controls"]).ToObject<bool[]>();
                    var controls = handsString.Split(",").Select((x, index) => (HasControl(x), index));
                    var controlsPartnership = controls.Select(x => x.Item1 || controlsPartner[x.index]);
                    return controlsPartnership.All(x => x);

                    static bool HasControl(string x)
                    {
                        return x.Contains("A") || x.Contains("K") || x.Length <= 1;
                    }
                }
            }

            static Dictionary<string, object> GetInformationFromAuction(Auction auction)
            {
                var stringBuilder = new StringBuilder(8129);
                Pinvoke.GetInformationFromAuction(auction.GetBidsAsStringASCII(), stringBuilder);
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(stringBuilder.ToString());
            }
        }

        public static BidInformation GetRecords(Bid bid, Phase phase, Phase nextPhase, Auction auction, string slamBids)
        {
            var informationJson = new StringBuilder(8192);
            if (nextPhase == Phase.SlamBidding)
                Pinvoke.GetRelativeRulesByBid(Bid.GetBidId(bid), slamBids, informationJson);
            else
                Pinvoke.GetRulesByBid(phase, Bid.GetBidId(bid), auction.currentPosition, auction.GetBidsAsStringASCII(), auction.IsCompetitive(), informationJson);
            var records = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(informationJson.ToString());
            var bidInformation = new BidInformation
            {
                minRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Min")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Min()),
                maxRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Max")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Max()),
                ids = records.SelectMany(x => x).Where(x => x.Key == "Id").Select(x => Convert.ToInt32(x.Value)).ToList(),
                controls = records.Any() ? new List<bool?> { GetHasProperty(records, "SpadeControl"), GetHasProperty(records, "HeartControl"), GetHasProperty(records, "DiamondControl"), 
                    GetHasProperty(records, "ClubControl") } : new List<bool?> { null, null, null, null},
                possibleKeyCards = records.SelectMany(x => x).Where(x => x.Key == "KeyCards" && !string.IsNullOrWhiteSpace(x.Value)).Select(x => int.Parse(x.Value)).ToList(),
                trumpQueen = records.Any() ? GetHasProperty(records, "TrumpQueen") : null
            };
            return bidInformation;

            static bool? GetHasProperty(List<Dictionary<string, string>> records, string fieldName)
            {
                var recordsWithValue = records.SelectMany(x => x).Where(x => x.Key == fieldName && !string.IsNullOrWhiteSpace(x.Value));
                bool? p = recordsWithValue.Any() ? int.Parse(recordsWithValue.First().Value) == 1 : null;
                return p;
            }
        }
    }
}