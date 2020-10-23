using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueueingApi.Model.Counters
{
    public class UpdateCounter
    {

        public int CounterNo { get; set; }

        public int CounterTypeId { get; set; }

        public string CounterName { get; set; }
    }
}
