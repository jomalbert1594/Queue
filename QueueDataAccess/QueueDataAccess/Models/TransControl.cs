using System;
using System.Collections.Generic;
using System.Text;

namespace QueueDataAccess.Models
{
    public class TransControl
    {
        public int TransControlId { get; set; }

        // Serves as the flag on what to serve next
        public bool? IsSpecial { get; set; }

        // Foreign Key
        public int? CounterTypeId { get; set; }

        // Navigational Properties
        public CounterType CounterType { get; set; }

        public List<TransPool> TransPools { get; set; }

    }
}
