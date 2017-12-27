using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkCommunication
{
    public class AlarmReceiver
    {
        public async void StartReceiving(CancellationToken cancellationToken)
        {
            using (var udpClient = new UdpClient(7001))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await udpClient.ReceiveAsync();
                    var text = $"Alarm: {Encoding.ASCII.GetString(message.Buffer)}";
                    Console.WriteLine(text);
                }
            }
        }
    }
}
