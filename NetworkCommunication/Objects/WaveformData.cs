using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NetworkCommunication.Objects
{
    public class WaveformData
    {
        public IPAddress IPAddress { get; }
        public int SequenceNumber { get; }
        public DateTime Timestamp { get; }
        public Dictionary<SensorType, List<short>> SensorWaveforms { get; }

        public WaveformData(
            IPAddress ipAddress,
            DateTime timestamp,
            int sequenceNumber, 
            Dictionary<SensorType, List<short>> sensorWaveforms)
        {
            IPAddress = ipAddress;
            Timestamp = timestamp;
            SensorWaveforms = sensorWaveforms;
            SequenceNumber = sequenceNumber;
        }

        public WaveformData(
            IPAddress ipAddress,
            DateTime timestamp,
            int sequenceNumber, 
            params Waveform[] waveforms)
            : this(ipAddress, timestamp, sequenceNumber, waveforms.ToDictionary(w => w.SensorType, w => w.Values))
        {
        }
    }
}