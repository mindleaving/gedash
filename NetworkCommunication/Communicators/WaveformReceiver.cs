using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class WaveformReceiver
    {
        readonly DataRequestSender dataRequestSender;

        public WaveformReceiver(DataRequestSender dataRequestSender)
        {
            this.dataRequestSender = dataRequestSender;
        }

        public event EventHandler<WaveformData> NewWaveformData;
        public event EventHandler<VitalSignData> NewVitalSignData;

        public async void RetrieveWaveformsFromTarget(
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
                var vitalSignsTask = ReceiveVitalSignData(vitalSignsUdpClient, cancellationToken);
                var waveformTask = ReceiveWaveforms(waveformUdpClient, cancellationToken);
                await Task.WhenAll(vitalSignsTask, waveformTask);
            }
        }

        async Task ReceiveVitalSignData(UdpClient udpClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Receive response
                var vitalSignData = await udpClient.ReceiveAsync();
                var timestamp = DateTime.Now;
                try
                {
                    var parseResult = VitalSignPacketParser.Parse(vitalSignData.Buffer, timestamp);
                    NewVitalSignData?.Invoke(this, parseResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        async Task ReceiveWaveforms(UdpClient udpClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Receive response
                var waveformData = await udpClient.ReceiveAsync();
                var timestamp = DateTime.Now;
                try
                {
                    var parseResult = WaveformPacketParser.Parse(waveformData.Buffer, timestamp);
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
