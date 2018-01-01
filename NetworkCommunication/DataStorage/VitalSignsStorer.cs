using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class VitalSignsStorer : IDisposable
    {
        private const string FileExtension = "csv";
        private const char Delimiter = ';';
        private const string FilePrefix = "GEDash_vitalSigns";
        private const string TimestampColumnName = "Timestamp";
        private DatedFileWriter writer;
        private readonly IMonitorDatabase monitorDatabase;
        private readonly FileManager fileManager;
        private readonly bool appendToFile;
        private readonly Dictionary<string, int> columnIndices = new Dictionary<string, int>();

        public VitalSignsStorer(
            IMonitorDatabase monitorDatabase, 
            FileManager fileManager, 
            bool append)
        {
            this.monitorDatabase = monitorDatabase;
            this.fileManager = fileManager;
            appendToFile = append;

            BuildColumnIndexLookup();
        }

        private void BuildColumnIndexLookup()
        {
            var columnIdx = 0;
            columnIndices.Add(TimestampColumnName, columnIdx);
            columnIdx++;
            foreach (SensorType sensorType in Enum.GetValues(typeof(SensorType)))
            {
                var vitalSignTypes = Informations.VitalSignTypesForSensor(sensorType);
                foreach (var vitalSignType in vitalSignTypes)
                {
                    var columnKey = BuildColumnKey(sensorType, vitalSignType);
                    if (columnIndices.ContainsKey(columnKey))
                        continue;
                    columnIndices.Add(columnKey, columnIdx);
                    columnIdx++;
                }
            }
        }

        public void Store(VitalSignData vitalSignData)
        {
            if (!monitorDatabase.Monitors.ContainsKey(vitalSignData.IPAddress))
                return;
            var monitor = monitorDatabase.Monitors[vitalSignData.IPAddress];
            var dateDirectory = fileManager.GetDatedPatientDirectory(vitalSignData.Timestamp, monitor.PatientInfo);
            if(writer == null || writer.Directory != dateDirectory)
                InitializeWriter(dateDirectory);

            var columns = new string[columnIndices.Count];
            columns[columnIndices[TimestampColumnName]] = vitalSignData.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            foreach (var vitalSignValue in vitalSignData.VitalSignValues)
            {
                var columnIdx = GetColumnIndex(vitalSignValue.SensorType, vitalSignValue.VitalSignType);
                if(columnIdx < 0)
                    continue;
                columns[columnIdx] = vitalSignValue.Value.ToString();
            }
            writer.Writer.WriteLine(columns.Aggregate((a,b) => a + Delimiter + b));
        }

        private int GetColumnIndex(SensorType sensorType, VitalSignType vitalSignType)
        {
            var columnKey = BuildColumnKey(sensorType, vitalSignType);
            if (!columnIndices.ContainsKey(columnKey))
                return -1;
            return columnIndices[columnKey];
        }

        private static string BuildColumnKey(SensorType sensorType, VitalSignType vitalSignType)
        {
            return $"{sensorType}_{vitalSignType}";
        }

        private void InitializeWriter(string dateDirectory)
        {
            if (writer != null)
            {
                writer.Writer.Flush();
                writer.Writer.Close();
                writer.Writer.Dispose();
            }

            var fileName = $@"{FilePrefix}.{FileExtension}";
            var combinedFilePath = Path.Combine(dateDirectory, fileName);
            var writeHeader = !File.Exists(combinedFilePath);
            writer = new DatedFileWriter(dateDirectory, fileName, appendToFile);
            if (writeHeader)
            {
                var header = columnIndices.OrderBy(kvp => kvp.Value)
                    .Select(kvp => kvp.Key)
                    .Aggregate((a, b) => a + Delimiter + b);
                writer.Writer.WriteLine(header);
            }
        }

        public void Dispose()
        {
            writer.Writer.Flush();
            writer.Writer.Close();
            writer.Writer.Dispose();
        }
    }
}