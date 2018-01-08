using System;
using System.Collections.Generic;

namespace NetworkCommunication.DataStorage
{
    public class ValueColumnComparer<T> : IComparer<string> where T: IComparable<T>
    {
        private readonly Func<string, T> valueParser;

        public ValueColumnComparer(Func<string, T> valueParser)
        {
            this.valueParser = valueParser;
        }

        public int Compare(string line1, string line2)
        {
            var value1 = valueParser(line1);
            var value2 = valueParser(line2);
            return value1.CompareTo(value2);
        }
    }
}
