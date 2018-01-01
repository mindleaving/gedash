using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformStorer : IDisposable
    {
        bool isInitialized;
        private const string FileExtension = "csv";
        private const char Delimiter = ';';
        private const string TimestampColumnName = "Timestamp";
        private readonly Dictionary<SensorType, TextWriter> writers = new Dictionary<SensorType, TextWriter>();
        private readonly string directory;
        private readonly bool appendToFile;

        public WaveformStorer(string directory, bool append)
        {
            this.directory = directory;
            appendToFile = append;
        }

        public void Initialize()
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            isInitialized = true;
        }

        public void Store(WaveformData waveformData)
        {
            if(!isInitialized)
                throw new InvalidOperationException($"{nameof(WaveformStorer)} not initialized");
            var timestamp = waveformData.Timestamp;
            var secondsSince1990 = SecondsSince1990(timestamp);
            foreach (var sensorType in waveformData.SensorWaveforms.Keys)
            {
                var sensorValues = waveformData.SensorWaveforms[sensorType];
                var timePerSample = Informations.SensorTypeSampleTime(sensorType);
                var lines = new List<string>();
                for (var valueIdx = 0; valueIdx < sensorValues.Count; valueIdx++)
                {
                    var sensorValue = sensorValues[valueIdx];
                    var time = secondsSince1990 + valueIdx * timePerSample.TotalSeconds;
                    var line = $"{time:F3}{Delimiter}{sensorValue}";
                    lines.Add(line);
                }

                if(!writers.ContainsKey(sensorType))
                    CreateWriter(sensorType);
                writers[sensorType].WriteLine(lines.Aggregate((a,b) => a + Environment.NewLine + b));
            }
        }

        static readonly DateTime Year1990 = new DateTime(1990, 1, 1, 0, 0, 0);
        static double SecondsSince1990(DateTime timestamp)
        {
            return (timestamp - Year1990).TotalSeconds;
        }

        void CreateWriter(SensorType sensorType)
        {
            var writer = new StreamWriter(Path.Combine(directory, $@"GEDash_waveforms_{sensorType}.{FileExtension}"),
                appendToFile);
            writers.Add(sensorType, writer);
            var header = $"{TimestampColumnName}{Delimiter}{sensorType}";
            writer.WriteLine(header);
        }

        public void Dispose()
        {
            foreach (var writer in writers.Values)
            {
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
        }
    }
}
