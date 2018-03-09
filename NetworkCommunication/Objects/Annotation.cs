using System;
using System.Runtime.Serialization;

namespace NetworkCommunication.Objects
{
    [DataContract]
    public class Annotation
    {
        public Annotation(DateTime timestamp, string title, string note)
        {
            Timestamp = timestamp;
            Title = title;
            Note = note;
        }

        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Note { get; set; }
    }
}