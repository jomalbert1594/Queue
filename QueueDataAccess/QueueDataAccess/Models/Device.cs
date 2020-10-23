using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace QueueDataAccess.Models
{
    public class Device
    {
        public int DeviceId { get; set; } 

        public string DeviceSerialNo { get; set; } // android ID and desktop ID

        public bool IsDesktop { get; set; } 

        public string ConnectionSerial { get; set; }

        public int? CouterTypeId { get; set; }
    }
}
