﻿using System;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.Objects;
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
            var messageParser = new VitalSignPacketParser();
            var message = messageBuilder.Build();
            var parseResult = messageParser.Parse(message, DateTime.Now);
            Assert.That(parseResult.VitalSignValues, Has.One.Matches<VitalSignValue>(
                x => x.SensorType == SensorType.SpO2 && x.VitalSignType == VitalSignType.SpO2));
            Assert.That(parseResult.VitalSignValues, Has.One.Matches<VitalSignValue>(
                x => x.SensorType == SensorType.SpO2 && x.VitalSignType == VitalSignType.HeartRate));
        }
    }
}
