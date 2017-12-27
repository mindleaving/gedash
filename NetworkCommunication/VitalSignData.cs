using System;
using System.Collections.Generic;
using System.Net;

namespace NetworkCommunication
{
    public class VitalSignData
    {
        public VitalSignData(DateTime timestamp)
        {
            Timestamp = timestamp;
        }

        public DateTime Timestamp { get; }
        public IPAddress IPAddress { get; set; }
        public int MessageCounter { get; set; }
        public List<VitalSignValue> VitalSignValues { get; set; }
    }
}