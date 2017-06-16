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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PostSharp.Laos.Serializers
{
    /// <summary>
    /// Implementation of <see cref="LaosSerializer"/> based on the
    /// <see cref="BinaryFormatter"/> provided by the full version
    /// of the .NET Framework.
    /// </summary>
    public class BinaryLaosSerializer : LaosSerializer
    {
        private static SerializationBinder binder = new BinaryLaosSerializationBinder();

        private readonly BinaryFormatter formatter = new BinaryFormatter();

        /// <summary>
        /// Initializes a new <see cref="BinaryLaosSerializer"/>.
        /// </summary>
        public BinaryLaosSerializer()
        {
            this.formatter = new BinaryFormatter {Binder = binder};
        }

        /// <inheritdoc />
        public override void Serialize( ILaosAspect[] aspects, Stream stream )
        {
            formatter.Serialize( stream, aspects );
        }

        /// <inheritdoc />
        public override ILaosAspect[] Deserialize( Stream stream )
        {
            object o = formatter.Deserialize( stream );

            ILaosAspect[] array = (ILaosAspect[]) o;

            return array;
        }

      
        /// <summary>
        /// Gets or sets the <see cref="SerializationBinder"/> used to deserializedaspects.
        /// </summary>
        /// <value>A <see cref="System.Runtime.Serialization.SerializationBinder"/>, or <b>null</b>
        /// to use the default one. By default, <see cref="BinaryLaosSerializationBinder"/>
        /// is used.</value>
        /// <remarks>
        /// You should set this property before the first aspect is deserialized;
        /// that is, before the static constructor of the first enhanced type is invoked.</remarks>
        public static SerializationBinder Binder
        {
            get { return binder; }
            set { binder = value; }
        }

    }
}

#endif