﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Common
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Auction
    {
        public Player currentPlayer;
        private int currentBiddingRound;
        public Dictionary<int, Dictionary<Player, Bid>> bids { get; set; } = new Dictionary<int, Dictionary<Player, Bid>>();
        public Bid currentContract = Bid.PassBid;
        public bool responderHasSignedOff = false;
        public int currentPosition = 1;

        private string DebuggerDisplay
        {
            get { return GetPrettyAuction(Environment.NewLine); }
        }

        public string GetPrettyAuction(string separator)
        {
            var bidsNorth = GetBids(Player.North);
            var bidsSouth = GetBids(Player.South);
            return string.Join(separator, bidsNorth.Zip(bidsSouth, (x, y) => $"{x}{y} {y.description}")) + (bidsNorth.Count() > bidsSouth.Count() ? separator + bidsNorth.Last() : "");
        }

        public Player GetDeclarer(Suit suit)
        {
            foreach (var biddingRoud in bids.Values)
            {
                foreach (var bid in biddingRoud)
                {
                    if (bid.Value.bidType == BidType.bid && bid.Value.suit == suit)
                        return bid.Key;
                }
            }
            return Player.UnKnown;
        }

        public Player GetDeclarerOrNorth(Suit suit)
        {
            var declarer = GetDeclarer(suit);
            return declarer == Player.UnKnown ? Player.North : declarer;
        }


        public void AddBid(Bid bid)
        {
            if (bid != Bid.PassBid || currentContract != Bid.PassBid)
                currentPosition++;

            if (!bids.ContainsKey(currentBiddingRound))
            {
                bids[currentBiddingRound] = new Dictionary<Player, Bid>();
            }
            bids[currentBiddingRound][currentPlayer] = bid;

            if (currentPlayer == Player.South)
            {
                currentPlayer = Player.West;
                ++currentBiddingRound;
            }
            else
            {
                ++currentPlayer;
            }
            if (bid.bidType == BidType.bid)
            {
                currentContract = bid;
            }
        }

        public void Clear(Player dealer)
        {
            bids.Clear();
            currentPlayer = dealer;
            currentBiddingRound = 1;
            currentContract = Bid.PassBid;
            currentPosition = 1;

            var player = Player.West;
            while (player != dealer)
            {
                if (!bids.ContainsKey(1))
                    bids[1] = new Dictionary<Player, Bid>();
                bids[1][player] = Bid.AlignBid;
                player++;
            }
        }

        public string GetBidsAsString(Player player)
        {
            return bids.Where(x => x.Value.ContainsKey(player)).Aggregate(string.Empty, (current, biddingRound) => current + biddingRound.Value[player]);
        }

        public string GetBidsAsString(Fase fase)
        {
            return GetBidsAsString(new Fase[] { fase });
        }

        public string GetBidsAsString(Fase[] fases)
        {
            const Player south = Player.South;
            return bids.Where(x => x.Value.TryGetValue(south, out var bid) && fases.Contains(bid.fase)).
                Aggregate(string.Empty, (current, biddingRound) => current + biddingRound.Value[south]);
        }

        public IEnumerable<Bid> GetBids(Player player)
        {
            return bids.Where(x => x.Value.ContainsKey(player)).Select(x => x.Value[player]);
        }

        public IEnumerable<Bid> GetBids(Player player, Fase fase)
        {
            return GetBids(player, new Fase[] { fase });
        }

        public IEnumerable<Bid> GetBids(Player player, Fase[] fases)
        {
            return bids.Where(x => x.Value.TryGetValue(player, out var bid) && fases.Contains(bid.fase)).Select(x => x.Value[player]);
        }

        public IEnumerable<Bid> GetPullBids(Player player, Fase[] fases)
        {
            return bids.Where(x => x.Value.TryGetValue(player, out var bid) && fases.Contains(bid.pullFase)).Select(x => x.Value[player]);
        }

        public void SetBids(Player player, IEnumerable<Bid> newBids)
        {
            bids.Clear();
            var biddingRound = 1;
            foreach (var bid in newBids)
            {
                bids[biddingRound] = new Dictionary<Player, Bid>(new List<KeyValuePair<Player, Bid>> { new KeyValuePair<Player, Bid>(player, bid) });
                biddingRound++;
            }
        }

        public bool Used4ClAsRelay()
        {
            var previousBiddingRound = bids.First();
            foreach (var biddingRound in bids.Skip(1))
            {
                if (biddingRound.Value.TryGetValue(Player.North, out var bid) && bid == Bid.fourClubBid)
                    return previousBiddingRound.Value[Player.South] == Bid.threeSpadeBid;

                previousBiddingRound = biddingRound;
            }
            return false;
        }

        public void CheckConsistency()
        {
            var bidsSouth = GetBids(Player.South);
            var previousBid = bidsSouth.First();
            foreach (var bid in bidsSouth.Skip(1))
            {
                if (bid.bidType == BidType.bid)
                {
                    if (bid <= previousBid)
                        throw new InvalidOperationException("Bid is lower");
                    previousBid = bid;
                }
            }
        }

        public bool IsEndOfBidding()
        {
            var allBids = bids.SelectMany(x => x.Value).Select(y => y.Value).Where(z => z.bidType != BidType.align);
            return (allBids.Count() == 4 && allBids.All(bid => bid == Bid.PassBid)) ||
                allBids.Count() > 3 && allBids.TakeLast(3).Count() == 3 && allBids.TakeLast(3).All(bid => bid == Bid.PassBid);
        }
    }
}
