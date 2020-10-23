using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueueingApi.Model.Counters
{
    public class CounterInfo
    {
        public int CounterId { get; set; }

        public int CounterNo { get; set; }

        public string CounterName { get; set; }

        public string CounterType { get; set; }
    }
}
