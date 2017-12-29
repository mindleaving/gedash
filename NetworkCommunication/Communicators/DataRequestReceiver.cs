using System;
using System.Net.Sockets;
using System.Threading;
using NetworkCommunication.Objects;

namespace NetworkCommunication.Communicators
{
    public class DataRequestReceiver
    {
        readonly WaveformStreamer waveformStreamer;
        readonly VitalSignDataStreamer vitalSignStreamer;

        public DataRequestReceiver(
            WaveformStreamer waveformStreamer, 
            VitalSignDataStreamer vitalSignStreamer)
        {
            this.waveformStreamer = waveformStreamer;
            this.vitalSignStreamer = vitalSignStreamer;
        }

        public async void StartListening(CancellationToken cancellationToken)
        {
            using (var udpClient = new UdpClient(2000))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var request = await udpClient.ReceiveAsync();
                    var requestType = DetermineRequestType(request);
                    switch (requestType)
                    {
                        case DataRequestType.Waveform:
                            waveformStreamer.NewDataRequestReceived(request.RemoteEndPoint);
                            //Console.WriteLine($"Got waveform request from {request.RemoteEndPoint}");
                            break;
                        case DataRequestType.VitalSigns:
                            vitalSignStreamer.NewDataRequestReceived(request.RemoteEndPoint);
                            //Console.WriteLine($"Got vital sign request from {request.RemoteEndPoint}");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            waveformStreamer.StopStreaming();
            vitalSignStreamer.StopStreaming();
        }

        DataRequestType DetermineRequestType(UdpReceiveResult request)
        {
            switch (request.Buffer[23])
            {
                case 0x29:
                    return DataRequestType.VitalSigns;
                case 0x0b:
                    return DataRequestType.Waveform;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
