#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of compile-time components of PostSharp.                *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU General Public License     *
 *   as published by the Free Software Foundation.                             *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU General Public License         *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using PostSharp.PlatformAbstraction;
using PostSharp.Utilities;
using Microsoft.Xml.Serialization.GeneratedAssembly;

namespace PostSharp.Extensibility.Configuration
{
    /// <summary>
    /// Provides utility methods for deserialization.
    /// </summary>
    internal static class ConfigurationHelper
    {
        /// <summary>
        /// XML Namespace of PostSharp.
        /// </summary>
        public const string PostSharpNamespace = "http://schemas.postsharp.org/1.0/configuration";

        public const string AssemblyBindingNamespace = "urn:schemas-microsoft-com:asm.v1";

        /// <summary>
        /// Array of serializers.
        /// </summary>
        private static XmlSerializer[] serializers;

        /// <summary>
        /// XML schema for configuration files.
        /// </summary>
        private static XmlSchema schema;


        /// <summary>
        /// Gets a serializer for a given kind of configuration document.
        /// </summary>
        /// <param name="kind">Kind of configuration document (project, plug-in, application).</param>
        /// <returns>An <see cref="XmlSerializer"/> for this <paramref name="kind"/> of document.</returns>
        private static XmlSerializer GetXmlSerializer( ConfigurationDocumentKind kind )
        {
            if ( serializers == null )
            {
                using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
                {
                    serializers = new XmlSerializer[4];

                    if ( Platform.Current.Identity == PlatformIdentity.Microsoft)
                    {
                        Assembly assembly = Assembly.Load("PostSharp.Core.XmlSerializers");
                        //使用强命名程序集
                        //Assembly.Load(typeof (ConfigurationHelper).Assembly.FullName.Replace("PostSharp.Core", "PostSharp.Core.XmlSerializers"));
                        
                        //XmlSerializerImplementation contract = (XmlSerializerImplementation)
                        //    assembly.CreateInstance("Microsoft.Xml.Serialization.GeneratedAssembly.XmlSerializerContract");

                        var contract = new XmlSerializerContract();
                        //var contract = new MyXmlSerializerImpl(assembly);
                        serializers[(int) ConfigurationDocumentKind.Application] =
                            contract.GetSerializer(typeof (ApplicationConfiguration));
                        serializers[(int) ConfigurationDocumentKind.PlugIn] =
                            contract.GetSerializer(typeof (PlugInConfiguration));
                        serializers[(int) ConfigurationDocumentKind.Project] =
                            contract.GetSerializer(typeof (ProjectConfiguration));
                        serializers[(int) ConfigurationDocumentKind.ExternalAssemblyBinding] =
                            contract.GetSerializer(typeof (AssemblyBindingExternalConfiguration));

                    }
                    else
                    {
                        XmlSerializerFactory factory = new XmlSerializerFactory();
                        serializers[(int) ConfigurationDocumentKind.Application] =
                            factory.CreateSerializer(typeof (ApplicationConfiguration));
                        serializers[(int) ConfigurationDocumentKind.PlugIn] =
                            factory.CreateSerializer(typeof (PlugInConfiguration));
                        serializers[(int) ConfigurationDocumentKind.Project] =
                            factory.CreateSerializer(typeof (ProjectConfiguration));
                        serializers[(int) ConfigurationDocumentKind.ExternalAssemblyBinding] =
                            factory.CreateSerializer(typeof (AssemblyBindingExternalConfiguration));
                    }


                    Trace.Timings.WriteLine( "XML Serializers were acquired in {0} ms.", timer.CurrentTime );
                }
            }

            return serializers[(int) kind];
        }

