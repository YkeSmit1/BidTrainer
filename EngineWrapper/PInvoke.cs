using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineWrapper
{
    internal class Pinvoke
    {
        [DllImport("Engine.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
        internal static extern int GetBidFromRule(Phase phase, string hand, int lastBidId, int position,
            int[] minSuitsPartner, int[] minSuitsOpener, out Phase newPhase, StringBuilder description);
        [DllImport("Engine.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
        internal static extern void GetRulesByBid(Phase phase, int bidId, int position, StringBuilder information);
    }
}
