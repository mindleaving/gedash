using System;
using System.Net;

namespace NetworkCommunication.Objects
{
    public class Alarm
    {
        public Alarm(IPAddress ipAddress,
            string wardName,
            string bedName,
            SensorType sensorType,
            string message,
            uint messageCounter,
            DateTime timestamp)
        {
            IPAddress = ipAddress;
            WardName = wardName;
            BedName = bedName;
            SensorType = sensorType;
            Message = message;
            MessageCounter = messageCounter;
            Timestamp = timestamp;
        }


        public IPAddress IPAddress { get; }
        public string WardName { get; }
        public string BedName { get; }
        public SensorType SensorType { get; }
        public string Message { get; }
        public uint MessageCounter { get; }
        public DateTime Timestamp { get; }

        public override string ToString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss}: {Message}";
        }
    }
}