namespace NetworkCommunication
{
    public class WaveformRequestData
    {
        public string AlarmText { get; set; } = "HOME ALARME:";
        public string WardName { get; set; } = "HOME";
        public string BedName { get; set; } = "B-02";
        public int TotalBytes { get; set; } = 62;
        public byte[] IntermediateBytes { get; set; } = new byte[]
        {
            0x00, 0xca, 0x00, 0x21, 0x00, 0x06, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x0b
        };
    }
}