using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkCommunication.DataProcessing;

namespace NetworkCommunication.Communicators
{
    public class DiscoveryMessageSender
    {
        private readonly IPAddress ourIpAddress;
        private readonly IPAddress broadcastAddress;

        public DiscoveryMessageSender(IPAddress ourIpAddress, IPAddress broadcastAddress)
        {
            this.ourIpAddress = ourIpAddress;
            this.broadcastAddress = broadcastAddress;
        }

        public async void StartSending(CancellationToken token)
        {
            var discoveryMessageGenerator = new DiscoveryMessageGenerator();
            try
            {
                await Task.Delay(3000, token);
            }
            catch (OperationCanceledException)
            {
            }

            //var variablePortOffset = (int) (DateTime.Now - DateTime.Today).TotalSeconds % 697;
            while (!token.IsCancellationRequested)
            {
                //variablePortOffset++;
                //variablePortOffset %= 697;
                var sourcePort = Informations.DiscoveryMessageSourcePort; //3004 + variablePortOffset;
                var targetPort = Informations.DiscoveryTargetPort;
                var payloadData = new DiscoveryData(ourIpAddress);
                var discoveryMessage = discoveryMessageGenerator.GenerateDiscoveryPayload(payloadData);
                try
                {
                    using (var udpClient = new UdpClient(sourcePort))
                    {
                        var broadcastEndpoint = new IPEndPoint(broadcastAddress, targetPort);
                        await udpClient.SendAsync(discoveryMessage, payloadData.TotalBytes, broadcastEndpoint);
                    }
                }
                catch (InvalidOperationException)
                {
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
    }
}
