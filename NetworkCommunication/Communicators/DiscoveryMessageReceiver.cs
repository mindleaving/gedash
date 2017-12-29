using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace NetworkCommunication.Communicators
{
    public class DiscoveryMessageReceiver
    {
        public async void StartReceiving(CancellationToken cancellationToken)
        {
            using (var udpClient = new UdpClient(7000))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await udpClient.ReceiveAsync();
                    var first4bytes = BitConverter.ToInt32(message.Buffer.Take(4).Reverse().ToArray(), 0);
                    var ipAddress = message.Buffer.Skip(4).Take(4).Select(b => (int)b + "").Aggregate((a,b) => a + "." + b);
                    var counter1 = BitConverter.ToUInt16(message.Buffer.Skip(8).Take(2).Reverse().ToArray(), 0);
                    var counter2 = BitConverter.ToUInt16(message.Buffer.Skip(10).Take(2).Reverse().ToArray(), 0);
                    var patientInfo = message.Buffer.Skip(12).Take(32)
                        .Select(b => (char)b + "")
                        .Aggregate((a,b) => a + b);
                    var other = "";
                    for (int i = 44; i < message.Buffer.Length; i += 4)
                    {
                        var byte1 = BitConverter.ToInt16(message.Buffer.Skip(i).Take(2).Reverse().ToArray(), 0);
                        var byte2 = BitConverter.ToInt16(message.Buffer.Skip(i+2).Take(2).Reverse().ToArray(), 0);
                        other += byte1 + byte2 + ";";
                    }
                    var decodedMessage = $"{first4bytes} {ipAddress} {counter1} {counter2}, {patientInfo}, {other}";
                    var text = $"{DateTime.Now:HH:mm:ss} from {message.RemoteEndPoint}: {decodedMessage}";
                    Console.WriteLine(text);
                }
            }
        }
    }
}
