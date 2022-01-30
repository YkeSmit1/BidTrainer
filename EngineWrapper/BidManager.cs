using Common;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EngineWrapper
{
    public static class BidManager
    {
        public static Bid GetBid(Auction auction, string handsString)
        {
            var description = new StringBuilder(128);
            var bidId = Pinvoke.GetBidFromRule(handsString, auction.GetBidsAsStringASCII(), description);

            if (bidId == 0)
            {
                var calculatedBid = GetCalculatedBid();
                if (calculatedBid > auction.currentContract)
                    return calculatedBid;
            }
            var bid = Bid.GetBid(bidId);
            bid.description = description.ToString();
            return bid;

            Bid GetCalculatedBid()
            {
                var info = GetInformationFromAuction(auction);
                if ((long)info["minHcpPartner"] == 0)
                    return Bid.PassBid;
                var suits = handsString.Split(',');
                var minSuitLengthsPartner = ((JArray)info["minSuitLengthsPartner"]).ToObject<int[]>();
                var majorFits = minSuitLengthsPartner
                    .Zip(suits, (x, y) => x + y.Length)
                    .Take(2)
                    .Select((x, index) => (x, (Suit)(3 - index)))
                    .Where(z => z.x >= 8);
                var playingSuit = !majorFits.Any() ? Suit.NoTrump : majorFits.MaxBy(z => z.x).First().Item2;
                var hcpPartnership = Util.GetHcpCount(handsString) + (long)info["minHcpPartner"];

                if (hcpPartnership < 23)
                    return Util.IsSameTeam(auction.CurrentPlayer, auction.GetDeclarer()) && majorFits.Any()
                        ? Bid.CheapestContract(auction.currentContract, majorFits.First().Item2)
                        : Bid.PassBid;

                var rank = playingSuit == Suit.NoTrump ? 2 : 3;
                if (hcpPartnership >= 25)
                    if (hcpPartnership < 29)
                        rank++;
                    else
                        rank = SlamIsPossible() ? 6 : new Bid(4, playingSuit) >= auction.currentContract ? 4 : 5;

                return new Bid(rank, playingSuit);

                bool SlamIsPossible()
                {
                    if (playingSuit == Suit.NoTrump)
                        return hcpPartnership >= 33;
                    var totalKeyCards = Util.GetKeyCards(handsString, playingSuit) + (long)info["keyCardsPartner"];
                    var totalTrumpQueen = Util.GetHasTrumpQueen(handsString, playingSuit) || (bool)info["trumpQueenPartner"];

                    var controlsPartner = ((JArray)info["controls"]).ToObject<bool[]>();
                    var controls = handsString.Split(",").Select((x, index) => (HasControl(x), index));
                    var controlsPartnership = controls.Select(x => x.Item1 || controlsPartner[x.index]);

                    var slamIsPossible = ((totalKeyCards == 4 && totalTrumpQueen) || totalKeyCards == 5) && controlsPartnership.All(x => x);
                    return slamIsPossible;

                    static bool HasControl(string x) => x.Contains("A") || x.Contains("K") || x.Length <= 1;
                }
            }
        }

        private static Dictionary<string, object> GetInformationFromAuction(Auction auction)
        {
            var stringBuilder = new StringBuilder(8129);
            Pinvoke.GetInformationFromAuction(auction.GetBidsAsStringASCII(), stringBuilder);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(stringBuilder.ToString());
        }

        public static string GetInformation(Bid bid, Auction auction)
        {
            var bidInformation = GetRecords();
            return GetInformation();

            string GetInformation()
            {
                var minRecords = bidInformation.minRecords;
                var maxRecords = bidInformation.maxRecords;
                return minRecords.Count == 0 ? "No information" : 
                    $"Spades: {minRecords["MinSpades"]} - {maxRecords["MaxSpades"]}" +
                    $"\nHearts: {minRecords["MinHearts"]} - {maxRecords["MaxHearts"]}" +
                    $"\nDiamonds: {minRecords["MinDiamonds"]} - {maxRecords["MaxDiamonds"]}" +
                    $"\nClubs: {minRecords["MinClubs"]} - {maxRecords["MaxClubs"]}" +
                    $"\nHcp: {minRecords["MinHcp"]} - {maxRecords["MaxHcp"]}";
            }

            BidInformation GetRecords()
            {
                var info = GetInformationFromAuction(auction);
                var informationJson = new StringBuilder(8192);

                if ((bool)info["isSlamBidding"])
                    Pinvoke.GetRelativeRulesByBid(Bid.GetBidId(bid), auction.GetBidsAsStringASCII(), informationJson);
                else
                    Pinvoke.GetRulesByBid(Bid.GetBidId(bid), auction.GetBidsAsStringASCII(), informationJson);

                var records = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(informationJson.ToString());
                var bidInformation = new BidInformation
                {
                    minRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Min")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Min()),
                    maxRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Max")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Max()),
                    ids = records.SelectMany(x => x).Where(x => x.Key == "Id").Select(x => Convert.ToInt32(x.Value)).ToList(),
                    controls = records.Any() ? new List<bool?> { GetHasProperty("SpadeControl"), GetHasProperty("HeartControl"), GetHasProperty("DiamondControl"), GetHasProperty("ClubControl") } :
                        new List<bool?> { null, null, null, null },
                    possibleKeyCards = records.SelectMany(x => x).Where(x => x.Key == "KeyCards" && !string.IsNullOrWhiteSpace(x.Value)).Select(x => int.Parse(x.Value)).ToList(),
                    trumpQueen = records.Any() ? GetHasProperty("TrumpQueen") : null
                };
                return bidInformation;

                bool? GetHasProperty(string fieldName)
                {
                    var recordsWithValue = records.SelectMany(x => x).Where(x => x.Key == fieldName && !string.IsNullOrWhiteSpace(x.Value));
                    bool? p = recordsWithValue.Any() ? int.Parse(recordsWithValue.First().Value) == 1 : null;
                    return p;
                }
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