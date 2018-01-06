using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Commons.Mathematics;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformFileLoader
    {
        private readonly FileManager fileManager;

        public WaveformFileLoader(FileManager fileManager)
        {
            this.fileManager = fileManager;
        }

        public Dictionary<SensorType, TimeSeries<short>> Load(
            string directory, 
            Range<DateTime> timeRange, 
            IReadOnlyList<SensorType> sensorTypes)
        {
            var timeSeries = sensorTypes.ToDictionary(sensorType => sensorType, _ => new TimeSeries<short>());

            foreach (var sensorType in sensorTypes)
            {
                var waveformFile = Path.Combine(directory, fileManager.GetWaveformFileName(sensorType));
                using (var streamReader = new StreamReader(waveformFile))
                {
                    var headerLine = streamReader.ReadLine();
                    if (headerLine == null)
                        return timeSeries;

                    var header = headerLine.Split(FileManager.Delimiter);
                    if(header[1] != sensorType.ToString())
                        throw new FormatException($"Wrong sensor type in header of '{waveformFile}'");

                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var splittedLine = line.Split(FileManager.Delimiter);
                        if (splittedLine.Length != 2)
                            continue;
                        if(!double.TryParse(splittedLine[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var secondsSince1990))
                            continue;
                        var timestamp = WaveformStorer.Year1990.AddSeconds(secondsSince1990);
                        if(!timeRange.Contains(timestamp))
                            continue;
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