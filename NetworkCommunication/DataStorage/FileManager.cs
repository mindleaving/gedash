using System;
using System.IO;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class FileManager
    {
        public const string VitalSignFilePrefix = "GEDash_vitalSigns";
        public const string WaveformFilePrefix = "GEDash_waveforms_";
        public const string FileExtension = "csv";
        public const char Delimiter = ';';
        public const string TimestampColumnName = "Timestamp";

        private readonly string baseDirectory;

        public FileManager(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
        }

        public string GetVitalSignFileName()
        {
            return $@"{VitalSignFilePrefix}.{FileExtension}";
        }

        public string GetWaveformFileName(SensorType sensorType)
        {
            return $@"{WaveformFilePrefix}{sensorType}.{FileExtension}";
        }

        public string GetDatedPatientDirectory(DateTime timestamp, PatientInfo patientInfo)
        {
            var dateDirectory = Path.Combine(
                GetPatientDirectory(patientInfo),
                timestamp.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(dateDirectory))
                Directory.CreateDirectory(dateDirectory);
            return dateDirectory;
        }

        public string GetPatientDirectory(PatientInfo patientInfo)
        {
            return Path.Combine(
                baseDirectory,
                $"{patientInfo.LastName}_{patientInfo.FirstName}");
        }
    }
}
