using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineWrapper
{
    public class BidInformation
    {
        public Dictionary<string, int> minRecords;
        public Dictionary<string, int> maxRecords;
        public List<int> ids;
        public List<bool?> controls;
        public List<int> possibleKeyCards;
        public bool? trumpQueen;
    }
}
