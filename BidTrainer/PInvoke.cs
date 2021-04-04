using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BidTrainer
{
    public class Pinvoke
    {
        [DllImport("Engine.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
        public static extern int GetBidFromRule(Phase phase, string hand, int lastBidId, int position, out Phase newPhase, StringBuilder description);
    }
}
