using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QueueDataAccess.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public int PrioNo { get; set; }

        public bool IsDeleted { get; set; }

        // Transaction name in abbreviation
        public string Name { get; set; }

        public bool IsServed { get; set; }

        // The date/time it have started to wait for a particular transaction
        public DateTime? DateTimeOrdered { get; set; }

        // Version controller
        [Timestamp]
        public ulong RowVersion2 { get; set; }

        // Foreign Key
        public int? PrevId { get; set; }

        public int? NextId { get; set; }

        public int? TransPoolId { get; set; }

        // Navigation properties

        public Transaction PrevTrans { get; set; }

        public Transaction NextTrans { get; set; }

        public TransPool TransPool { get; set; }
    }
}
