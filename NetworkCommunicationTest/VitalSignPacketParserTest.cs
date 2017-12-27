using System;
using NetworkCommunication;
using NUnit.Framework;

namespace NetworkCommunicationTest
{
    [TestFixture]
    public class VitalSignPacketParserTest
    {
        [Test]
        public void RoundtripWithMessageBuilderResultsInSameMessage()
        {
            var messageBuilder = VitalSignsMessageBuilderTest.CreateDefault();
            var message = messageBuilder.Build();
            var parseResult = VitalSignPacketParser.Parse(message, DateTime.Now);
            Assert.That(parseResult.VitalSignValues, Has.One.Matches<VitalSignValue>(
                x => x.SensorType == SensorType.SpO2 && x.VitalSignType == VitalSignType.SpO2));
            Assert.That(parseResult.VitalSignValues, Has.One.Matches<VitalSignValue>(
                x => x.SensorType == SensorType.SpO2 && x.VitalSignType == VitalSignType.HeartRate));
        }
    }
}