        /// <summary>
        /// Reads an XML configuration file and deserializes it.
        /// </summary>
        /// <param name="filePath">Full path of the source configuration file.</param>
        /// <param name="documentKind">Kind of configuration document (project, plug-in, application).</param>
        /// <returns>The object representation of <paramref name="filePath"/>.</returns>
        public static BaseConfiguration DeserializeDocument( string filePath, ConfigurationDocumentKind documentKind )
        {
            XmlSerializer serializer = GetXmlSerializer( documentKind );

            using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
            {
                // Validation is disabled by default because it is very expensive.
                bool validate = ApplicationInfo.GetSettingBoolean( "ValidateXmlDocuments", false );
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
                                                          {ConformanceLevel = ConformanceLevel.Document};

                if ( validate )
                {
                    xmlReaderSettings.CheckCharacters = true;
                    xmlReaderSettings.Schemas.Add( GetXmlSchema() );
                    xmlReaderSettings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings;
                    xmlReaderSettings.ValidationType = ValidationType.Schema;
                }

                using ( XmlReader xmlReader = XmlReader.Create( filePath, xmlReaderSettings ) )
                {
                  
                    try
                    {

                            BaseConfiguration o = (BaseConfiguration)serializer.Deserialize(xmlReader);
                        Trace.Timings.WriteLine( "File {0} parsed in {1} ms.",
                                                 Path.GetFileName( filePath ), timer.CurrentTime );
                            return o;

                    }
                    catch ( InvalidOperationException e )
                    {
                            XmlSchemaException schemaException;

                            if ((schemaException = e.InnerException as XmlSchemaException) != null)
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0003",
                                                              new object[]
                                                                  {
                                                                      Message.GetExceptionStackMessage(
                                                                          schemaException )
                                                                  }, filePath,
                                                              schemaException.LineNumber,
                                                              schemaException.LinePosition, schemaException );
                        }
                        else
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0003",
                                                              new object[] {Message.GetExceptionStackMessage( e )},
                                                              filePath,
                                                              Message.NotAvailable, Message.NotAvailable, e );
                        }

