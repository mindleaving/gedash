using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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

            var ourIpAddress = IPAddress.Parse("192.168.1.194");
            var broadcastAddress = IPAddress.Parse("192.168.1.255");
            var targetAddress = IPAddress.Parse(args[0]);
            var simulationSettings = new SimulationSettings();
            var simulators = new Dictionary<SensorType, ISimulator>
            {
                { SensorType.EcgLeadI, new EcgSimulator(SensorType.EcgLeadI, simulationSettings) },
                { SensorType.EcgLeadII, new EcgSimulator(SensorType.EcgLeadII, simulationSettings) },
                { SensorType.EcgLeadIII, new EcgSimulator(SensorType.EcgLeadIII, simulationSettings) },
                { SensorType.EcgLeadPrecordial, new EcgSimulator(SensorType.EcgLeadPrecordial, simulationSettings) },
                { SensorType.RespirationRate, new RespirationSimulator(simulationSettings) },
                { SensorType.SpO2, new SpO2Simulator(simulationSettings) },
                { SensorType.BloodPressure, new NibpSimualtor(simulationSettings)}
            };
            var simulatedSensors = new[]
            {
                SensorType.RespirationRate,
                SensorType.SpO2,
                SensorType.BloodPressure,
                SensorType.EcgLeadI,
                SensorType.EcgLeadII,
                SensorType.EcgLeadIII,
                SensorType.EcgLeadPrecordial,
            };
            var appendToFile = true;
            var directory = $@"C:\Temp\{DateTime.Now:yyyy-MM-dd HHmmss}";

            var cancellationTokenSource = new CancellationTokenSource();
            var discoveryMessageSender = new DiscoveryMessageSender();
            var discoveryMessageReceiver = new DiscoveryMessageReceiver();
            var alarmReceiver = new AlarmReceiver();
            var dataRequestGenerator = new DataRequestGenerator();
            var dataRequestSender = new DataRequestSender(dataRequestGenerator);
            var waveformMessageBuilder = new WaveformMessageBuilder(simulatedSensors, simulators);
            var vitalSignMessageBuilder = new VitalSignMessageBuilder(simulatedSensors, simulators, ourIpAddress);

            using(var vitalSignStreamer = new VitalSignDataStreamer(vitalSignMessageBuilder))
            using(var waveformStreamer = new WaveformStreamer(waveformMessageBuilder))
            using (var waveformStorer = new WaveformStorer(directory, appendToFile))
            using (var vitalSignsStorer = new VitalSignsStorer(directory, appendToFile))
            {
                var dataRequestReceiver = new DataRequestReceiver(waveformStreamer, vitalSignStreamer);
                var waveformReceiver = new WaveformReceiver(dataRequestSender, waveformStorer, vitalSignsStorer);
                waveformStorer.Initialize();
                vitalSignsStorer.Initialize();

                discoveryMessageSender.StartSending(broadcastAddress, cancellationTokenSource.Token);
                discoveryMessageReceiver.StartReceiving(cancellationTokenSource.Token);
                alarmReceiver.StartReceiving(cancellationTokenSource.Token);
                //waveformReceiver.RetrieveWaveformsFromTarget(targetAddress, cancellationTokenSource.Token);
                dataRequestReceiver.StartListening(cancellationTokenSource.Token);

                Console.ReadLine();
                cancellationTokenSource.Cancel();
            }
        }
    }
}
