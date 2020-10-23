using System;
using System.Collections.Generic;

namespace QueueDataAccess.Models
{
    public class TransPool
    {
        public int TransPoolId { get; set; }

        public DateTime TransactionDate { get; set; }

        // First number generated for the day
        public int First { get; set; }

        // Last number
        public int Last { get; set; }

        // Senior citizen and Persons with disability
        public bool IsSpecial { get; set; }

        // foreign key
        public int? TransControlId { get; set; }

        // Navigation Prop
        public List<Transaction> Transactions { get; set; }

        public TransControl TransControl { get; set; }

    }
}
