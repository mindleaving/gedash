using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkCommunication
{
    public class WaveformReceiver
    {
        readonly DataRequestSender dataRequestSender;
        readonly WaveformStorer waveformStorer;
        readonly VitalSignsStorer m_vitalSignsStorer;
        readonly VitalSignsStorer vitalSignsStorer;

        public WaveformReceiver(
            DataRequestSender dataRequestSender, 
            WaveformStorer waveformStorer, 
            VitalSignsStorer vitalSignsStorer)
        {
            this.dataRequestSender = dataRequestSender;
            this.waveformStorer = waveformStorer;
            this.vitalSignsStorer = vitalSignsStorer;
        }

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
                    vitalSignsStorer.Store(parseResult);
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
                    waveformStorer.Store(parseResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
