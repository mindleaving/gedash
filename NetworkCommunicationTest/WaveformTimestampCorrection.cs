using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Commons.Physics;
using NetworkCommunication;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;
using NUnit.Framework;

namespace NetworkCommunicationTest
{
    [TestFixture]
    public class WaveformTimestampCorrection
    {
        [Test]
        [TestCase(@"C:\Users\Jan\Desktop\GEDashData\SCHOLTYSSEK_J")]
        public void FixWaveformFiles(string dataDirectory)
        {
            FixDirectory(dataDirectory);
        }

        private void FixDirectory(string dataDirectory)
        {
            var subDirectories = Directory.GetDirectories(dataDirectory);
            foreach (var subDirectory in subDirectories)
            {
                FixDirectory(subDirectory);
            }

            var csvFiles = Directory.EnumerateFiles(dataDirectory, "GEDash_waveforms_*.csv");
            foreach (var csvFile in csvFiles)
            {
                FixFile(csvFile);
            }
        }

        private void FixFile(string csvFile)
        {
            var sensorType = ParseSensorType(Path.GetFileName(csvFile));
            var batchSize = Informations.SensorBatchSizes[sensorType];
            const int BatchesPerMessage = 15;
            var valuesPerMessage = BatchesPerMessage * batchSize;
            var backupFilename = csvFile + ".bak";
            File.Copy(csvFile, backupFilename);
            try
            {
                using (var reader = new StreamReader(backupFilename))
                using (var writer = new StreamWriter(csvFile))
                {
                    writer.WriteLine(reader.ReadLine());
                    var timeSeries = new TimeSeries<short>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var timePoint = ParseLine(line);
                        if (timeSeries.Count == valuesPerMessage)
                        {
                            var firstTimestamp = timeSeries.First().Time;
                            var nextTimestamp = timePoint.Time;
                            var sampleTime = (nextTimestamp - firstTimestamp).TotalSeconds/valuesPerMessage;
                            for (int pointIdx = 0; pointIdx < timeSeries.Count; pointIdx++)
                            {
                                var correctedTimestamp = (firstTimestamp - WaveformStorer.Year1990).TotalSeconds
                                                         + pointIdx * sampleTime;
                                writer.WriteLine($"{correctedTimestamp:F3};{timeSeries[pointIdx].Value}");
                            }
                            timeSeries.Clear();
                        }
                        timeSeries.Add(timePoint);
                    }

                    foreach (var seriesPoint in timeSeries)
                    {
                        var correctedTimestamp = (seriesPoint.Time - WaveformStorer.Year1990).TotalSeconds;
                        writer.WriteLine($"{correctedTimestamp:F3};{seriesPoint.Value}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fixup of {csvFile} failed: {e.Message}");
                File.Copy(backupFilename, csvFile);
            }
            File.Delete(backupFilename);
        }

        private static TimePoint<short> ParseLine(string line)
        {
            var splittedLine = line.Split(';');
            var secondsSince1990 = double.Parse(splittedLine[0], NumberStyles.Any, CultureInfo.InvariantCulture);
            var value = short.Parse(splittedLine[1]);
            return new TimePoint<short>(WaveformStorer.Year1990.AddSeconds(secondsSince1990), value);
        }

        private SensorType ParseSensorType(string filename)
        {
            var sensorTypeString = Path.GetFileNameWithoutExtension(filename)
                .Replace("GEDash_waveforms_", "");
            return (SensorType) Enum.Parse(typeof(SensorType), sensorTypeString);
        }
    }
}
