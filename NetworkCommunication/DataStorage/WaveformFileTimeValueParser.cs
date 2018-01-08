using System;
using System.Globalization;

namespace NetworkCommunication.DataStorage
{
    public class WaveformFileTimeValueParser
    {
        private readonly char delimiter;
        private readonly int valueColumnIndex;

        public WaveformFileTimeValueParser(char delimiter, int valueColumnIndex = 0)
        {
            this.delimiter = delimiter;
            this.valueColumnIndex = valueColumnIndex;
        }

        public double Parse(string line)
        {
            var splittedLine = line.Split(delimiter);
            if(splittedLine.Length <= valueColumnIndex)
                throw new FormatException("Line didn't contain value column");
            return double.Parse(splittedLine[valueColumnIndex], CultureInfo.InvariantCulture);
        }
    }
}