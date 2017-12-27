using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NetworkCommunication
{
    public static class VitalSignPacketParser
    {
        const int SensorEntryLength = 70;

        const int SensorIdentifierOffset = 1;
        const int SensorIdentifieryLength = 4;

        const int FirstValueOffset = 6;
        const int ValueLength = 2;

        const int FirstAlarmLimitOffset = 32;
        const int AlarmLimitLength = 4;

        public static VitalSignData Parse(byte[] buffer, DateTime timestamp)
        {
            var ipAddress = new IPAddress(buffer.Skip(0).Take(4).ToArray());
            var counter = BitConverter.ToInt32(buffer.Skip(75).Take(4).Reverse().ToArray(), 0);
            var entryPointIndices = GetEntryStartIndices(buffer.Length); //ParserHelpers.FindIndicesOfValue(buffer, entryByte, startIndex);
            var vitalSignValues = new List<VitalSignValue>();
            foreach (var entryPointIndex in entryPointIndices)
            {
                var sectionBytes = buffer.Skip(entryPointIndex).Take(SensorEntryLength).ToArray();
                var sensorCodeBytes = sectionBytes.Skip(SensorIdentifierOffset).Take(SensorIdentifieryLength);
                var sensorCode = BitConverter.ToUInt32(sensorCodeBytes.Reverse().ToArray(), 0);
                var sensorType = Informations.MapVitalSignSenorCodeToSensorType(sensorCode);
                switch (sensorType)
                {
                    case SensorType.Ecg:
                        vitalSignValues.AddRange(ParseEcgSection(sectionBytes));
                        break;
                    case SensorType.EcgLeadI:
                    case SensorType.EcgLeadII:
                    case SensorType.EcgLeadIII:
                    case SensorType.EcgLeadPrecordial:
                    case SensorType.RespirationRate:
                    case SensorType.SpO2:
                    case SensorType.BloodPressure:
                        vitalSignValues.AddRange(ParseGeneric(sectionBytes, sensorType));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            return new VitalSignData(timestamp)
            {
                IPAddress = ipAddress,
                MessageCounter = counter,
                VitalSignValues = vitalSignValues
            };
        }

        static IEnumerable<int> GetEntryStartIndices(int bufferLength)
        {
            var idx = 124;
            while (idx+SensorEntryLength < bufferLength)
            {
                yield return idx;
                idx += SensorEntryLength;
            }
        }

        static IEnumerable<VitalSignValue> ParseEcgSection(byte[] sectionBytes)
        {
            const int ValueCount = 10;

            var lowerLimitBytes = sectionBytes.Skip(FirstAlarmLimitOffset).Take(2);
            var lowerLimitUshort = BitConverter.ToUInt16(lowerLimitBytes.Reverse().ToArray(), 0);
            var lowerLimit = ParserHelpers.ToShortValue(lowerLimitUshort);
            var upperLimitBytes = sectionBytes.Skip(FirstAlarmLimitOffset+2).Take(2);
            var upperLimitUshort = BitConverter.ToUInt16(upperLimitBytes.Reverse().ToArray(), 0);
            var upperLimit = ParserHelpers.ToShortValue(upperLimitUshort);

            for (int valueIdx = 0; valueIdx < ValueCount; valueIdx++)
            {
                var offset = FirstValueOffset + ValueLength * valueIdx;
                var valueBytes = sectionBytes.Skip(offset).Take(ValueLength);
                if(valueBytes.First() == 0x80)
                    continue;
                var valueUShort = BitConverter.ToUInt16(valueBytes.Reverse().ToArray(), 0);
                var value = ParserHelpers.ToShortValue(valueUShort);
                if(value < 5 || value > 300)
                    continue;
                yield return new VitalSignValue(
                    SensorType.Ecg, 
                    VitalSignType.HeartRate, 
                    value,
                    lowerLimit,
                    upperLimit);
            }
        }

        static IEnumerable<VitalSignValue> ParseGeneric(byte[] sectionBytes, SensorType sensorType)
        {
            var vitalSigns = Informations.VitalSignTypesForSensor(sensorType);
            var valueCount = vitalSigns.Count;
            for (int valueIdx = 0; valueIdx < valueCount; valueIdx++)
            {
                var vitalSignType = vitalSigns[valueIdx];
                var valueBytes = sectionBytes.Skip(FirstValueOffset + ValueLength * valueIdx).Take(ValueLength);
                if(valueBytes.First() == 0x80)
                    continue;
                var value = ParserHelpers.ToShortValue(BitConverter.ToUInt16(valueBytes.Reverse().ToArray(), 0));
                if(value < 0)
                    continue;
                var lowerLimitBytes = sectionBytes.Skip(FirstAlarmLimitOffset + AlarmLimitLength * valueIdx).Take(2);
                var lowerLimit = ParserHelpers.ToShortValue(BitConverter.ToUInt16(lowerLimitBytes.Reverse().ToArray(), 0));
                var upperLimitBytes = sectionBytes.Skip(FirstAlarmLimitOffset + AlarmLimitLength * valueIdx + 2).Take(2);
                var upperLimit = ParserHelpers.ToShortValue(BitConverter.ToUInt16(upperLimitBytes.Reverse().ToArray(), 0));
                yield return new VitalSignValue(sensorType, vitalSignType, value, lowerLimit, upperLimit);
            }
        }
    }
}
