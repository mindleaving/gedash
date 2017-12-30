using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetworkCommunication.Objects;
using NetworkCommunication.Simulators;

namespace NetworkCommunication.DataProcessing
{
    public class VitalSignMessageBuilder
    {
        readonly IPAddress ourIpAddress;
        readonly IList<SensorType> sensorTypes;
        readonly Dictionary<SensorType, ISimulator> simulators;
        uint sequenceNumber = 0x00010001;

        public VitalSignMessageBuilder(
            IList<SensorType> sensorTypes,
            Dictionary<SensorType, ISimulator> simulators, 
            IPAddress ourIpAddress)
        {
            if(!sensorTypes.All(simulators.ContainsKey))
                throw new ArgumentException("Not simulators for all sensor types");
            this.sensorTypes = sensorTypes
                .Concat(new []{ SensorType.Ecg })
                .OrderBy(GetSensorOrder)
                .ToList();
            this.simulators = simulators;
            this.ourIpAddress = ourIpAddress;
        }

        int GetSensorOrder(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.Ecg:
                    return 0;
                case SensorType.EcgLeadI:
                    return 101;
                case SensorType.EcgLeadII:
                    return 102;
                case SensorType.EcgLeadIII:
                    return 103;
                case SensorType.EcgLeadPrecordial:
                    return 104;
                case SensorType.Respiration:
                    return 1;
                case SensorType.SpO2:
                    return 2;
                case SensorType.BloodPressure:
                    return 3;
                default:
                    return 999;
            }
        }

        public byte[] Build()
        {
            var sequenceNumberBytes = BitConverter.GetBytes(sequenceNumber).Reverse().ToArray();
            var counterLastByte = sequenceNumberBytes.Last();

            var ipSectionSuffix = new byte[] {0x00, 0x06, 0x06, 0x01, (byte)sensorTypes.Count };
            var combinedSensorBytes = new List<byte>();
            foreach (var sensorType in sensorTypes)
            {
                var dataSource = GetDataSource(sensorType);
                var sensorVitalSigns = dataSource is EcgSimulator && sensorType != SensorType.Ecg
                    ? new List<VitalSignValue>()
                    : dataSource.GetVitalSignValues();
                var sensorCodeBytes = GetSensorCodeBytes(sensorType);
                var qualityByte = dataSource is EcgSimulator && sensorType != SensorType.Ecg
                    ? (byte)0x00
                    : dataSource.QualityByte;
                var valueBytes = new List<byte>();
                var alarmLimitBytes = new List<byte>();
                for (int valueIdx = 0; valueIdx < 3; valueIdx++)
                {
                    if (valueIdx < sensorVitalSigns.Count)
                    {
                        var vitalSignValue = sensorVitalSigns[valueIdx];
                        var transformedValue = ParserHelpers.ToUShort(vitalSignValue.Value);
                        var transformedLowerLimit = ParserHelpers.ToUShort(vitalSignValue.LowerLimit);
                        var transformedUpperLimit = ParserHelpers.ToUShort(vitalSignValue.UpperLimit);
                        valueBytes.AddRange(BitConverter.GetBytes(transformedValue).Reverse());
                        alarmLimitBytes.AddRange(BitConverter.GetBytes(transformedLowerLimit).Reverse());
                        alarmLimitBytes.AddRange(BitConverter.GetBytes(transformedUpperLimit).Reverse());
                    }
                    else
                    {
                        valueBytes.AddRange(new byte[] { 0x80, 0x00 });
                        if(dataSource is EcgSimulator && sensorType != SensorType.Ecg)
                            alarmLimitBytes.AddRange(new byte[] { 0xff, 0xec, 0x00, 0x14});
                        else
                            alarmLimitBytes.AddRange(new byte[4]);
                    }
                }

                var unknownValueBytes = GetUnknownValueBytes(sensorType, sensorCodeBytes);
                var sensorConfig1Bytes = GetSensorConfig1Bytes(sensorType, sensorCodeBytes);
                var sensorConfig2Bytes = GetSensorConfig2Bytes(sensorType, sensorCodeBytes);
                var sensorConfig3Bytes = GetSensorConfig3Bytes(sensorType, sensorCodeBytes);
                var sensorPriorityByte = GetSensorPriorityByte(sensorType);
                var sensorBytes = sensorCodeBytes
                    .Concat(new []{qualityByte})
                    .Concat(valueBytes)
                    .Concat(unknownValueBytes)
                    .Concat(sensorConfig1Bytes)
                    .Concat(alarmLimitBytes)
                    .Concat(new byte[2])
                    .Concat(sensorConfig2Bytes)
                    .Concat(sensorConfig3Bytes)
                    .Concat(new byte[] { sensorPriorityByte, sensorCodeBytes[2], 0x00, 0x00 })
                    .Concat(new[] {counterLastByte})
                    .ToArray();
                combinedSensorBytes.AddRange(sensorBytes);
            }

            var ipAddressBytes = ourIpAddress.GetAddressBytes();
            var ipSection2MessageLength = (ushort) (combinedSensorBytes.Count - 64);
            var ipSection2MessageLengthBytes = BitConverter.GetBytes(ipSection2MessageLength).Reverse();
            var ipAddressSection2 = ipAddressBytes
                .Concat(new byte[2])
                .Concat(ipAddressBytes)
                .Concat(new byte[2])
                .Concat(new byte[] { 0x00, 0xc9, 0x00, 0x14 })
                .Concat(sequenceNumberBytes)
                .Concat(new byte[38])
                .Concat(ipSection2MessageLengthBytes)
                .ToList();
            var ipSection1MessageLength = (ushort)(combinedSensorBytes.Count - 64 + ipAddressSection2.Count);
            var ipSection1MessageLengthBytes = BitConverter.GetBytes(ipSection1MessageLength).Reverse();
            var ipAddressSection1 = ipAddressBytes
                .Concat(new byte[2])
                .Concat(ipAddressBytes)
                .Concat(new byte[2])
                .Concat(new byte[] { 0x00, 0xc9, 0x00, 0x14, 0x00, 0x01 })
                .Concat(new byte[40])
                .Concat(ipSection1MessageLengthBytes)
                .ToList();

            var message = ipAddressSection1
                .Concat(ipAddressSection2)
                .Concat(ipSectionSuffix)
                .Concat(combinedSensorBytes)
                .Concat(new byte[] { 0x00 })
                .ToArray();
            sequenceNumber++;
            return message;
        }

