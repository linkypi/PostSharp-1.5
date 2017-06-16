using Microsoft.Xml.Serialization.GeneratedAssembly;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace PostSharp.Core.XmlSerializers.GeneratedAssembly
{
    public class ApplicationConfigurationSerializer : XmlSerializer1
    {
        public override bool CanDeserialize(XmlReader xmlReader)
        {
            return xmlReader.IsStartElement("Configuration", "http://schemas.postsharp.org/1.0/configuration");
        }

        protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
        {
            ((XmlSerializationWriter1)writer).Write21_Configuration(objectToSerialize);
        }

        protected override object Deserialize(XmlSerializationReader reader)
        {
            return ((XmlSerializationReader1)reader).Read21_Configuration();
        }
    }
}
