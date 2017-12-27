using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkCommunication
{
    public class DataRequestSender
    {
        readonly DataRequestGenerator dataRequestGenerator;

        public DataRequestSender(DataRequestGenerator dataRequestGenerator)
        {
            this.dataRequestGenerator = dataRequestGenerator;
        }

        public async void StartRequesting(
            IPEndPoint target,
            UdpClient metadataUdpClient,
            UdpClient waveformUdpClient,
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

        async void SendMetadataRequest(
            VitalSignRequestData payloadData, 
            IPEndPoint target,
            UdpClient udpClient)
        {
            var discoveryMessage = dataRequestGenerator.GenerateVitalSignRequest(payloadData);
            await SendBytes(target, udpClient, discoveryMessage);
        }

        async void SendWaveformRequest(
            WaveformRequestData payloadData, 
            IPEndPoint target,
            UdpClient udpClient)
        {
            var discoveryMessage = dataRequestGenerator.GenerateWaveformRequest(payloadData);
            await SendBytes(target, udpClient, discoveryMessage);
        }

        static async Task SendBytes(
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
