using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;

namespace HealforceEcgFileDecoder
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: HealforceEcgFileDecoder <ECG DAT-file>");
                Console.WriteLine("or     HealforceEcgFileDecoder <directory>");
                return;
            }

            if (args[0].EndsWith(".dat"))
            {
                var ecgFile = args[0];
                var outputFile = ecgFile.Replace(".dat", ".csv");
                var decodingResult = DecodeEcgFile(ecgFile);
                WriteOutput(outputFile, decodingResult);
            }
            else
            {
                var ecgFiles = Directory.EnumerateFiles(args[0], "*.dat", SearchOption.TopDirectoryOnly);
                foreach (var ecgFile in ecgFiles)
                {
                    var outputFile = ecgFile.Replace(".dat", ".csv");
                    var decodingResult = DecodeEcgFile(ecgFile);
                    WriteOutput(outputFile, decodingResult);
                }
            }
        }

        private static HealforceEcgData DecodeEcgFile(string ecgFile)
        {
            const double SamplesPerSecond = 150;
            var startTime = DateTime.MinValue;

            var data = File.ReadAllBytes(ecgFile);
            var leadCount = data[0x1ff];
            var dataIdx = 0;
            var waitingForHeader = true;
            var leadData = new Dictionary<byte, TimeSeries<short>>();
            var leadId = (byte)0;
            var sampleIdx = 0;
            while (dataIdx < data.Length)
            {
                var valueByte = data[dataIdx];
                var idByte = data[dataIdx + 1];
                dataIdx += 2;

                if (idByte == 0xff)
                {
                    waitingForHeader = true;
                    continue;
                }

                if (waitingForHeader)
                {
                    dataIdx += 0x200;
                    waitingForHeader = false;
                }
                if(!idByte.InSet(new byte[] {0x01,0x02,0x81,0x82}))
                    continue;
                var value = idByte == 1 ? (short)(valueByte - 0xff)
                    : idByte == 2 ? valueByte
                    : idByte == 0x81 ? (short)(valueByte - 0xff) // Heart beat detected/synchronization?
                    : idByte == 0x82 ? (valueByte) // Heart beat detected/synchronization?
                    : throw new ArgumentOutOfRangeException();
                if(!leadData.ContainsKey(leadId))
                    leadData.Add(leadId, new TimeSeries<short>());
                var time = startTime.AddSeconds(sampleIdx / SamplesPerSecond);
                var timePoint = new TimePoint<short>(time, value);
                leadData[leadId].Add(timePoint);
                leadId++;
                if (leadId >= leadCount)
                {
                    leadId = 0;
                    sampleIdx++;
                }
            }
            return new HealforceEcgData(leadData);
        }

        private static void WriteOutput(string outputFile, HealforceEcgData decodingResult)
        {
            var lines = new List<string>();
            var maxLeadDataLength = decodingResult.LeadData.Max(kvp => kvp.Value.Count);
            var maxDataLead = decodingResult.LeadData.First(kvp => kvp.Value.Count == maxLeadDataLength).Key;
            var startTime = decodingResult.LeadData[maxDataLead].First().Time;

            var leadIds = decodingResult.LeadData.Keys.ToList();
            lines.Add("Time;" + leadIds.Select(x => $"Lead {x}").Aggregate((a,b) => $"{a};{b}"));
            for (int lineIdx = 0; lineIdx < maxLeadDataLength; lineIdx++)
            {
                var time = (decodingResult.LeadData[maxDataLead][lineIdx].Time - startTime).TotalSeconds;
                var line = $"{time:F3}";
                foreach (var leadId in leadIds)
                {
                    var values = decodingResult.LeadData[leadId];
                    if(lineIdx >= values.Count)
                    {
                        line += ";";
                        continue;
                    }
                    line += $";{values[lineIdx].Value}";
                }
                lines.Add(line);
            }
            File.WriteAllLines(outputFile, lines);
        }
    }

    public class HealforceEcgData
    {
        public Dictionary<byte, TimeSeries<short>> LeadData { get; }

        public HealforceEcgData(Dictionary<byte, TimeSeries<short>> leadData)
        {
            LeadData = leadData;
        }
    }
}
