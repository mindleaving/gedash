using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformStorer : IDisposable
    {
        private readonly Dictionary<PatientInfo, WaveformWriter> writers = new Dictionary<PatientInfo, WaveformWriter>();
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

        public void Store(WaveformData waveformData)
        {
            lock (writeLock)
            {
                if(isDisposed)
                    return;
                var timestamp = waveformData.Timestamp;
                var secondsSince1990 = SecondsSince1990(timestamp);
                if(!monitorDatabase.Monitors.ContainsKey(waveformData.IPAddress))
                    return;
                var monitor = monitorDatabase.Monitors[waveformData.IPAddress];
                var patientInfo = monitor.PatientInfo;
                var dateDirectory = fileManager.GetDatedPatientDirectory(timestamp, monitor.PatientInfo);
                if(!writers.ContainsKey(patientInfo))
                    writers.Add(patientInfo, new WaveformWriter(fileManager, append));

                foreach (var sensorType in waveformData.SensorWaveforms.Keys.Where(Informations.IsWaveformSensorType)) // Excludes Raw
                {
                    var sensorValues = waveformData.SensorWaveforms[sensorType];
                    var timePerSample = Informations.SensorTypeSampleTime(sensorType);
                    var lines = new List<string>();
                    for (var valueIdx = 0; valueIdx < sensorValues.Count; valueIdx++)
                    {
                        var sensorValue = sensorValues[valueIdx];
                        var time = secondsSince1990 + valueIdx * timePerSample.TotalSeconds;
                        var timeString = time.ToString("F3", CultureInfo.InvariantCulture);
                        var line = $"{timeString}{FileManager.Delimiter}{sensorValue}";
                        lines.Add(line);
                    }

                    var writer = writers[patientInfo].GetWriter(sensorType, dateDirectory);
                    writer.WriteLine(lines.Aggregate((a,b) => a + Environment.NewLine + b));
                }
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
