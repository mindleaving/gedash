﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataProcessing
{
    public class VitalSignPacketParser
    {
        private const int SensorEntryLength = 70;

        private const int SensorIdentifierOffset = 1;
        private const int SensorIdentifieryLength = 4;

        private const int FirstValueOffset = 6;
        private const int ValueLength = 2;

        private const int FirstAlarmLimitOffset = 32;
        private const int AlarmLimitLength = 4;

        public VitalSignData Parse(byte[] buffer, DateTime timestamp)
        {
            var ipAddress = new IPAddress(buffer.Skip(0).Take(4).ToArray());
            var counter = BitConverter.ToInt32(buffer.Skip(75).Take(4).Reverse().ToArray(), 0);
            var entryPointIndices = GetEntryStartIndices(buffer.Length); //ParserHelpers.FindIndicesOfValue(buffer, entryByte, startIndex);
            var vitalSignValues = new List<VitalSignValue>();
            foreach (var entryPointIndex in entryPointIndices)
            {
                var sectionBytes = buffer.Skip(entryPointIndex).Take(SensorEntryLength).ToArray();
                var sensorCodeByte = sectionBytes[SensorIdentifierOffset+2];
                try
                {
                    var sensorType = Informations.MapVitalSignSenorCodeToSensorType(sensorCodeByte);
                    vitalSignValues.AddRange(ParseSensorSection(sectionBytes, sensorType));
                }
                catch (ArgumentOutOfRangeException)
                {
                    continue; // Ignore unknown sensors
                }
            }
            return new VitalSignData(ipAddress, counter, vitalSignValues, timestamp);
        }

        private static IEnumerable<int> GetEntryStartIndices(int bufferLength)
        {
            var idx = 124;
            while (idx+SensorEntryLength < bufferLength)
            {
                yield return idx;
                idx += SensorEntryLength;
            }
        }

        private static IEnumerable<VitalSignValue> ParseSensorSection(byte[] sectionBytes, SensorType sensorType)
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
