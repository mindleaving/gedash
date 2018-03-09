using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetworkCommunication.Objects
{
    [CollectionDataContract]
    public class AnnotationCollection : List<Annotation>
    {
        public AnnotationCollection() { }
        public AnnotationCollection(IEnumerable<Annotation> annotations)
            : base(annotations)
        {
        }
    }
}
