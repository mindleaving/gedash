using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NetworkCommunication.Communicators;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.Objects;
using NetworkCommunication.Simulators;

namespace NetworkCommunication
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: NetworkCommunication <IP address>");
                return;
            }

            var ourIpAddress = IPAddress.Parse("192.168.1.33");
            var broadcastAddress = IPAddress.Parse("192.168.1.255");
            var simulationSettings = new SimulationSettings();
            var simulators = new Dictionary<SensorType, ISimulator>
            {
                { SensorType.EcgLeadI, new EcgSimulator(SensorType.EcgLeadI, simulationSettings) },
                { SensorType.EcgLeadII, new EcgSimulator(SensorType.EcgLeadII, simulationSettings) },
                { SensorType.EcgLeadIII, new EcgSimulator(SensorType.EcgLeadIII, simulationSettings) },
                { SensorType.EcgLeadPrecordial, new EcgSimulator(SensorType.EcgLeadPrecordial, simulationSettings) },
                { SensorType.Respiration, new RespirationSimulator(simulationSettings) },
                { SensorType.SpO2, new SpO2Simulator(simulationSettings) },
                { SensorType.BloodPressure, new NibpSimualtor(simulationSettings)}
            };
            var simulatedSensors = new[]
            {
                SensorType.Respiration,
                SensorType.SpO2,
                SensorType.BloodPressure,
                SensorType.EcgLeadI,
                SensorType.EcgLeadII,
                SensorType.EcgLeadIII,
                SensorType.EcgLeadPrecordial,
            };

            var cancellationTokenSource = new CancellationTokenSource();
            var discoveryMessageParser = new DiscoveryMessageParser();
            var discoveryMessageReceiver = new DiscoveryMessageReceiver(discoveryMessageParser);
            var discoveryMessageSender = new DiscoveryMessageSender(ourIpAddress, broadcastAddress);
            var alarmMessageParser = new AlarmMessageParser();
            var alarmReceiver = new AlarmReceiver(alarmMessageParser);
            var dataRequestGenerator = new DataRequestGenerator();
            var vitalSignsUdpClient = new UdpClient(Informations.VitalSignsRequestOutboundPort);
            var waveformUdpClient = new UdpClient(Informations.WaveformRequestOutboundPort);
            var dataRequestSender = new DataRequestSender(dataRequestGenerator, vitalSignsUdpClient, waveformUdpClient);
            var waveformMessageBuilder = new WaveformMessageBuilder(simulatedSensors, simulators);
            var vitalSignMessageBuilder = new VitalSignMessageBuilder(simulatedSensors, simulators, ourIpAddress);
            var waveformMessageParser = new WaveformPacketParser();
            var vitalSignMessageParser = new VitalSignPacketParser();

            using(var vitalSignStreamer = new VitalSignDataStreamer(vitalSignMessageBuilder))
            using(var waveformStreamer = new WaveformStreamer(waveformMessageBuilder))
            {
                var dataRequestReceiver = new DataRequestReceiver(waveformStreamer, vitalSignStreamer);
                var waveformReceiver = new WaveformAndVitalSignReceiver(vitalSignMessageParser, waveformMessageParser, vitalSignsUdpClient, waveformUdpClient);

                discoveryMessageSender.StartSending(cancellationTokenSource.Token);
                discoveryMessageReceiver.StartReceiving(cancellationTokenSource.Token);
                alarmReceiver.StartReceiving(cancellationTokenSource.Token);
                dataRequestReceiver.StartListening(cancellationTokenSource.Token);
                waveformReceiver.StartReceiving(cancellationTokenSource.Token);

                Console.ReadLine();
                cancellationTokenSource.Cancel();
                waveformReceiver.Dispose();
                vitalSignsUdpClient.Dispose();
                waveformUdpClient.Dispose();
            }
        }
    }
}
