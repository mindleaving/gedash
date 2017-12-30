using System;
using System.Net.Sockets;
using System.Threading;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class DiscoveryMessageReceiver
    {
        private readonly DiscoveryMessageParser messageParser;

        public DiscoveryMessageReceiver(DiscoveryMessageParser messageParser)
        {
            this.messageParser = messageParser;
        }

        public async void StartReceiving(CancellationToken cancellationToken)
        {
            using (var udpClient = new UdpClient(7000))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await udpClient.ReceiveAsync();
                    var discoveryMessage = messageParser.Parse(message.Buffer, DateTime.UtcNow);
                    DiscoveryMessageReceived?.Invoke(this, discoveryMessage);
                }
            }
        }

        public event EventHandler<DiscoveryMessage> DiscoveryMessageReceived;
    }
}
