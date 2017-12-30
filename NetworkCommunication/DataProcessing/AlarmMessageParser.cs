using System;
using System.Linq;
using System.Net;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataProcessing
{
    public class AlarmMessageParser
    {
        public Alarm Parse(byte[] buffer, DateTime timestamp)
        {
            var broadcastAddress = new IPAddress(buffer.Take(4).ToArray());
            var ipAddress = new IPAddress(buffer.Skip(6).Take(4).ToArray());
            var wardName = new string(buffer
                .Skip(24)
                .Select(b => (char) b)
                .TakeWhile(c => c != '|')
                .ToArray());
            var bedName = new string(buffer
                .Skip(12 + wardName.Length + 1)
                .TakeWhile(b => b != 0x00)
                .Select(b => (char)b)
                .ToArray());
            var message = new string(buffer
                .Skip(66)
                .TakeWhile(b => b != 0x00)
                .Select(b => (char) b)
                .ToArray());
            var sensorType = DetermineSensorType(buffer[63]);
            var messageCounterBytes = buffer.Skip(80).Take(4);
            var messageCounter = BitConverter.ToUInt32(messageCounterBytes.Reverse().ToArray(), 0);
            return new Alarm(
                ipAddress, 
                wardName,
                bedName,
                sensorType,
                message, 
                messageCounter,
                timestamp);
        }

        private SensorType DetermineSensorType(byte code)
        {
            switch (code)
            {
                case 0x2d:
                    return SensorType.SpO2;
                case 0x3a:
                    return SensorType.Ecg;
                default:
                    return SensorType.Undefined;
            }
        }
    }
}