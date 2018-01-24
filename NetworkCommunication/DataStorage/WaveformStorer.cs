using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformStorer : IDisposable
    {
        private readonly Dictionary<PatientInfo, PatientWaveformWriterCollection> writers = new Dictionary<PatientInfo, PatientWaveformWriterCollection>();
        private readonly IMonitorDatabase monitorDatabase;
        private readonly FileManager fileManager;
        private readonly bool append;
        private readonly object writeLock = new object();
        private volatile bool isDisposed;

        public WaveformStorer(
            IMonitorDatabase monitorDatabase,
            FileManager fileManager,
            bool append)
        {
            this.monitorDatabase = monitorDatabase;
            this.fileManager = fileManager;
            this.append = append;
        }

        public void Store(WaveformCollection waveformCollection)
        {
            lock (writeLock)
            {
                if(isDisposed)
                    return;
                if(!monitorDatabase.Monitors.ContainsKey(waveformCollection.IPAddress))
                    return;
                var monitor = monitorDatabase.Monitors[waveformCollection.IPAddress];
                var patientInfo = monitor.PatientInfo;
                if(!writers.ContainsKey(patientInfo))
                    writers.Add(patientInfo, new PatientWaveformWriterCollection(fileManager, append));
                var timestamp = waveformCollection.Timestamp;
                var lastTimestamp = writers[patientInfo].LastMessageTime;
                var timeSinceLastPackage = lastTimestamp == DateTime.MinValue
                    ? TimeSpan.FromSeconds(1.0 / 4)
                    : timestamp - lastTimestamp;
                var secondsSince1990 =  SecondsSince1990(timestamp - timeSinceLastPackage);
                var dateDirectory = fileManager.GetDatedPatientDirectory(timestamp, monitor.PatientInfo);

                foreach (var sensorType in waveformCollection.SensorWaveforms.Keys.Where(Informations.IsWaveformSensorType)) // Excludes Raw
                {
                    var sensorValues = waveformCollection.SensorWaveforms[sensorType];
                    var timePerSampleInSeconds = timeSinceLastPackage.TotalSeconds / sensorValues.Count;
                    var lines = new List<string>();
                    for (var valueIdx = 0; valueIdx < sensorValues.Count; valueIdx++)
                    {
                        var sensorValue = sensorValues[valueIdx];
                        if(Math.Abs(sensorValue) > 10000)
                            continue;
                        var time = secondsSince1990 + valueIdx * timePerSampleInSeconds;
                        var timeString = time.ToString("F3", CultureInfo.InvariantCulture);
                        var line = $"{timeString}{FileManager.Delimiter}{sensorValue}";
                        lines.Add(line);
                    }

                    if (!lines.Any())
                        continue;
                    var writer = writers[patientInfo].GetWriter(sensorType, dateDirectory);
                    writer.WriteLine(lines.Aggregate((a,b) => a + Environment.NewLine + b));
                }
                writers[patientInfo].LastMessageTime = timestamp;
            }
        }

        public static readonly DateTime Year1990 = new DateTime(1990, 1, 1, 0, 0, 0);

        private static double SecondsSince1990(DateTime timestamp)
        {
            return (timestamp - Year1990).TotalSeconds;
        }

        public void Dispose()
        {
            isDisposed = true;
            lock (writeLock)
            {
                foreach (var writer in writers.Values)
                {
                    writer.Dispose();
                }
            }
        }
    }
}
