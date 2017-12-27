using System.Linq;
using System.Text;

namespace NetworkCommunication
{
    public class DataRequestGenerator
    {
        public byte[] GenerateWaveformRequest(WaveformRequestData data)
        {
            var alarmTextBytes = Encoding.ASCII.GetBytes(data.AlarmText);
            var wardBedBytes = Encoding.ASCII.GetBytes($"{data.WardName}|{data.BedName}");
            var message = alarmTextBytes
                .Concat(data.IntermediateBytes)
                .Concat(wardBedBytes)
                .ToArray();
            var zeroPadding = new byte[data.TotalBytes-message.Length];
            message = message.Concat(zeroPadding).ToArray();
            return message;
        }

        public byte[] GenerateVitalSignRequest(VitalSignRequestData data)
        {
            var wardBedBytes = Encoding.ASCII.GetBytes($"{data.WardName}|{data.BedName}");
            byte[] message = data.StartBytes
                .Concat(wardBedBytes)
                .ToArray();
            var zeroPadding = new byte[data.TotalBytes-message.Length];
            message = message.Concat(zeroPadding).ToArray();
            return message;
        }
    }
}
