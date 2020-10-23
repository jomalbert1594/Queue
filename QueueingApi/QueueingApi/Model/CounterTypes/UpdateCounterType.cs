using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueueingApi.Model.CounterTypes
{
    public class UpdateCounterType
    {

        public bool IsEndpoint { get; set; }

        public string CounterTypeName { get; set; }

        public string ShortName { get; set; }
    }
}
