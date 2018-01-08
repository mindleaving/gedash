using System.IO;
using System.Text;
using NetworkCommunication.DataStorage;
using NUnit.Framework;

namespace NetworkCommunicationTest.DataStorage
{
    [TestFixture]
    public class FileBinarySearchTest
    {
        private readonly int minimumLineLength = 5;
        private readonly TimestampedLineComparer lineComparer;
        private readonly string testFile = "2017-12-30 22:47:49;1;0;0;17;;;;;\r\n" +
                                           "2017-12-30 22:47:51;2;0;0;16;;;;;\r\n" +
                                           "2017-12-30 22:47:53;3;0;0;18;;;;;\r\n" +
                                           "2017-12-30 22:47:55;4;0;0;21;;;;;\r\n" +
                                           "2017-12-30 22:47:57;5;0;0;21;;;;;\r\n" +
                                           "2017-12-30 22:47:59;6;0;0;19;;;;;\r\n" +
                                           "2017-12-30 22:48:01;7;0;0;19;;;;;";

        public FileBinarySearchTest()
        {
            var timestampParser = new TimestampedLineParser(';', 0);
            lineComparer = new TimestampedLineComparer(timestampParser.Parse);
        }

        [Test]
        public void MinusOneReturnedForEmptyStream()
        {
            var stream = new MemoryStream();
            var sut = new FileBinarySearch(minimumLineLength);
            var searchObject = "2017-12-30 22:47:53;;;;;;;";
            var actual = sut.FindFirstLineMatching(stream, searchObject, 0, lineComparer);
            Assert.That(actual, Is.LessThan(0));
        }

        [Test]
        public void CorrectStreamPositionFoundForMatchingDateTime()
        {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(testFile));
            var sut = new FileBinarySearch(minimumLineLength);
            var searchObject = "2017-12-30 22:47:53;;;;;;;";
            var actual = sut.FindFirstLineMatching(stream, searchObject, 0, lineComparer);
            Assert.That(actual, Is.EqualTo(70));
        }

        [Test]
        public void LastLineEarlierThanDateTimeIsFound()
        {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(testFile));
            var sut = new FileBinarySearch(minimumLineLength);
            var searchObject = "2017-12-30 22:47:54;;;;;;;";
            var actual = sut.FindFirstLineMatching(stream, searchObject, 0, lineComparer);
            Assert.That(actual, Is.EqualTo(70));
        }
    }
}
