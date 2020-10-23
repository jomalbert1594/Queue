using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QueueDataAccess.Models
{
    public class Counter
    {
        public int CounterId { get; set; }

        public int COunterNo { get; set; }

        public string CounterName { get; set; }

        public bool IsDeleted { get; set; }

        // Version controller
        [Timestamp]
        public ulong RowVersion2 { get; set; }

        // Foreign Key
        //[Required(ErrorMessage = "Counter Type Id is required")]
        public int? CounterTypeId { get; set; }

        public int? TransactionId { get; set; }

        // Navigation properties
        public CounterType CounterType { get; set; }
    }
}
