using PostSharp.Core.XmlSerializers.GeneratedAssembly;
using PostSharp.Extensibility.Configuration;
using System;
using System.Collections;
using System.Xml.Serialization;

namespace Microsoft.Xml.Serialization.GeneratedAssembly
{
    public class XmlSerializerContract : XmlSerializerImplementation
    {
        private Hashtable readMethods;

        private Hashtable writeMethods;

        private Hashtable typedSerializers;

        public override XmlSerializationReader Reader
        {
            get
            {
                return new XmlSerializationReader1();
            }
        }

        public override XmlSerializationWriter Writer
        {
            get
            {
                return new XmlSerializationWriter1();
            }
        }

        public override Hashtable ReadMethods
        {
            get
            {
                if (this.readMethods == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable["PostSharp.Extensibility.Configuration.ApplicationConfiguration:http://schemas.postsharp.org/1.0/configuration:Configuration:True:"] = "Read21_Configuration";
                    hashtable["PostSharp.Extensibility.Configuration.PlugInConfiguration:http://schemas.postsharp.org/1.0/configuration:PlugIn:True:"] = "Read22_PlugIn";
                    hashtable["PostSharp.Extensibility.Configuration.ProjectConfiguration:http://schemas.postsharp.org/1.0/configuration:Project:True:"] = "Read23_Project";
                    if (this.readMethods == null)
                    {
                        this.readMethods = hashtable;
                    }
                }
                return this.readMethods;
            }
        }

        public override Hashtable WriteMethods
        {
            get
            {
                if (this.writeMethods == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable["PostSharp.Extensibility.Configuration.ApplicationConfiguration:http://schemas.postsharp.org/1.0/configuration:Configuration:True:"] = "Write21_Configuration";
                    hashtable["PostSharp.Extensibility.Configuration.PlugInConfiguration:http://schemas.postsharp.org/1.0/configuration:PlugIn:True:"] = "Write22_PlugIn";
                    hashtable["PostSharp.Extensibility.Configuration.ProjectConfiguration:http://schemas.postsharp.org/1.0/configuration:Project:True:"] = "Write23_Project";
                    if (this.writeMethods == null)
                    {
                        this.writeMethods = hashtable;
                    }
                }
                return this.writeMethods;
            }
        }

        public override Hashtable TypedSerializers
        {
            get
            {
                if (this.typedSerializers == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add("PostSharp.Extensibility.Configuration.ApplicationConfiguration:http://schemas.postsharp.org/1.0/configuration:Configuration:True:", new ApplicationConfigurationSerializer());
                    hashtable.Add("PostSharp.Extensibility.Configuration.PlugInConfiguration:http://schemas.postsharp.org/1.0/configuration:PlugIn:True:", new PlugInConfigurationSerializer());
                    hashtable.Add("PostSharp.Extensibility.Configuration.ProjectConfiguration:http://schemas.postsharp.org/1.0/configuration:Project:True:", new ProjectConfigurationSerializer());
                    if (this.typedSerializers == null)
                    {
                        this.typedSerializers = hashtable;
                    }
                }
                return this.typedSerializers;
            }
        }

        public override bool CanSerialize(Type type)
        {
            return type == typeof(ApplicationConfiguration) || type == typeof(PlugInConfiguration) || type == typeof(ProjectConfiguration);
        }

        public override XmlSerializer GetSerializer(Type type)
        {
            if (type == typeof(ApplicationConfiguration))
            {
                return new ApplicationConfigurationSerializer();
            }
            if (type == typeof(PlugInConfiguration))
            {
                return new PlugInConfigurationSerializer();
            }
            if (type == typeof(ProjectConfiguration))
            {
                return new ProjectConfigurationSerializer();
            }
            return null;
        }
    }
}
