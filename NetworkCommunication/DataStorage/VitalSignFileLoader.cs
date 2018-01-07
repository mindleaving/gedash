using System;
using System.Collections.Generic;
using System.IO;
using Commons.Mathematics;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class VitalSignFileLoader
    {
        private readonly FileManager fileManager;

        public VitalSignFileLoader(FileManager fileManager)
        {
            this.fileManager = fileManager;
        }

        public Dictionary<SensorVitalSignType, TimeSeries<short>> Load(
            string directory, 
            Range<DateTime> timeRange, 
            IReadOnlyList<SensorType> sensorTypes, 
            IReadOnlyList<VitalSignType> vitalSignTypes)
        {
            var vitalSignFile = Path.Combine(directory, fileManager.GetVitalSignFileName());
            var timeSeries = new Dictionary<SensorVitalSignType, TimeSeries<short>>();
            var sensorTypeHashSet = new HashSet<SensorType>(sensorTypes);
            var vitalSignTypeHashSet = new HashSet<VitalSignType>(vitalSignTypes);
            using (var fileStream = new FileStream(vitalSignFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream))
            {
                var headerLine = streamReader.ReadLine();
                if (headerLine == null)
                    return timeSeries;

                // Header parsing
                var header = ParseHeader(headerLine);

                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    var splittedLine = line.Split(FileManager.Delimiter);
                    if(splittedLine.Length != header.Count+1) // +1 for timestamp column
                        continue;
                    var timestamp = DateTime.Parse(splittedLine[0]);
                    if(!timeRange.Contains(timestamp))
                        continue;
                    for (int columnIdx = 1; columnIdx < splittedLine.Length; columnIdx++)
                    {
                        var senorVitalSignKey = header[columnIdx];
                        if(!sensorTypeHashSet.Contains(senorVitalSignKey.SensorType))
                            continue;
                        if(!vitalSignTypeHashSet.Contains(senorVitalSignKey.VitalSignType))
                            continue;

                        var valueString = splittedLine[columnIdx];
                        if(!short.TryParse(valueString, out var value))
                            continue;
                        var timePoint = new TimePoint<short>(timestamp, value);
                        if(!timeSeries.ContainsKey(senorVitalSignKey))
                            timeSeries.Add(senorVitalSignKey, new TimeSeries<short>());
                        timeSeries[senorVitalSignKey].Add(timePoint);
                    }
                }
            }
            return timeSeries;
        }

        private Dictionary<int, SensorVitalSignType> ParseHeader(string headerLine)
        {
            var splittedLine = headerLine.Split(FileManager.Delimiter);
            if(splittedLine[0] != FileManager.TimestampColumnName)
                throw new FormatException("Timestamp column missing");
            var header = new Dictionary<int, SensorVitalSignType>();
            for (int columnIdx = 1; columnIdx < splittedLine.Length; columnIdx++)
            {
                var columnName = splittedLine[columnIdx];
                var splittedColumnName = columnName.Split('_');
                if(splittedColumnName.Length != 2)
                    throw new FormatException();
                var sensorType = (SensorType) Enum.Parse(typeof(SensorType), splittedColumnName[0]);
                var vitalSignType = (VitalSignType) Enum.Parse(typeof(VitalSignType), splittedColumnName[1]);
                header.Add(columnIdx, new SensorVitalSignType(sensorType, vitalSignType));
            }
            return header;
        }
    }
}