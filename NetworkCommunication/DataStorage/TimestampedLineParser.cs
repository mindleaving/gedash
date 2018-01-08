using System;

namespace NetworkCommunication.DataStorage
{
    public class TimestampedLineParser
    {
        private readonly char delimiter;
        private readonly int timestampColumnIndex;

        public TimestampedLineParser(char delimiter, int timestampColumnIndex)
        {
            this.delimiter = delimiter;
            this.timestampColumnIndex = timestampColumnIndex;
        }

        public DateTime Parse(string line)
        {
            var splittedLine = line.Split(delimiter);
            if(splittedLine.Length <= timestampColumnIndex)
                throw new FormatException("Line has no timestamp column");
            return DateTime.Parse(splittedLine[timestampColumnIndex]);
        }
    }
}