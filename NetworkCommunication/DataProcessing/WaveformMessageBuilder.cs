using System;
using System.Collections.Generic;
using System.Linq;
using NetworkCommunication.Objects;
using NetworkCommunication.Simulators;

namespace NetworkCommunication.DataProcessing
{
    public class WaveformMessageBuilder
    {
        private readonly IList<SensorType> sensorTypes;
        private readonly Dictionary<SensorType, ISimulator> simulators;
        private ushort sequenceNumber = 1;

        public WaveformMessageBuilder(
            IList<SensorType> sensorTypes, 
            Dictionary<SensorType, ISimulator> simulators)
        {
            if(!sensorTypes.All(simulators.ContainsKey))
                throw new ArgumentException("Not simulators for all sensor types");

            this.sensorTypes = sensorTypes
                .Where(Informations.IsWaveformSensorType)
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

        private ISimulator GetDataSource(SensorType sensorType)
        {
            return simulators[sensorType];
        }

        private int GetSensorTypeOrder(SensorType sensorType)
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
                case SensorType.Respiration:
                    return 5;
                case SensorType.SpO2:
                    return 6;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sensorType), sensorType, null);
            }
        }
    }
}
