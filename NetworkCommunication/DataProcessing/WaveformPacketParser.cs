using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataProcessing
{
    public class WaveformPacketParser
    {
        public WaveformData Parse(byte[] data, 
            IPAddress ipAddress,
            DateTime timestamp)
        {
            var shortValues = new List<short>();
            for (int idx = 0; idx+1 < data.Length; idx += 2)
            {
                var bytes = data.Skip(idx).Take(2).Reverse().ToArray();
                var ushortValue = BitConverter.ToUInt16(bytes, 0);
                var shortValue = ParserHelpers.ToShortValue(ushortValue);
                shortValues.Add(shortValue);
            }

            var sequenceNumber = shortValues[0];
            var waveforms = new Dictionary<SensorType, List<short>> {{SensorType.Raw, shortValues}};

            var valueIdx = 2;
            while (valueIdx < shortValues.Count)
            {
                var sensorType = Informations.MapWaveformCodeToSensorType(shortValues[valueIdx]);
                if(sensorType == SensorType.Undefined)
                    break;
                var valueCount = Informations.SensorBatchSizes[sensorType];
                if(!waveforms.ContainsKey(sensorType))
                    waveforms.Add(sensorType, new List<short>());
                waveforms[sensorType].AddRange(shortValues.Skip(valueIdx+1).Take(valueCount));
                valueIdx += valueCount + 1;
            }
            if(shortValues[valueIdx] > 0)
                waveforms.Add(SensorType.Undefined, shortValues.Skip(valueIdx).ToList());
            return new WaveformData(ipAddress, timestamp, sequenceNumber, waveforms);
        }
    }
}
