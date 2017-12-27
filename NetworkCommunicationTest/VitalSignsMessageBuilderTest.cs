using System.Collections.Generic;
using System.Net;
using NetworkCommunication;
using NetworkCommunication.Simulators;
using NUnit.Framework;

namespace NetworkCommunicationTest
{
    [TestFixture]
    public class VitalSignsMessageBuilderTest
    {
        static readonly IList<SensorType> sensorTypes = new List<SensorType>
        {
            SensorType.SpO2,
            SensorType.EcgLeadI,
            SensorType.EcgLeadII,
            SensorType.EcgLeadIII,
            SensorType.EcgLeadPrecordial
        };
        static readonly IPAddress ipAddress = IPAddress.Parse("192.168.1.194");

        [Test]
        public void MessageLengthAsExpected()
        {
            var sut = CreateDefault();
            var message = sut.Build();
            Assert.That(message.Length, Is.EqualTo(546));
        }

        static Dictionary<SensorType, ISimulator> CreateSimulators()
        {
            var simulationSettings = new SimulationSettings();
            return new Dictionary<SensorType, ISimulator>
            {
                { SensorType.EcgLeadI, new EcgSimulator(SensorType.EcgLeadI, simulationSettings) },
                { SensorType.EcgLeadII, new EcgSimulator(SensorType.EcgLeadII, simulationSettings) },
                { SensorType.EcgLeadIII, new EcgSimulator(SensorType.EcgLeadIII, simulationSettings) },
                { SensorType.EcgLeadPrecordial, new EcgSimulator(SensorType.EcgLeadPrecordial, simulationSettings) },
                { SensorType.RespirationRate, new RespirationSimulator(simulationSettings) },
                { SensorType.SpO2, new SpO2Simulator(simulationSettings) }
            };
        }

        public static VitalSignMessageBuilder CreateDefault()
        {
            return new VitalSignMessageBuilder(sensorTypes, CreateSimulators(), ipAddress);
        }
    }
}
