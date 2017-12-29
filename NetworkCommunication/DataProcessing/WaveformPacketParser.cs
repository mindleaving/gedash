using System;
using System.Collections.Generic;
using System.Linq;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataProcessing
{
    public static class WaveformPacketParser
    {
        public static WaveformData Parse(byte[] buffer, DateTime timestamp)
        {
            var shortValues = new List<short>();
            for (int idx = 0; idx+1 < buffer.Length; idx += 2)
            {
                var bytes = buffer.Skip(idx).Take(2).Reverse().ToArray();
                var ushortValue = BitConverter.ToUInt16(bytes, 0);
                var shortValue = ParserHelpers.ToShortValue(ushortValue);
                shortValues.Add(shortValue);
            }

            var sequenceNumber = shortValues[0];
            var waveforms = new Dictionary<SensorType, List<short>> {{SensorType.Raw, shortValues}};

            var valueIdx = 2;
            while (valueIdx < shortValues.Count)
            {
                SensorType sensorType;
                try
                {
                    sensorType = Informations.MapWaveformCodeToSensorType(shortValues[valueIdx]);
                }
                catch (ArgumentOutOfRangeException)
                {
                    break;
                }
                var valueCount = Informations.SensorBatchSizes[sensorType];
                if(!waveforms.ContainsKey(sensorType))
                    waveforms.Add(sensorType, new List<short>());
                waveforms[sensorType].AddRange(shortValues.Skip(valueIdx+1).Take(valueCount));
                valueIdx += valueCount + 1;
            }
            if(shortValues[valueIdx] > 0)
                waveforms.Add(SensorType.Other, shortValues.Skip(valueIdx).ToList());
            return new WaveformData(timestamp, sequenceNumber, waveforms);
        }
    }
}
