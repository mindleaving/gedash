using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkCommunication
{
    public class WaveformData
    {
        public int SequenceNumber { get; }
        public DateTime Timestamp { get; }
        public Dictionary<SensorType, List<short>> SensorWaveforms { get; }

        public WaveformData(DateTime timestamp,
            int sequenceNumber, 
            Dictionary<SensorType, List<short>> sensorWaveforms)
        {
            Timestamp = timestamp;
            SensorWaveforms = sensorWaveforms;
            SequenceNumber = sequenceNumber;
        }

        public WaveformData(DateTime timestamp,
            int sequenceNumber, 
            params Waveform[] waveforms)
            : this(timestamp, sequenceNumber, waveforms.ToDictionary(w => w.SensorType, w => w.Values))
        {
        }
    }
}