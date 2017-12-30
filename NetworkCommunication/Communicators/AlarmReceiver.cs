using System;
using System.Net.Sockets;
using System.Threading;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class AlarmReceiver
    {
        private readonly AlarmMessageParser alarmMessageParser;

        public AlarmReceiver(AlarmMessageParser alarmMessageParser)
        {
            this.alarmMessageParser = alarmMessageParser;
        }

        public async void StartReceiving(CancellationToken cancellationToken)
        {
            using (var udpClient = new UdpClient(7001))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await udpClient.ReceiveAsync();
                    var parseResult = alarmMessageParser.Parse(
                        message.Buffer, 
                        DateTime.UtcNow);
                    NewAlarmReceived?.Invoke(this, parseResult);
                }
            }
        }

        public event EventHandler<Alarm> NewAlarmReceived;
    }
}
