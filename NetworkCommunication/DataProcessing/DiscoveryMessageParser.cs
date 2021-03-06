﻿using System;
using System.Linq;
using System.Net;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataProcessing
{
    public class DiscoveryMessageParser
    {
        public const char WardBedSeparator = '|';

        public DiscoveryMessage Parse(byte[] data, DateTime timestamp)
        {
            var ipAddress = new IPAddress(data.Skip(4).Take(4).ToArray());
            var counter = BitConverter.ToUInt32(data.Skip(8).Take(4).Reverse().ToArray(), 0);
            var wardName = new string(data
                .Skip(12)
                .TakeWhile(b => b != WardBedSeparator)
                .Select(b => (char)b)
                .Where(IsPrintableCharacter)
                .ToArray());
            var bedName = new string(data
                .Skip(12+wardName.Length+1)
                .TakeWhile(b => b != 0x00)
                .Select(b => (char)b)
                .Where(IsPrintableCharacter)
                .ToArray());
            var lastName = new string(data
                .Skip(28)
                .Select(b => (char)b)
                .TakeWhile(c => c != ',')
                .Where(IsPrintableCharacter)
                .ToArray());
            if (string.IsNullOrWhiteSpace(lastName))
                lastName = "Name";
            var firstName = new string(data
                .Skip(28 + lastName.Length + 1)
                .TakeWhile(b => b != 0x00)
                .Select(b => (char) b)
                .Where(IsPrintableCharacter)
                .ToArray());
            if (string.IsNullOrWhiteSpace(firstName))
                firstName = "Unknown";
            var patientInfo = new PatientInfo(firstName, lastName);
            return new DiscoveryMessage(
                ipAddress,
                counter,
                wardName, 
                bedName, 
                patientInfo, 
                timestamp);
        }

        private bool IsPrintableCharacter(char c)
        {
            return char.IsLetter(c) || c.InSet('-', '.', ' ');
        }
    }
}
