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
            var bidsPartner = auction.GetBids(Util.GetPartner(auction.CurrentPlayer)).Where(x => x.extraInformation != null);
            var minLengthPartner = GetMinSuitLength(bidsPartner);

            var bidsOpener = auction.GetBids(auction.GetDeclarer()).Where(x => x.extraInformation != null);
            var minLengthOpener = GetMinSuitLength(bidsOpener);
            var allControlsPresent = GetAllControlsPresent();

            var bidId = Pinvoke.GetBidFromRule(phase, handsString, auction.GetBidsAsStringASCII(), out var nextPhase, description);
            if (bidId == 0)
            {
                var hcpPartner = GetHcp(bidsPartner);
                var bid = hcpPartner == 0 ? Bid.PassBid : GetCalculatedBid(hcpPartner);
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

            static int CombineMinRecords(IEnumerable<Bid> bids, string property)
            {
                return bids.Select(x => !((BidInformation)x.extraInformation).minRecords.Any() ? 0 : ((BidInformation)x.extraInformation).minRecords[property]).Max();
            }

            bool GetAllControlsPresent()
            {
                var controlInfoPartner = bidsPartner.Select(x => ((BidInformation)x.extraInformation).controls);
                var controls = handsString.Split(",").Select((x, index) => (HasControl(x), index));
                var controlsPartnership = controls.Select(x => x.Item1 || controlInfoPartner.Any(y => y[x.index].HasValue && y[x.index].Value));
                return controlsPartnership.All(x => x);

                static bool HasControl(string x)
                {
                    return x.Contains("A") || x.Contains("K") || x.Length <= 1;
                }
            }

            Bid GetCalculatedBid(int hcpPartner)
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
                    var keyCards = Util.GetKeyCards(handsString, playingSuit);
                    var bidsWithKeyCards = bidsPartner.Where(x => ((BidInformation)x.extraInformation).possibleKeyCards.Any());
                    var keyCardsPartner = bidsWithKeyCards.Any() ? ((BidInformation)bidsWithKeyCards.Single().extraInformation).possibleKeyCards : null;
                    var totalKeyCards = keyCards + (keyCardsPartner?.SingleOrDefault(x => x + keyCards <= 5) ?? 0);

                    var trumpQueen = Util.GetHasTrumpQueen(handsString, playingSuit);
                    var bidsWithTrumpQueen = bidsPartner.Where(x => ((BidInformation)x.extraInformation).trumpQueen != null);
                    var trumpQueenPartner = bidsWithTrumpQueen.Any() ? ((BidInformation)bidsWithTrumpQueen.Single().extraInformation).trumpQueen : null;

                    var totalTrumpQueen = trumpQueen || (trumpQueenPartner.HasValue && trumpQueenPartner.Value);

                    return totalKeyCards >= 4 && totalTrumpQueen && allControlsPresent;
                }
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