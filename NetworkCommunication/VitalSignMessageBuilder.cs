using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetworkCommunication.Simulators;

namespace NetworkCommunication
{
    public class VitalSignMessageBuilder
    {
        readonly IPAddress ourIpAddress;
        readonly IList<SensorType> sensorTypes;
        readonly Dictionary<SensorType, ISimulator> simulators;
        uint sequenceNumber = 1;

        public VitalSignMessageBuilder(
            IList<SensorType> sensorTypes,
            Dictionary<SensorType, ISimulator> simulators, 
            IPAddress ourIpAddress)
        {
            if(!sensorTypes.All(simulators.ContainsKey))
                throw new ArgumentException("Not simulators for all sensor types");
            this.sensorTypes = sensorTypes;
            this.simulators = simulators;
            this.ourIpAddress = ourIpAddress;
        }

        public byte[] Build()
        {
            var sequenceNumberBytes = BitConverter.GetBytes(sequenceNumber).Reverse().ToArray();
            var counterLastByte = sequenceNumberBytes.Last();

            var ipAddressBytes = ourIpAddress.GetAddressBytes();
            var ipAddressSection1 = ipAddressBytes
                .Concat(new byte[2])
                .Concat(ipAddressBytes)
                .Concat(new byte[2])
                .Concat(new byte[] { 0x00, 0xc9, 0x00, 0x14, 0x00, 0x01 })
                .Concat(new byte[40])
                .Concat(new byte[] { 0x01, 0xe6 });
            var ipAddressSection2 = ipAddressBytes
                .Concat(new byte[2])
                .Concat(ipAddressBytes)
                .Concat(new byte[2])
                .Concat(new byte[] { 0x00, 0xc9, 0x00, 0x14, 0x00, 0x01 })
                .Concat(new byte[40])
                .Concat(new byte[] { 0x01, 0xaa });
            var ipSectionSuffix = new byte[] {0x00, 0x06, 0x06, 0x01, (byte)(sensorTypes.Count+1) }; // +1 for ECG section
            var combinedSensorBytes = new List<byte>();
            var ecgHeartrateSection = BuildEcgHeartrateSection();
            foreach (var sensorType in sensorTypes)
            {
                var dataSource = GetDataSource(sensorType);
                var sensorVitalSigns = dataSource.GetVitalSignValues();
                var sensorCodeBytes = GetSensorCodeBytes(sensorType);
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
                        alarmLimitBytes.AddRange(new byte[4]);
                    }
                }

                var settingsBytes = GetSettingsBytes(sensorType, sensorCodeBytes);
                var sensorBytes = new[] {counterLastByte}
                    .Concat(sensorCodeBytes)
                    .Concat(new []{dataSource.QualityByte})
                    .Concat(valueBytes)
                    .Concat(new byte[14])
                    .Concat(settingsBytes)
                    .Concat(new byte[] { 0x00, 0x00 })
                    .Concat(alarmLimitBytes)
                    .Concat(new byte[22])
                    .Concat(new byte[] { 0x0b, sensorCodeBytes[2], 0x00, 0x00 })
                    .ToArray();
                combinedSensorBytes.AddRange(sensorBytes);
            }

            var message = ipAddressSection1
                .Concat(ipAddressSection2)
                .Concat(ipSectionSuffix)
                .Concat(ecgHeartrateSection)
                .Concat(combinedSensorBytes)
                .Concat(new byte[] {counterLastByte, 0x00 })
                .ToArray();
            sequenceNumber++;
            return message;
        }

        static byte[] GetSettingsBytes(SensorType sensorType, byte[] sensorCodeBytes)
        {
            switch (sensorType)
            {
                case SensorType.RespirationRate:
                    var ecgLead = EcgLead.II;
                    var ecgLeadByte = GetRespirationLeadIndicator(ecgLead);
                    return new byte[]{ 0x03, sensorCodeBytes[2], 0x86, ecgLeadByte };
                default:
                    return new byte[] { 0x03, sensorCodeBytes[2], 0x00, 0x00 };
            }
        }

        byte[] BuildEcgHeartrateSection()
        {
            var sensorCodeBytes = GetSensorCodeBytes(SensorType.Ecg);
            var validityByte = (byte) 0x08;
            var combinedHeartrateBytes = new List<byte>();
            var ecgSimulator = simulators.Values.OfType<EcgSimulator>().FirstOrDefault();
            if (ecgSimulator != null)
            {
                var heartRateUshort = ParserHelpers.ToUShort(ecgSimulator.Heartrate);
                var heartrateBytes = BitConverter.GetBytes(heartRateUshort).Reverse();
                combinedHeartrateBytes.AddRange(heartrateBytes);
                var vesUshort = ParserHelpers.ToUShort(ecgSimulator.VentricularExtraSystoles);
                var vesBytes = BitConverter.GetBytes(vesUshort).Reverse();
                combinedHeartrateBytes.AddRange(vesBytes);
                combinedHeartrateBytes.AddRange(new byte[]{ 0x00, 0x00 });
                combinedHeartrateBytes.AddRange(new byte[] { 0x0c, 0x3a });
                combinedHeartrateBytes.AddRange(Enumerable.Repeat((byte)0x80, 12));
            }
            else
            {
                combinedHeartrateBytes.AddRange(new byte[] { 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x0c, 0x3a });
                combinedHeartrateBytes.AddRange(Enumerable.Repeat((byte)0x80, 12));
            }

            // Alarm limit
            var firstEcgSimulator = simulators.Values.OfType<EcgSimulator>().FirstOrDefault();
            var lowerLimit = firstEcgSimulator?.HeartrateLowerLimit ?? 50;
            var lowerLimitBytes = BitConverter.GetBytes(ParserHelpers.ToUShort(lowerLimit)).Reverse();
            var upperLimit = firstEcgSimulator?.HeartRateUpperLimit?? 50;
            var upperLimitBytes = BitConverter.GetBytes(ParserHelpers.ToUShort(upperLimit)).Reverse();
            var alarmLimitBytes = lowerLimitBytes.Concat(upperLimitBytes);

            var ecgHeartrateSection = sensorCodeBytes
                .Concat(new []{validityByte})
                .Concat(combinedHeartrateBytes)
                .Concat(new byte[] {0x03, sensorCodeBytes[2], 0x00, 0x00})
                .Concat(new byte[] {0x00, 0x01})
                .Concat(alarmLimitBytes)
                .Concat(new byte[] {0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00})
                .Concat(new byte[] {0x15, sensorCodeBytes[2], 0x00, 0x00})
                .Concat(new byte[] {0x40, 0x21, 0x40, 0x00, 0x00, 0x00})
                .Concat(new byte[] {0x02, 0x3a, 0x20, 0x85})
                .Concat(new byte[] {0x00, 0x00, 0x00, 0x09, 0x00, 0x00})
                .Concat(new byte[] {0x01, sensorCodeBytes[2], 0x01, 0x00})
                .ToArray();
            return ecgHeartrateSection;
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
            return simulators[sensorType];
        }
    }
}
