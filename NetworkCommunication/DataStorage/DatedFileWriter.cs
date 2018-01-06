using System;
using System.IO;

namespace NetworkCommunication.DataStorage
{
    public class DatedFileWriter : IDisposable
    {
        public DatedFileWriter(string directory, string fileName, bool append)
        {
            Directory = directory;
            FileName = fileName;
            var filePath = Path.Combine(Directory, FileName);
            Writer = new StreamWriter(filePath, append);
        }

        public string Directory { get; }
        public string FileName { get; }
        public TextWriter Writer { get; }

        public void Dispose()
        {
            Writer.Flush();
            Writer.Close();
            Writer.Dispose();
        }
    }
}