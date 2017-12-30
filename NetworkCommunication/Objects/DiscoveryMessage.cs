using System;
using System.Net;

namespace NetworkCommunication.Objects
{
    public class DiscoveryMessage
    {
        public DiscoveryMessage(
            IPAddress ipAddress,
            uint messageCounter,
            string wardName, 
            string bedName, 
            PatientInfo patientInfo,
            DateTime timestamp)
        {
            IPAddress = ipAddress;
            MessageCounter = messageCounter;
            WardName = wardName;
            BedName = bedName;
            PatientInfo = patientInfo;
            Timestamp = timestamp;
        }

        public IPAddress IPAddress { get; }
        public uint MessageCounter { get; }
        public string WardName { get; }
        public string BedName { get; }
        public PatientInfo PatientInfo { get; }
        public DateTime Timestamp { get; }
    }
}