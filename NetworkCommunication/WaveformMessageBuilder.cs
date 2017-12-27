using System;
using System.Collections.Generic;
using System.Linq;
using NetworkCommunication.Simulators;

namespace NetworkCommunication
{
    public class WaveformMessageBuilder
    {
        readonly IList<SensorType> sensorTypes;
        readonly Dictionary<SensorType, ISimulator> simulators;
        ushort sequenceNumber = 1;

        public WaveformMessageBuilder(
            IList<SensorType> sensorTypes, 
            Dictionary<SensorType, ISimulator> simulators)
        {
            if(!sensorTypes.All(simulators.ContainsKey))
                throw new ArgumentException("Not simulators for all sensor types");

            this.sensorTypes = sensorTypes
                .Where(IsWaveformSensorType)
                .OrderBy(GetSensorTypeOrder)
                .ToList();
            this.simulators = simulators;
        }

        public byte[] Build()
        {
            const int BatchCount = 15;
            
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(sequenceNumber).Reverse());
            bytes.AddRange(new byte[]{ 0x00, 0x00 });
            for (int batchIdx = 0; batchIdx < BatchCount; batchIdx++)
            {
                foreach (var sensorType in sensorTypes)
                {
                    var sensorCode = Informations.SensorWaveformCode[sensorType];
                    var batchSize = Informations.SensorBatchSizes[sensorType];
                    var dataSource = GetDataSource(sensorType);
                    var batch = Enumerable.Range(0, batchSize)
                        .Select(_ => dataSource.GetNextValue())
                        .Select(ParserHelpers.ToUShort)
                        .SelectMany(x => BitConverter.GetBytes(x).Reverse());
                    bytes.AddRange(BitConverter.GetBytes(sensorCode).Reverse());
                    bytes.AddRange(batch);
                }
            }
            bytes.AddRange(
                new short[]{ -1522, 10, 23, 39, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0 }
                    .SelectMany(x => BitConverter.GetBytes(x).Reverse()));
            sequenceNumber++;
            return bytes.ToArray();
        }

        ISimulator GetDataSource(SensorType sensorType)
        {
            return simulators[sensorType];
        }

        static bool IsWaveformSensorType(SensorType sensorType)
        {
            var waveformSensors = new[]
            {
                SensorType.EcgLeadI,
                SensorType.EcgLeadII,
                SensorType.EcgLeadIII,
                SensorType.EcgLeadPrecordial,
                SensorType.RespirationRate,
                SensorType.SpO2
            };
            return sensorType.InSet(waveformSensors);
        }
        int GetSensorTypeOrder(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.EcgLeadI:
                    return 1;
                case SensorType.EcgLeadII:
                    return 2;
                case SensorType.EcgLeadIII:
                    return 3;
                case SensorType.EcgLeadPrecordial:
                    return 4;
                case SensorType.RespirationRate:
                    return 5;
                case SensorType.SpO2:
                    return 6;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sensorType), sensorType, null);
            }
        }
    }
}
