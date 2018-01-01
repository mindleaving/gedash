using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformStorer : IDisposable
    {
        private const string FileExtension = "csv";
        private const char Delimiter = ';';
        private const string FilePrefix = "GEDash_waveforms_";
        private const string TimestampColumnName = "Timestamp";
        private readonly Dictionary<SensorType, DatedFileWriter> writers = new Dictionary<SensorType, DatedFileWriter>();
        private readonly IMonitorDatabase monitorDatabase;
        private readonly FileManager fileManager;
        private readonly bool appendToFile;

        public WaveformStorer(
            IMonitorDatabase monitorDatabase,
            FileManager fileManager,
            bool append)
        {
            this.monitorDatabase = monitorDatabase;
            this.fileManager = fileManager;
            appendToFile = append;
        }

        public void Store(WaveformData waveformData)
        {
            var timestamp = waveformData.Timestamp;
            var secondsSince1990 = SecondsSince1990(timestamp);
            if(!monitorDatabase.Monitors.ContainsKey(waveformData.IPAddress))
                return;
            var monitor = monitorDatabase.Monitors[waveformData.IPAddress];
            var dateDirectory = fileManager.GetDatedPatientDirectory(timestamp, monitor.PatientInfo);
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

                if(!writers.ContainsKey(sensorType) || writers[sensorType].Directory != dateDirectory)
                    InitializeWriter(sensorType, dateDirectory);
                writers[sensorType].Writer.WriteLine(lines.Aggregate((a,b) => a + Environment.NewLine + b));
            }
        }

        private static readonly DateTime Year1990 = new DateTime(1990, 1, 1, 0, 0, 0);

        private static double SecondsSince1990(DateTime timestamp)
        {
            return (timestamp - Year1990).TotalSeconds;
        }

        private void InitializeWriter(SensorType sensorType, string dateDirectory)
        {
            if(writers.ContainsKey(sensorType))
            {
                writers[sensorType].Writer.Flush();
                writers[sensorType].Writer.Close();
                writers[sensorType].Writer.Dispose();
            }

            var fileName = $@"{FilePrefix}{sensorType}.{FileExtension}";
            var combinedFilePath = Path.Combine(dateDirectory, fileName);
            var writeHeader = !File.Exists(combinedFilePath);
            var writer = new StreamWriter(combinedFilePath, appendToFile);
            var fileWriter = new DatedFileWriter(dateDirectory, fileName, appendToFile);
            writers[sensorType] = fileWriter;
            if(writeHeader)
            {
                var header = $"{TimestampColumnName}{Delimiter}{sensorType}";
                writer.WriteLine(header);
            }
        }

        public void Dispose()
        {
            foreach (var writer in writers.Values)
            {
                writer.Writer.Flush();
                writer.Writer.Close();
                writer.Writer.Dispose();
            }
        }
    }
}
