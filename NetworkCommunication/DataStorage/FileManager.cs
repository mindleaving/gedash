using System;
using System.IO;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class FileManager
    {
        private readonly string baseDirectory;

        public FileManager(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
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
