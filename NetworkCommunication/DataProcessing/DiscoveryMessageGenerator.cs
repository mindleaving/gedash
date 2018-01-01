using System;
using System.Linq;
using System.Net;
using System.Text;

namespace NetworkCommunication.DataProcessing
{
    public class DiscoveryData
    {
        public DiscoveryData(IPAddress ourIpAddress)
        {
            IPAddress = ourIpAddress.GetAddressBytes();
        }
        public byte[] FirstBytes { get; set; } = new byte[] {0x01, 0x04, 0x00, 0x00};
        public byte[] IPAddress { get; set; }
        public int Counter => 0x5a40cdc0 + (int) (DateTime.Now - new DateTime(2017, 12, 25, 10, 7, 30)).TotalSeconds;
        public string WardName { get; set; } = "HOME";
        public string BedName { get; set; } = "B-02";
        public string LastName { get; set; } = "SCHILDISSEK";
        public string FirstName { get; set; } = "J";
        public byte[] TrailingData { get; set; } = new byte[] { 
            0x00, 0x03, 0x07, 0xd0,  0x00, 0x11, 0x00, 0x20, 
            0x00, 0x09, 0x07, 0xd0,  0x00, 0x13, 0x0b, 0xb8, 
            0x00, 0x1c, 0x0b, 0xb9,  0x00, 0x0d, 0x07, 0xd0,
            0x00, 0x0c, 0x07, 0xd0 };
        public int TotalBytes { get; set; } = 88;
    }
    public class DiscoveryMessageGenerator
    {
        public byte[] GenerateDiscoveryPayload(DiscoveryData data)
        {
            var wardBedBytes = Encoding.ASCII.GetBytes($"{data.WardName}|{data.BedName}");
            wardBedBytes = wardBedBytes.Concat(new byte[16 - wardBedBytes.Length]).ToArray();
            var patientInfoBytes = Encoding.ASCII.GetBytes($"{data.LastName},{data.FirstName}");
            patientInfoBytes = patientInfoBytes.Concat(new byte[15-patientInfoBytes.Length]).Concat(new byte[]{0x07}).ToArray();
            var message = data.FirstBytes
                .Concat(data.IPAddress)
                .Concat(BitConverter.GetBytes(data.Counter).Reverse())
                .Concat(wardBedBytes)
                .Concat(patientInfoBytes)
                .Concat(data.TrailingData)
                .ToArray();
            var zeroPadding = new byte[data.TotalBytes-message.Length];
            message = message.Concat(zeroPadding).ToArray();
            return message;
        }
    }
}
