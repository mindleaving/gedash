using System.Collections.Generic;

namespace NetworkCommunication
{
    public static class ParserHelpers
    {
        public static short ToShortValue(ushort ushortValue)
        {
            if (ushortValue > ushort.MaxValue / 2)
                return (short) (ushortValue - ushort.MaxValue);
            return (short) ushortValue;
        }

        public static ushort ToUShort(short s)
        {
            if (s < 0)
                return (ushort) (ushort.MaxValue + s);
            return (ushort) s;
        }

        public static IEnumerable<int> FindIndicesOfValue(byte[] buffer, byte searchValue, int startIndex)
        {
            var idx = startIndex;
            while (idx < buffer.Length)
            {
                if (buffer[idx] == searchValue)
                    yield return idx;
                idx++;
            }
        }
    }
}
