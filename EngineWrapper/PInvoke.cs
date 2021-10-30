using System.Runtime.InteropServices;
using System.Text;

namespace EngineWrapper
{
    public class Pinvoke
    {
        [DllImport("Engine")]
        public static extern int GetBidFromRule(Phase phase, string hand, int lastBidId, int position,
            int[] minSuitsPartner, int[] minSuitsOpener, string previousBidding, bool isCompetitive, out Phase newPhase, StringBuilder description);
        [DllImport("Engine", CharSet = CharSet.Ansi)]
        public static extern void GetRulesByBid(Phase phase, int bidId, int position, string previousBidding, bool isCompetitive, StringBuilder information);
        [DllImport("Engine")]
        public static extern void SetModules(int modules);
    }
}
