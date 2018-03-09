using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.Mathematics;
using Commons.Physics;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class HistoryLoader
    {
        private readonly FileManager fileManager;
        private readonly AvailableDataFinder availableDataFinder;
        private readonly VitalSignFileLoader vitalSignLoader;
        private readonly WaveformFileLoader waveformLoader;

        public HistoryLoader(
            FileManager fileManager, 
            AvailableDataFinder availableDataFinder, 
            VitalSignFileLoader vitalSignLoader, 
            WaveformFileLoader waveformLoader)
        {
            this.fileManager = fileManager;
            this.availableDataFinder = availableDataFinder;
            this.vitalSignLoader = vitalSignLoader;
            this.waveformLoader = waveformLoader;
        }

        public List<Range<DateTime>> GetAvailableDataForPatient(PatientInfo patientInfo)
        {
            return availableDataFinder.FindTimePeriodsWithAvailableData(patientInfo);
        }

        public RecordedPatientData GetDataInRange(PatientInfo patientInfo, 
            Range<DateTime> timeRange,
            IReadOnlyList<SensorType> sensorTypes,
            IReadOnlyList<VitalSignType> vitalSignTypes)
        {
            var vitalParameters = GetVitalSignDataInRange(patientInfo, timeRange, sensorTypes, vitalSignTypes);
            var waveforms = GetWaveformDataInRange(patientInfo, timeRange, sensorTypes);
            return new RecordedPatientData(patientInfo, vitalParameters, waveforms);
        }

        public IReadOnlyDictionary<SensorVitalSignType, TimeSeries<short>> GetVitalSignDataInRange(
            PatientInfo patientInfo,
            Range<DateTime> timeRange,
            IReadOnlyList<SensorType> sensorTypes,
            IReadOnlyList<VitalSignType> vitalSignTypes)
        {
            if(timeRange == null)
                throw new ArgumentNullException(nameof(timeRange));
            var patientDirectory = fileManager.GetPatientDirectory(patientInfo);
            var matchingDateDirectories = Directory.GetDirectories(patientDirectory)
                .Where(dir => IsDateDirectoryInRange(dir, timeRange))
                .OrderBy(dir => dir);
            var vitalParameters = new Dictionary<SensorVitalSignType, TimeSeries<short>>();
            foreach (var dateDirectory in matchingDateDirectories)
            {
                try
                {
                    var directoryVitalParameters = vitalSignLoader.Load(dateDirectory, timeRange, sensorTypes, vitalSignTypes);
                    foreach (var sensorVitalSignType in directoryVitalParameters.Keys)
                    {
                        if (!vitalParameters.ContainsKey(sensorVitalSignType))
                            vitalParameters.Add(sensorVitalSignType, new TimeSeries<short>());
                        var newData = directoryVitalParameters[sensorVitalSignType];
                        vitalParameters[sensorVitalSignType].AddRange(newData);
                    }
                }
                catch { }
            }
            return vitalParameters;
        }

        public IReadOnlyDictionary<SensorType, TimeSeries<short>> GetWaveformDataInRange(
            PatientInfo patientInfo,
            Range<DateTime> timeRange,
            IReadOnlyList<SensorType> sensorTypes)
        {
            var patientDirectory = fileManager.GetPatientDirectory(patientInfo);
            var matchingDateDirectories = Directory.GetDirectories(patientDirectory)
                .Where(dir => IsDateDirectoryInRange(dir, timeRange))
                .OrderBy(dir => dir);
            var waveforms = new Dictionary<SensorType, TimeSeries<short>>();
            foreach (var dateDirectory in matchingDateDirectories)
            {
                try
                {
                    var directoryWaveforms = waveformLoader.Load(dateDirectory, timeRange, sensorTypes);
                    foreach (var sensorType in directoryWaveforms.Keys)
                    {
                        if (!waveforms.ContainsKey(sensorType))
                            waveforms.Add(sensorType, new TimeSeries<short>());
                        var newData = directoryWaveforms[sensorType];
                        waveforms[sensorType].AddRange(newData);
                    }
                }
                catch { }
            }
            return waveforms;
        }

        private static bool IsDateDirectoryInRange(string directory, Range<DateTime> timeRange)
        {
            var directoryInfo = new DirectoryInfo(directory);
            if (!DateTime.TryParse(directoryInfo.Name, out var directoryTime))
                return false;
            var directoryTimeRange = new Range<DateTime>(directoryTime, directoryTime.AddDays(1));
            return directoryTimeRange.Overlaps(timeRange);
        }
    }
}
