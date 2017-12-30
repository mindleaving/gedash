using System;
using System.Collections.Generic;
using System.Net;

namespace NetworkCommunication.Objects
{
    public class VitalSignData
    {
        public VitalSignData(
            IPAddress ipAddress, 
            int messageCounter, 
            List<VitalSignValue> vitalSignValues, 
            DateTime timestamp)
        {
            Timestamp = timestamp;
            IPAddress = ipAddress;
            MessageCounter = messageCounter;
            VitalSignValues = vitalSignValues;
        }

        public IPAddress IPAddress { get; }
        public int MessageCounter { get; }
        public List<VitalSignValue> VitalSignValues { get; }
        public DateTime Timestamp { get; }
    }
}