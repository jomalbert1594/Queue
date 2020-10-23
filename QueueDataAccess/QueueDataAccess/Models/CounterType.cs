using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QueueDataAccess.Models
{
    public class CounterType
    {
        public int CounterTypeId { get; set; }

        //[Required(ErrorMessage = "Counter name is required")]
        public string CounterName { get; set; }

        public string CounterShortName { get; set; }

        public bool IsEndpoint { get; set; }

        // Navigational Properties
        public List<Counter> Counters { get; set; }

        public List<TransControl> TransControls { get; set; }
    }
}
