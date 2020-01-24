using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class WaveformAndVitalSignReceiver
    {
        private readonly DataRequestSender dataRequestSender;
        private readonly VitalSignPacketParser vitalSignPacketParser;
        private readonly WaveformPacketParser waveformPacketParser;

        public WaveformAndVitalSignReceiver(
            DataRequestSender dataRequestSender, 
            VitalSignPacketParser vitalSignPacketParser, 
            WaveformPacketParser waveformPacketParser)
        {
            this.dataRequestSender = dataRequestSender;
            this.vitalSignPacketParser = vitalSignPacketParser;
            this.waveformPacketParser = waveformPacketParser;
        }

        public event EventHandler<WaveformCollection> NewWaveformData;
        public event EventHandler<VitalSignData> NewVitalSignData;

        public async void StartReceiving(
            IPAddress targetAddress, 
            CancellationToken cancellationToken)
        {
            var target = new IPEndPoint(targetAddress, Informations.DataRequestPort);
            using (var vitalSignsUdpClient = new UdpClient(Informations.VitalSignsRequestOutboundPort))
            using (var waveformUdpClient = new UdpClient(Informations.WaveformRequestOutboundPort))
            {
                dataRequestSender.StartRequesting( 
                    target, 
                    vitalSignsUdpClient, 
                    waveformUdpClient, 
                    TimeSpan.FromSeconds(5), 
                    cancellationToken);
                var vitalSignsTask = Task.Factory.StartNew(
                    () => ReceiveVitalSignData(vitalSignsUdpClient, cancellationToken),
                    cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Current);
                var waveformTask = Task.Factory.StartNew(
                    () => ReceiveWaveforms(waveformUdpClient, cancellationToken),
                    cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Current);
                try
                {
                    await Task.WhenAll(vitalSignsTask, waveformTask);
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
            }
        }

        private void ReceiveVitalSignData(UdpClient udpClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Receive response
                var receiveTask = udpClient.ReceiveAsync();
                receiveTask.Wait(cancellationToken);
                if(cancellationToken.IsCancellationRequested)
                    return;
                var vitalSignData = receiveTask.Result;
                var timestamp = DateTime.Now;
                try
                {
                    var parseResult = vitalSignPacketParser.Parse(vitalSignData.Buffer, timestamp);
                    NewVitalSignData?.Invoke(this, parseResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void ReceiveWaveforms(UdpClient udpClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Receive response
                var receiveTask = udpClient.ReceiveAsync();
                receiveTask.Wait(cancellationToken);
                if(cancellationToken.IsCancellationRequested)
                    return;
                var waveformData = receiveTask.Result;
                var timestamp = DateTime.Now;
                try
                {
                    var parseResult = waveformPacketParser.Parse(
                        waveformData.Buffer, 
                        waveformData.RemoteEndPoint.Address, 
                        timestamp);
                    NewWaveformData?.Invoke(this, parseResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
