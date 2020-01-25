using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class DataRequestSender
    {
        private readonly DataRequestGenerator dataRequestGenerator;
        private readonly UdpClient metadataUdpClient;
        private readonly UdpClient waveformUdpClient;

        public DataRequestSender(
            DataRequestGenerator dataRequestGenerator,
            UdpClient metadataUdpClient,
            UdpClient waveformUdpClient)
        {
            this.dataRequestGenerator = dataRequestGenerator;
            this.metadataUdpClient = metadataUdpClient;
            this.waveformUdpClient = waveformUdpClient;
        }

        public async void StartRequesting(
            IPEndPoint target,
            TimeSpan interval,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var payload1 = new VitalSignRequestData();
                SendMetadataRequest(payload1, target, metadataUdpClient);
                var payload2 = new WaveformRequestData();
                SendWaveformRequest(payload2, target, waveformUdpClient);
                try
                {
                    await Task.Delay(interval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async void SendMetadataRequest(
            VitalSignRequestData payloadData, 
            IPEndPoint target,
            UdpClient udpClient)
        {
            var discoveryMessage = dataRequestGenerator.GenerateVitalSignRequest(payloadData);
            await SendBytes(target, udpClient, discoveryMessage);
        }

        private async void SendWaveformRequest(
            WaveformRequestData payloadData, 
            IPEndPoint target,
            UdpClient udpClient)
        {
            var discoveryMessage = dataRequestGenerator.GenerateWaveformRequest(payloadData);
            await SendBytes(target, udpClient, discoveryMessage);
        }

        private static async Task SendBytes(
            IPEndPoint target, 
            UdpClient udpClient, 
            byte[] discoveryMessage)
        {
            try
            {
                await udpClient.SendAsync(discoveryMessage, discoveryMessage.Length, target);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending data request to {target.Address}:{target.Port}: {e.Message}");
            }
        }
    }
}
