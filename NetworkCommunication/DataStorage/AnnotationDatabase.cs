using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using NetworkCommunication.Objects;

namespace NetworkCommunication.DataStorage
{
    public class AnnotationDatabase
    {
        private readonly string annotationFile;
        private readonly DataContractSerializer serializer = new DataContractSerializer(typeof(AnnotationCollection));

        public AnnotationDatabase(FileManager fileManager, PatientInfo patientInfo)
        {
            var patientDirectory = fileManager.GetPatientDirectory(patientInfo);
            annotationFile = Path.Combine(patientDirectory, "annotations.xml");
            LoadAnnotations();
        }

        private readonly List<Annotation> annotations = new List<Annotation>();
        public IReadOnlyList<Annotation> Annotations => annotations;

        public void Add(Annotation annotation)
        {
            annotations.Add(annotation);
            StoreAnnotations();
        }

        private void LoadAnnotations()
        {
            annotations.Clear();
            if(!File.Exists(annotationFile))
                return;
            using (var stream = File.OpenRead(annotationFile))
            {
                try
                {
                    var annotationCollection = (AnnotationCollection) serializer.ReadObject(stream);
                    annotations.AddRange(annotationCollection);
                }
                catch (XmlException) { }
            }
        }

        private void StoreAnnotations()
        {
            using (var stream = File.Create(annotationFile))
            {
                serializer.WriteObject(stream, new AnnotationCollection(Annotations));
            }
        }
    }
}