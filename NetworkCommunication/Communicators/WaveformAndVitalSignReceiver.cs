using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class WaveformAndVitalSignReceiver : IDisposable
    {
        private readonly VitalSignPacketParser vitalSignPacketParser;
        private readonly WaveformPacketParser waveformPacketParser;
        private readonly UdpClient vitalSignsUdpClient;
        private readonly UdpClient waveformUdpClient;

        public WaveformAndVitalSignReceiver(
            VitalSignPacketParser vitalSignPacketParser, 
            WaveformPacketParser waveformPacketParser,
            UdpClient vitalSignsUdpClient,
            UdpClient waveformUdpClient)
        {
            this.vitalSignPacketParser = vitalSignPacketParser;
            this.waveformPacketParser = waveformPacketParser;
            this.vitalSignsUdpClient = vitalSignsUdpClient;
            this.waveformUdpClient = waveformUdpClient;
        }

        public event EventHandler<WaveformCollection> NewWaveformData;
        public event EventHandler<VitalSignData> NewVitalSignData;

        public async void StartReceiving(CancellationToken cancellationToken)
        {
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

        public void Dispose()
        {
            vitalSignsUdpClient.Dispose();
            waveformUdpClient.Dispose();
        }
    }
}
