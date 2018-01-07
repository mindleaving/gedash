using System;
using System.Collections.Generic;
using System.IO;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class WaveformWriter : IDisposable
    {
        private readonly Dictionary<SensorType, AutoFlushingFileWriter> writers = new Dictionary<SensorType, AutoFlushingFileWriter>();
        private readonly FileManager fileManager;
        private readonly bool appendToFile;

        public WaveformWriter(FileManager fileManager, bool appendToFile)
        {
            this.fileManager = fileManager;
            this.appendToFile = appendToFile;
        }

        public TextWriter GetWriter(SensorType sensorType, string directory)
        {
            if(!writers.ContainsKey(sensorType))
                InitializeWriter(sensorType, directory);
            if(writers[sensorType].Directory != directory)
                InitializeWriter(sensorType, directory);
            return writers[sensorType].Writer;
        }

        private void InitializeWriter(SensorType sensorType, string dateDirectory)
        {
            if (writers.ContainsKey(sensorType))
            {
                writers[sensorType].Writer.Dispose();
            }

            var fileName = fileManager.GetWaveformFileName(sensorType);
            var combinedFilePath = Path.Combine(dateDirectory, fileName);
            var writeHeader = !File.Exists(combinedFilePath);
            var fileWriter = new AutoFlushingFileWriter(dateDirectory, fileName, appendToFile);
            writers[sensorType] = fileWriter;
            if (writeHeader)
            {
                var header = $"{FileManager.TimestampColumnName}{FileManager.Delimiter}{sensorType}";
                fileWriter.Writer.WriteLine(header);
            }
        }

        public void Dispose()
        {
            foreach (var writer in writers.Values)
            {
                writer.Writer.Dispose();
            }
        }
    }
}
