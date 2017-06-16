#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

#if !SMALL

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PostSharp.Laos.Serializers
{
    /// <summary>
    /// Implementation of <see cref="LaosSerializer"/> based on the <see cref="XmlSerializer"/> provided by the
    /// .NET Framework.
    /// </summary>
    public sealed class XmlLaosSerializer : LaosSerializer
    {
        private readonly Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();


        private XmlSerializer GetSerializer( Type type )
        {
            XmlSerializer serializer;

            if ( !serializers.TryGetValue( type, out serializer ) )
            {
                serializer = new XmlSerializer( type );
                serializers.Add( type, serializer );
            }

            return serializer;
        }

        /// <inheritdoc />
        public override void Serialize( ILaosAspect[] aspects, Stream stream )
        {
            XmlDocument targetDocument = new XmlDocument();
            XmlElement rootElement = targetDocument.CreateElement( "aspects" );
            targetDocument.AppendChild( rootElement );

            // Build a list of all aspect types and serialize aspects;
            for ( int i = 0; i < aspects.Length; i++ )
            {
                Type type = aspects[i].GetType();
                XmlSerializer serializer = GetSerializer( type );

                XmlElement aspectElement = targetDocument.CreateElement( "aspect" );
                rootElement.AppendChild( aspectElement );

                XmlElement typeElement = targetDocument.CreateElement( "typeName" );
                typeElement.InnerText = type.AssemblyQualifiedName;
                aspectElement.AppendChild( typeElement );


                StringWriter stringWriter = new StringWriter();
                serializer.Serialize( stringWriter, aspects[i] );

                XmlElement dataElement = targetDocument.CreateElement( "data" );
                aspectElement.AppendChild( dataElement );
                dataElement.InnerXml = stringWriter.ToString().Replace( "<?xml version=\"1.0\" encoding=\"utf-16\"?>",
                                                                        "" );
            }

            using ( XmlTextWriter writer = new XmlTextWriter( stream, Encoding.UTF8 ) )
            {
                targetDocument.WriteContentTo( writer );
            }
        }

        /// <inheritdoc />
        public override ILaosAspect[] Deserialize( Stream stream )
        {
            XmlDocument document;

            try
            {
                document = new XmlDocument();
                document.Load( stream );
            }
            finally
            {
                // The next line causes an error in CF...
                //stream.Dispose();
            }

            List<ILaosAspect> aspects = new List<ILaosAspect>();

            foreach ( XmlElement element in document.SelectNodes( "/aspects/aspect" ) )
            {
                string typeName = element.ChildNodes[0].InnerText;
                Type type = Type.GetType( typeName );
                XmlSerializer serializer = GetSerializer( type );
                XmlNodeReader reader = new XmlNodeReader( element.ChildNodes[1].ChildNodes[0] );
                aspects.Add( (ILaosAspect) serializer.Deserialize( reader ) );
            }

            return aspects.ToArray();
        }
    }
}

#endif