using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Commons.Mathematics;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformFileLoader
    {
        private readonly FileManager fileManager;
        private readonly FileBinarySearch fileBinarySearch;
        private readonly IComparer<string> lineComparer;

        public WaveformFileLoader(FileManager fileManager)
        {
            this.fileManager = fileManager;
            var minimumLineLength = 10;
            fileBinarySearch = new FileBinarySearch(minimumLineLength);
            var waveformFileTimeValueParser = new WaveformFileTimeValueParser(FileManager.Delimiter);
            lineComparer = new ValueColumnComparer<double>(waveformFileTimeValueParser.Parse);
        }

        public Dictionary<SensorType, TimeSeries<short>> Load(
            string directory, 
            Range<DateTime> timeRange, 
            IReadOnlyList<SensorType> sensorTypes)
        {
            var timeSeries = new Dictionary<SensorType, TimeSeries<short>>();
            var timeRangeStartValue = (timeRange.From - WaveformStorer.Year1990).TotalSeconds;
            var startSearchString = $"{timeRangeStartValue}";

            foreach (var sensorType in sensorTypes)
            {
                var waveformFile = Path.Combine(directory, fileManager.GetWaveformFileName(sensorType));
                if(!File.Exists(waveformFile))
                    continue;
                timeSeries.Add(sensorType, new TimeSeries<short>());
                using (var fileStream = new FileStream(waveformFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var headerLine = streamReader.ReadLine();
                    if (headerLine == null)
                        return timeSeries;

                    var header = headerLine.Split(FileManager.Delimiter);
                    if(header[1] != sensorType.ToString())
                        throw new FormatException($"Wrong sensor type in header of '{waveformFile}'");

                    string line;
                    var searchStartPosition = headerLine.Length;
                    do
                    {
                        searchStartPosition++;
                        fileStream.Seek(searchStartPosition, SeekOrigin.Begin);
                        line = streamReader.ReadLine();
                    } while (string.IsNullOrEmpty(line));
                    var streamPosition = fileBinarySearch.FindFirstLineMatching(fileStream, startSearchString, searchStartPosition, lineComparer);
                    fileStream.Seek(streamPosition, SeekOrigin.Begin);

                    var maxTimeValue = (DateTime.Now - WaveformStorer.Year1990).TotalSeconds;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var splittedLine = line.Split(FileManager.Delimiter);
                        if (splittedLine.Length != 2)
                            continue;
                        if(!double.TryParse(splittedLine[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var secondsSince1990))
                            continue;
                        if(secondsSince1990 > maxTimeValue)
                            continue;
                        var timestamp = WaveformStorer.Year1990.AddSeconds(secondsSince1990);
                        if (timestamp < timeRange.From)
                            continue;
                        if(timestamp > timeRange.To)
                            break;
                        if(!short.TryParse(splittedLine[1], out var value))
                            continue;
                        var timePoint = new TimePoint<short>(timestamp, value);
                        timeSeries[sensorType].Add(timePoint);
                    }
                }
            }
            return timeSeries;
        }
    }
}