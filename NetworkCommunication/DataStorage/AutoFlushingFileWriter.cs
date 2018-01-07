using System;
using System.IO;

namespace NetworkCommunication.DataStorage
{
    public class AutoFlushingFileWriter : IDisposable
    {
        public AutoFlushingFileWriter(string directory, string fileName, bool append)
        {
            Directory = directory;
            FileName = fileName;
            var filePath = Path.Combine(Directory, FileName);
            Writer = new StreamWriter(filePath, append) { AutoFlush = true };

        }

        public string Directory { get; }
        public string FileName { get; }
        public TextWriter Writer { get; }

        public void Dispose()
        {
            Writer.Close();
            Writer.Dispose();
        }
    }
}