                        return null; // unreachable
                    }
                }
            }
        }

        public static object DeserializeFragment( XmlElement fragment,ConfigurationDocumentKind documentKind  )
        {
            XmlSerializer serializer = GetXmlSerializer(documentKind);

            XmlNodeReader reader = new XmlNodeReader( fragment);
            return serializer.Deserialize(reader);
 
        }

        /// <summary>
        /// Gets the XML schema of configuration files.
        /// </summary>
        /// <returns></returns>
        public static XmlSchema GetXmlSchema()
        {
            if ( schema == null )
            {
                using (
                    Stream stream =
                        typeof(ConfigurationHelper).Assembly.GetManifestResourceStream(
                            "PostSharp.Resources.PostSharpConfiguration.xsd" ) )
                {
                    schema = XmlSchema.Read( stream, OnInvalidSchema );
                }
            }

            return schema;
        }

        /// <summary>
        /// Called by <see cref="GetXmlSchema"/> in case the schema is invalid.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnInvalidSchema( object sender, ValidationEventArgs e )
        {
            throw ExceptionHelper.Core.CreateAssertionFailedException( "InvalidXmlSchema", e.Message );
        }

        /// <summary>
        /// Converts a <see cref="NameValuePairCollection"/> into a <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="collection">Source collection.</param>
        /// <returns>The <see cref="NameValueCollection"/> built from <paramref name="collection"/>.</returns>
        internal static NameValueCollection ConvertNameValueCollection( NameValuePairCollection collection )
        {
            if ( collection == null )
                return null;

            NameValueCollection target = new NameValueCollection( collection.Count );
            foreach ( NameValuePair pair in collection )
            {
                target.Add( pair.Name, pair.Value );
            }

            return target;
        }

        /*
        /// <summary>
        /// Event handlers for the <see cref="XmlSerializer"/>.
        /// </summary>
        /// <remarks>
        /// The objective to have these handlers encapsulated into a standalone class
        /// is to store the context of these handlers: the source filename, which
        /// scope is method-level.
        /// </remarks>
        private class EventHandlers : IDisposable
        {
            /// <summary>
            /// Path of the source file.
            /// </summary>
            private string fileName;

            /// <summary>
            /// Monitored <see cref="XmlSerializer"/>.
            /// </summary>
            private XmlSerializer serializer;

            /// <summary>
            /// Settings.
            /// </summary>
            private XmlReaderSettings xmlReaderSettings;

            /// <summary>
            /// Initializes a new <see cref="EventHandlers"/>.
            /// </summary>
            /// <param name="fileName">Path of the source file.</param>
            /// <param name="serializer">Monitored <see cref="XmlSerializer"/>.</param>
            /// <param name="xmlReaderSettings">Settings.</param>
            public EventHandlers( string fileName, XmlSerializer serializer, XmlReaderSettings xmlReaderSettings )
            {
                this.fileName = fileName;
                this.serializer = serializer;
                this.xmlReaderSettings = xmlReaderSettings;
                this.serializer.UnknownAttribute += new XmlAttributeEventHandler( this.OnUnknownAttribute );
                this.serializer.UnknownElement += new XmlElementEventHandler( this.OnUnknownElement );
                this.serializer.UnknownNode += new XmlNodeEventHandler( this.OnUnknownNode );
                this.xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler( this.OnValidationEvent );
            }


            /// <summary>
            /// Event raised when an unknown attribute is met.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="e">Event arguments.</param>
            private void OnUnknownAttribute( object sender, XmlAttributeEventArgs e )
            {
                CoreMessageSource.Instance.Write( SeverityType.Warning, "PS0001",
                                                  new object[] {e.Attr.Name, XmlNodeType.Attribute, e.Attr.NamespaceURI},
                                                  this.fileName, e.LineNumber, e.LinePosition );
            }

            /// <summary>
            /// Event raised when an unknown element is met.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="e">Event arguments.</param>
            private void OnUnknownElement( object sender, XmlElementEventArgs e )
            {
                CoreMessageSource.Instance.Write( SeverityType.Warning, "PS0001",
                                                  new object[]
                                                      {e.Element.LocalName, XmlNodeType.Element, e.Element.NamespaceURI},
                                                  this.fileName, e.LineNumber, e.LinePosition );
            }

            /// <summary>
            /// Event raised when an unknown node is met.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="e">Event arguments.</param>
            private void OnUnknownNode( object sender, XmlNodeEventArgs e )
            {
                CoreMessageSource.Instance.Write( SeverityType.Warning, "PS0001",
                                                  new object[] {e.LocalName, e.NodeType, e.NamespaceURI}, this.fileName,
                                                  e.LineNumber, e.LinePosition );
            }

            /// <summary>
            /// Event raised when a validation error occurs.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="e">Event arguments.</param>
            private void OnValidationEvent( object sender, ValidationEventArgs e )
            {
                SeverityType severity;
                switch ( e.Severity )
                {
                    case XmlSeverityType.Error:
                        severity = SeverityType.Fatal;
                        break;

                    case XmlSeverityType.Warning:
                        severity = SeverityType.Warning;
                        break;

                    default:
                        throw ExceptionHelper.CreateInvalidEnumerationValueException( e.Severity, "e.Severity" );
                }

                CoreMessageSource.Instance.Write( severity, "PS0003", new object[] {e.Message}, this.fileName );
            }

            #region IDisposable Members

            /// <inheritdoc />
            public void Dispose()
            {
                this.serializer.UnknownAttribute -= new XmlAttributeEventHandler( this.OnUnknownAttribute );
                this.serializer.UnknownElement -= new XmlElementEventHandler( this.OnUnknownElement );
                this.serializer.UnknownNode -= new XmlNodeEventHandler( this.OnUnknownNode );
                this.xmlReaderSettings.ValidationEventHandler -= new ValidationEventHandler( this.OnValidationEvent );
            }

            #endregion
        }
         */
    }


    /// <summary>
    /// Kinds of configuration documents.
    /// </summary>
    internal enum ConfigurationDocumentKind
    {
        /// <summary>
        /// Application.
        /// </summary>
        Application,
        /// <summary>
        /// Plug-in.
        /// </summary>
        PlugIn,
        /// <summary>
        /// Project.
        /// </summary>
        Project,

        ExternalAssemblyBinding
    }
}
