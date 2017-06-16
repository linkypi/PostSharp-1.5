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
using System.Globalization;
using System.IO;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Unmanaged resource given in raw form, as an array of bytes.
    /// </summary>
    public sealed class RawUnmanagedResource : UnmanagedResource
    {
        private byte[] data;

        /// <summary>
        /// Initializes a new <see cref="RawUnmanagedResource"/>
        /// </summary>
        /// <param name="name">Resource name.</param>
        /// <param name="type">Resource type. Typically, an <see cref="UnmanagedResourceType"/>
        /// wrapped into an <see cref="UnmanagedResourceName"/>.</param>
        /// <param name="codePage">Code page identifier.</param>
        /// <param name="language">Language identifier.</param>
        /// <param name="version">Resource version. Only <see cref="System.Version.Major"/>
        /// and <see cref="System.Version.Minor"/> properties are relevant.</param>
        /// <param name="characteristics">Resource characteristics (attributes).</param>
        /// <param name="data">Raw data, or <b>null</b>.</param>
        public RawUnmanagedResource( UnmanagedResourceName name, UnmanagedResourceName type, int codePage, int language,
                                     Version version, int characteristics, byte[] data )
            :
                base( name, type, codePage, language, version, characteristics )
        {
            this.data = data;
        }

        /// <inheritdoc />
        internal override void Write( BinaryWriter writer )
        {
            if ( data != null )
            {
                writer.Write( data, 0, data.Length );
            }
        }

        /// <summary>
        /// Gets or sets the array of bytes implementing the resource.
        /// </summary>
        public byte[] Data { get { return this.data; } set { this.data = value; } }


        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format( CultureInfo.InvariantCulture,
                                  "RawUnmanagedResource: Type={0}, Name={1}, CodePage={2}, Size={3}",
                                  this.Type, this.Name, this.CodePage, this.data == null ? (object) "null" : this.data.Length );
        }
    }
}