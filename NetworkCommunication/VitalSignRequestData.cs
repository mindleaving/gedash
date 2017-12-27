namespace NetworkCommunication
{
    public class VitalSignRequestData
    {
        public string WardName { get; set; } = "HOME";
        public string BedName { get; set; } = "B-02";
        public int TotalBytes { get; set; } = 62;
        public byte[] StartBytes { get; set; } = new byte[]
        {
            0x00, 0x25, 0x15, 0xfc, 0x00, 0x00, 0x00, 0x00, 0x00, 0x25, 0x15, 0xe4, 0x00, 0xca, 0x00, 0x60, 0x00, 0x06,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x29
        };
    }
}