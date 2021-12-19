using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineWrapper
{
    public class Pinvoke
    {
        [DllImport("Engine", CharSet = CharSet.Ansi)]
        public static extern int GetBidFromRule(Phase phase, string hand, int lastBidId, int position,
            int[] minSuitsPartner, int[] minSuitsOpener, out Phase newPhase, StringBuilder description);
        [DllImport("Engine", CharSet = CharSet.Ansi)]
        public static extern void GetRulesByBid(Phase phase, int bidId, int position, StringBuilder information);
        [DllImport("Engine", CharSet = CharSet.Ansi)]
        public static extern int Setup(string database);
    }
}
