using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace PostSharp.Extensibility.Configuration
{
    public class MyXmlSerializerImpl
    {
        public MyXmlSerializerImpl() { }
        public MyXmlSerializerImpl(Assembly assembly)
        {
            _contract = (XmlSerializerImplementation)
                                assembly.CreateInstance("Microsoft.Xml.Serialization.GeneratedAssembly.XmlSerializerContract");
        }

        private XmlSerializerImplementation _contract;
        public XmlSerializer GetSerializer(Type type)
        {
            foreach (var key in _contract.TypedSerializers.Keys)
            {
                if (key.ToString().Contains(type.Name)) {
                    return (XmlSerializer)_contract.TypedSerializers[key];
                }   
            }
            return null;
        }
    }
}
