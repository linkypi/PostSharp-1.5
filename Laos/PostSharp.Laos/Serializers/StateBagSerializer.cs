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
using System.Reflection;
using System.Text;

namespace PostSharp.Laos.Serializers
{
    /// <summary>
    /// Trivial serializer for use with light editions of the .NET Framework
    /// (Compact Framework, Silverlight). Aspects should implement the
    /// <see cref="IStateBagSerializable"/> interface and should have
    /// a parameterless constructor. This constructor is used during deserialization.
    /// </summary>
    public sealed class StateBagSerializer : LaosSerializer
    {
        private static readonly Type[] emptyTypes = new Type[0];

        /// <inheritdoc />
        public override void Serialize( ILaosAspect[] aspects, Stream stream )
        {
            using ( BinaryWriter writer = new BinaryWriter( stream, Encoding.UTF8 ) )
            {
                // We write a list of types.
                Dictionary<Type, short> types = new Dictionary<Type, short>();
                for ( int i = 0; i < aspects.Length; i++ )
                {
                    Type type = aspects[i].GetType();
                    if ( !types.ContainsKey( type ) )
                    {
                        types.Add( type, (short) types.Count );
                    }
                }
                writer.Write( (short) types.Count );
                foreach ( KeyValuePair<Type, short> pair in types )
                {
                    writer.Write( pair.Key.AssemblyQualifiedName );
                }

                writer.Write( aspects.Length );

                for ( int i = 0; i < aspects.Length; i++ )
                {
                    IStateBagSerializable serializable = aspects[i] as IStateBagSerializable;

                    if ( serializable == null )
                    {
                        throw new StateBagSerializerException(
                            string.Format(
                                "The type {0} does not implement IStateBagSerializable. ",
                                aspects[i].GetType().Name ) );
                    }

                    if ( serializable.GetType().GetConstructor( emptyTypes ) == null )
                    {
                        throw new StateBagSerializerException( string.Format(
                                                                   "The type {0} has no default constructor. ",
                                                                   aspects[i].GetType().Name ) );
                    }

                    writer.Write( types[serializable.GetType()] );

                    StateBag stateBag = new StateBag();
                    serializable.Serialize( stateBag );

                    stateBag.Serialize( writer );
                }

                writer.Flush();

                stream.Seek( 0, SeekOrigin.Begin );
                Deserialize( stream );
            }
        }

        /// <inheritdoc />
        public override ILaosAspect[] Deserialize( Stream stream )
        {
            BinaryReader reader = new BinaryReader( stream );

            // Read the dictionary of types.
            short typesCount = reader.ReadInt16();
            ConstructorInfo[] constructors = new ConstructorInfo[typesCount];

            for ( int i = 0; i < typesCount; i++ )
            {
                string typeName = reader.ReadString();
                Type type = Type.GetType( typeName );
                constructors[i] = type.GetConstructor( emptyTypes );
            }

            int aspectsCount = reader.ReadInt32();
            ILaosAspect[] aspects = new ILaosAspect[aspectsCount];

            for ( int i = 0; i < aspectsCount; i++ )
            {
                short typeId = reader.ReadInt16();
                IStateBagSerializable serializable = (IStateBagSerializable) constructors[typeId].Invoke( null );

                StateBag stateBag = StateBag.Deserialize( reader );
                serializable.Deserialize( stateBag );

                aspects[i] = (ILaosAspect) serializable;
            }

            return aspects;
        }
    }
}

#endif