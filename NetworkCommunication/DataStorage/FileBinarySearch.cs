using System;
using System.Collections.Generic;
using System.IO;

namespace NetworkCommunication.DataStorage
{
    public class FileBinarySearch
    {
        private readonly int minimumLineLength;

        public FileBinarySearch(int minimumLineLength)
        {
            this.minimumLineLength = minimumLineLength;
        }

        /// <summary>
        /// Searches a file with oredered entries (e.g. log files with timestamps) for a pattern
        /// </summary>
        /// <returns>Position in stream of first line matching</returns>
        public long FindFirstLineMatching(Stream stream, 
            string searchObject, 
            long startPosition,
            IComparer<string> lineComparer)
        {
            var endPosition = stream.Length;
            var currentPosition = (startPosition + endPosition) / 2;
            var firstLine = GetFirstLine(stream, startPosition);
            if (firstLine == null)
                return -1;
            var firstLineComparison = lineComparer.Compare(firstLine, searchObject);
            var isFirstLineAfterSearchObject = firstLineComparison > 0;
            if (isFirstLineAfterSearchObject)
                return -1;
            var isFirstLineEqual = firstLineComparison == 0;
            if (isFirstLineEqual)
                return 0;
            //var lastLine = GetLastLine(stream);
            //var lastLineComparison = lineComparer.Compare(lastLine, searchObject);
            while (startPosition < endPosition)
            {
                var lineWithPosition = GetNextFullLineAfterPosition(stream, currentPosition);
                if (string.IsNullOrEmpty(lineWithPosition.Line))
                {
                    endPosition = currentPosition-minimumLineLength;
                    currentPosition = (startPosition + endPosition) / 2;
                    continue;
                }
                if (lineWithPosition.Position > endPosition) // Corner case: Because GetNextFullLineAfterPosition skips a line, we might end after the current end position
                {
                    // When we get here, we are close to the desired position, use linear search(ish)
                    return PerformLinearSearch(stream, searchObject, startPosition, endPosition, lineComparer);
                }
                var comparisonResult = lineComparer.Compare(lineWithPosition.Line, searchObject);
                var isLineEarlierThanSearchObject = comparisonResult < 0;
                if (isLineEarlierThanSearchObject)
                    startPosition = lineWithPosition.Position;
                else if (comparisonResult == 0) // Line is equal to search object
                    return lineWithPosition.Position;
                else
                    endPosition = lineWithPosition.Position - 1;

                currentPosition = (startPosition + endPosition) / 2;
            }
            return startPosition;
        }

        private long PerformLinearSearch(
            Stream stream, 
            string searchObject, 
            long startPosition, 
            long endPosition, 
            IComparer<string> lineComparer)
        {
            do
            {
                // Test if line after start position is before or after search object
                var lineWithPosition = GetNextFullLineAfterPosition(stream, startPosition);
                var comparisonResult = lineComparer.Compare(lineWithPosition.Line, searchObject);
                if (comparisonResult == 0)
                    return lineWithPosition.Position;
                var isLineBeforeStartPosition = comparisonResult < 0;
                if (isLineBeforeStartPosition)
                    startPosition = lineWithPosition.Position;
                else
                    return startPosition;
            } while (startPosition < endPosition);
            return startPosition;
        }

        private string GetFirstLine(Stream stream, long startPosition)
        {
            stream.Seek(startPosition, SeekOrigin.Begin);
            var reader = new StreamReader(stream);
            string line;
            do
            {
                line = reader.ReadLine();
            } while (string.IsNullOrEmpty(line));
            return line;
        }

        private LineWithPosition GetNextFullLineAfterPosition(Stream stream, long position)
        {
            stream.Seek(position, SeekOrigin.Begin);
            var reader = new StreamReader(stream);
            var discardedLine = reader.ReadLine(); // Discard, because it is probably not complete
            if(discardedLine == null)
                return new LineWithPosition(null, position);
            var newPosition = position + discardedLine.Length;
            do
            {
                newPosition++;
                stream.Seek(newPosition, SeekOrigin.Begin);
                discardedLine = reader.ReadLine();
            } while (string.IsNullOrEmpty(discardedLine) && newPosition+1 < stream.Length);
            var fullLine = discardedLine;
            return new LineWithPosition(fullLine, newPosition);
        }

        private class LineWithPosition
        {
            public LineWithPosition(string line, long position)
            {
                Line = line;
                Position = position;
            }

            public string Line { get; }
            public long Position { get; }
        }
    }
}
