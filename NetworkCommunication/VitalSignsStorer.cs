using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetworkCommunication
{
    public class VitalSignsStorer : IDisposable
    {
        const string FileExtension = "csv";
        const char Delimiter = ';';
        const string TimestampColumnName = "Timestamp";
        TextWriter writer;
        readonly string directory;
        readonly bool appendToFile;
        bool isInitialized;
        readonly Dictionary<string, int> columnIndices = new Dictionary<string, int>();

        public VitalSignsStorer(string directory, bool append)
        {
            this.directory = directory;
            appendToFile = append;

            BuildColumnIndexLookup();
        }

        void BuildColumnIndexLookup()
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

        public void Initialize()
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            writer = new StreamWriter(Path.Combine(directory, $@"GEDash_vitalSigns.{FileExtension}"), appendToFile);
            var header = columnIndices.OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .Aggregate((a, b) => a + Delimiter + b);
            writer.WriteLine(header);
            isInitialized = true;
        }

        public void Store(VitalSignData vitalSignData)
        {
            if(!isInitialized)
                throw new InvalidOperationException($"{nameof(WaveformStorer)} not initialized");

            var columns = new string[columnIndices.Count];
            columns[columnIndices[TimestampColumnName]] = vitalSignData.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            foreach (var vitalSignValue in vitalSignData.VitalSignValues)
            {
                var columnIdx = GetColumnIndex(vitalSignValue.SensorType, vitalSignValue.VitalSignType);
                if(columnIdx < 0)
                    continue;
                columns[columnIdx] = vitalSignValue.Value.ToString();
            }
            writer.WriteLine(columns.Aggregate((a,b) => a + Delimiter + b));
        }

        int GetColumnIndex(SensorType sensorType, VitalSignType vitalSignType)
        {
            var columnKey = BuildColumnKey(sensorType, vitalSignType);
            if (!columnIndices.ContainsKey(columnKey))
                return -1;
            return columnIndices[columnKey];
        }

        static string BuildColumnKey(SensorType sensorType, VitalSignType vitalSignType)
        {
            return $"{sensorType}_{vitalSignType}";
        }

        public void Dispose()
        {
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }
    }
}