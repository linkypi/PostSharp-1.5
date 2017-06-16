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

#region Using directives

using System;
using System.Diagnostics.CodeAnalysis;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.MarshalTypes
{
    /// <summary>
    /// Special <see cref="MarshalType"/> meaning that a custom marshaller should
    /// be used.
    /// </summary>
    [SuppressMessage( "Microsoft.Naming", "CA1704",
        Justification="Spelling is correct." )]
    public sealed class CustomMarshallerMarshalType : MarshalType
    {
        #region Fields

        /// <summary>
        /// Guid.
        /// </summary>
        private readonly Guid guid;

        /// <summary>
        /// Unmanaged type name.
        /// </summary>
        private readonly string unmanagedType;

        /// <summary>
        /// Managed type name.
        /// </summary>
        private readonly string managedType;

        /// <summary>
        /// Cookie.
        /// </summary>
        private readonly string cookie;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="CustomMarshallerMarshalType"/>.
        /// </summary>
        /// <param name="guid">GUID.</param>
        /// <param name="unmanagedType">Unmanaged type name.</param>
        /// <param name="managedType">Managed type name.</param>
        /// <param name="cookie">Cookie.</param>
        public CustomMarshallerMarshalType( Guid guid,
                                            string unmanagedType,
                                            string managedType,
                                            string cookie )
        {
            this.guid = guid;
            this.unmanagedType = unmanagedType;
            this.managedType = managedType;
            this.cookie = cookie;
        }


        /// <summary>
        /// Gets the custom marshaller GUID.
        /// </summary>
        public Guid Guid { get { return guid; } }

        /// <summary>
        /// Gets the unmanaged type name.
        /// </summary>
        public string UnmanagedType { get { return unmanagedType; } }

        /// <summary>
        /// Gets the managed type name.
        /// </summary>
        public string ManagedType { get { return managedType; } }

        /// <summary>
        /// Gets the cookie.
        /// </summary>
        public string Cookie { get { return cookie; } }


        /// <inheritdoc />
        internal override void WriteILReference( ILWriter writer )
        {
            if ( this.guid != Guid.Empty || !string.IsNullOrEmpty( this.unmanagedType ) )
            {
                throw new NotImplementedException();
            }

            writer.WriteKeyword( "custom" );
            writer.WriteSymbol( '(' );
            writer.WriteQuotedString( this.managedType, WriteStringOptions.DoubleQuoted | WriteStringOptions.IgnoreByteArray );
            writer.WriteSymbol( ',' );
            writer.WriteConditionalLineBreak();
            writer.WriteQuotedString(this.cookie, WriteStringOptions.DoubleQuoted | WriteStringOptions.IgnoreByteArray);
            writer.WriteSymbol( ')' );
        }
    }
}