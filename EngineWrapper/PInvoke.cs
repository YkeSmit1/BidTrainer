using System.Runtime.InteropServices;
using System.Text;

namespace EngineWrapper
{
    public static class PInvoke
    {
        [DllImport("Engine")]
        public static extern int GetBidFromRule(string hand, string previousBidding, StringBuilder description);
        [DllImport("Engine", CharSet = CharSet.Ansi)]
        public static extern void GetRulesByBid(int bidId, string previousBidding, StringBuilder information);
        [DllImport("Engine", CharSet = CharSet.Ansi)]
        public static extern void SetModules(int modules);

        [DllImport("Engine")]
        public static extern void GetInformationFromAuction(string previousBidding, StringBuilder informationFromAuctionJson);
        [DllImport("Engine")]
        public static extern int Setup(string database);
    }
}
