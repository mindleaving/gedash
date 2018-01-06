using System;
using System.Collections.Generic;
using System.IO;
using Commons.Mathematics;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class AvailableDataFinder
    {
        private readonly TimeSpan newRangeThreshold;
        private readonly FileManager fileManager;

        public AvailableDataFinder(FileManager fileManager, TimeSpan newRangeThreshold)
        {
            this.fileManager = fileManager;
            this.newRangeThreshold = newRangeThreshold;
        }

        public List<Range<DateTime>> FindTimePeriodsWithAvailableData(PatientInfo patientInfo)
        {
            var patientDirectory = fileManager.GetPatientDirectory(patientInfo);
            var dateDirectories = Directory.EnumerateDirectories(patientDirectory);
            var availableDataTimeRanges = new List<Range<DateTime>>();
            foreach (var dateDirectory in dateDirectories)
            {
                var vitalSignFile = Path.Combine(dateDirectory, fileManager.GetVitalSignFileName());
                using (var streamReader = new StreamReader(vitalSignFile))
                {
                    var headerLine = streamReader.ReadLine();
                    if(headerLine == null)
                        continue;
                    if(!headerLine.StartsWith(FileManager.TimestampColumnName))
                        throw new FormatException($"Timestamp column in vital sign file '{vitalSignFile}' not present as first column");
                    string line;
                    var rangeStart = DateTime.MinValue;
                    var lastTimestamp = DateTime.MinValue;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var splittedLine = line.Split(FileManager.Delimiter);
                        if(!DateTime.TryParse(splittedLine[0], out var timestamp))
                            continue;
                        if (timestamp - lastTimestamp > newRangeThreshold)
                        {
                            if (rangeStart != DateTime.MinValue)
                            {
                                var timeRange = new Range<DateTime>(rangeStart, lastTimestamp);
                                availableDataTimeRanges.Add(timeRange);
                            }
                            rangeStart = timestamp;
                        }
                        lastTimestamp = timestamp;
                    }
                    if (rangeStart != DateTime.MinValue)
                    {
                        var timeRange = new Range<DateTime>(rangeStart, lastTimestamp);
                        availableDataTimeRanges.Add(timeRange);
                    }
                }
            }
            return availableDataTimeRanges;
        }
    }
}
