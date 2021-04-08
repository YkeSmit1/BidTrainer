using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BidTrainer
{
    public class Pinvoke
    {
        [DllImport("Engine.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
        public static extern int GetBidFromRule(Phase phase, string hand, int lastBidId, int position,
            int minSpades, int minHearts, int minDiamonds, int minClubs, out Phase newPhase, StringBuilder description);
        [DllImport("Engine.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
        public static extern int GetRulesByBid(Phase phase, int bidId, int position, StringBuilder information);
    }
}