        byte[] GetUnknownValueBytes(SensorType sensorType, byte[] sensorCodeBytes)
        {
            byte categoryByte = 0x0c;
            switch (sensorType)
            {
                case SensorType.Ecg:
                    return new byte[] { categoryByte, sensorCodeBytes[2], 0x80, 0x80 }
                        .Concat(Enumerable.Repeat((byte)0x80, 10))
                        .ToArray();
                case SensorType.BloodPressure:
                    return new byte[] { categoryByte, sensorCodeBytes[2], 0x80, 0x01 }
                        .Concat(new byte[10])
                        .ToArray();
                default:
                    return new byte[14];
            }
        }

        static byte[] GetSensorConfig1Bytes(SensorType sensorType, byte[] sensorCodeBytes)
        {
            byte categoryByte = 0x03;
            switch (sensorType)
            {
                case SensorType.Ecg:
                    var selectedLead = EcgLead.II;
                    var selectedLeadCode = GetEcgLeadByte(selectedLead);
                    return new byte[] {categoryByte, sensorCodeBytes[2], 0x10, 0x80, 0x00, selectedLeadCode};
                case SensorType.SpO2:
                    return new byte[] {categoryByte, sensorCodeBytes[2], 0xa0, 0x20, 0x00, 0x09};
                case SensorType.Respiration:
                    var ecgLead = EcgLead.II;
                    var ecgLeadByte = GetRespirationLeadIndicator(ecgLead);
                    return new byte[]{ categoryByte, sensorCodeBytes[2], 0x86, ecgLeadByte, 0x00, 0x00 };
                case SensorType.BloodPressure:
                    return new byte[] { categoryByte, sensorCodeBytes[2], 0x00, 0x20, 0x00, 0x0f};
                default:
                    return new byte[] { categoryByte, sensorCodeBytes[2], 0x00, 0x00, 0x00, 0x00 };
            }
        }

        static byte[] GetSensorConfig2Bytes(SensorType sensorType, byte[] sensorCodeBytes)
        {
            byte categoryByte = 0x15;
            switch (sensorType)
            {
                case SensorType.Ecg:
                    return new byte[] {categoryByte, sensorCodeBytes[2], 0x00, 0x00}
                        .Concat(new byte[] {0x40, 0x21, 0x40, 0x00, 0x00, 0x00})
                        .ToArray();
                case SensorType.Respiration:
                case SensorType.SpO2:
                case SensorType.BloodPressure:
                    return new byte[] {categoryByte, sensorCodeBytes[2], 0x40, 0x00}
                        .Concat(new byte[] {0x40, 0x00, 0x40, 0x01, 0x3f, 0xea})
                        .ToArray();
                default:
                    return new byte[10];
            }
        }

        static byte[] GetSensorConfig3Bytes(SensorType sensorType, byte[] sensorCodeBytes)
        {
            byte categoryByte = 0x02;
            switch (sensorType)
            {
                case SensorType.Ecg:
                    return new byte[] {categoryByte, sensorCodeBytes[2], 0x20, 0x85}
                        .Concat(new byte[] {0x00, 0x00, 0x00, 0x09, 0x00, 0x00})
                        .ToArray();
                case SensorType.BloodPressure:
                    return new byte[]{ categoryByte, sensorCodeBytes[2], 0x00, 0x01 }
                        .Concat(new byte[6])
                        .ToArray();
                default:
                    return new byte[10];
            }
        }

        static byte GetEcgLeadByte(EcgLead lead)
        {
            switch (lead)
            {
                case EcgLead.I:
                    return 0x00;
                case EcgLead.II:
                    return 0x01;
                case EcgLead.III:
                    return 0x02;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lead), lead, null);
            }
        }

        byte GetSensorPriorityByte(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.Ecg:
                    return 0x01;
                case SensorType.EcgLeadI:
                case SensorType.EcgLeadII:
                case SensorType.EcgLeadIII:
                case SensorType.EcgLeadPrecordial:
                    return 0x0d;
                case SensorType.Respiration:
                    return 0x08;
                case SensorType.SpO2:
                    return 0x0b;
                case SensorType.BloodPressure:
                    return 0x0a;
                default:
                    return 0x0e;
            }
        }

        static byte GetRespirationLeadIndicator(EcgLead lead)
        {
            switch (lead)
            {
                case EcgLead.I:
                    return 0x24;
                case EcgLead.II:
                    return 0x64;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lead), lead, null);
            }
        }

        static byte[] GetSensorCodeBytes(SensorType sensorType)
        {
            var sensorCode = Informations.SensorVitalSignCode[sensorType];
            var sensorCodeBytes = BitConverter.GetBytes(sensorCode).Reverse().ToArray();
            return sensorCodeBytes;
        }

        ISimulator GetDataSource(SensorType sensorType)
        {
            if (sensorType == SensorType.Ecg)
                return simulators.Values.OfType<EcgSimulator>().First();
            return simulators[sensorType];
        }
    }
}
