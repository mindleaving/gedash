using System;
using System.Collections.Generic;

namespace NetworkCommunication.DataStorage
{
    public class TimestampedLineComparer : IComparer<string>
    {
        private readonly Func<string, DateTime> timestampParser;

        public TimestampedLineComparer(Func<string, DateTime> timestampParser)
        {
            this.timestampParser = timestampParser;
        }

        public int Compare(string line1, string line2)
        {
            var timestamp1 = timestampParser(line1);
            var timestamp2 = timestampParser(line2);
            return timestamp1.CompareTo(timestamp2);
        }
    }
}