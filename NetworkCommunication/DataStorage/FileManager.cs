using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.Extensions;
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
                $"{patientInfo.LastName.ToUpperInvariant()}_{patientInfo.FirstName.ToUpperInvariant()}");
        }

        public IList<PatientInfo> GetAllPatients()
        {
            var directories = Directory.GetDirectories(baseDirectory)
                .Select(directory => new DirectoryInfo(directory).Name);
            var patients = new List<PatientInfo>();
            foreach (var directory in directories)
            {
                var splittedName = directory.Split('_');
                var lastName = splittedName[0].FirstLetterToUpperInvariant();
                var firstName = splittedName.Length > 1 ? splittedName[1].FirstLetterToUpperInvariant() : "";
                patients.Add(new PatientInfo(firstName, lastName));
            }

            return patients;
        }
    }
}
