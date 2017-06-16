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

using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.MarshalTypes
{
    /// <summary>
    /// Represents the type of an unmanaged string passed by value.
    /// </summary>
    public sealed class FixedStringMarshalType : MarshalType
    {
        /// <summary>
        /// Number of characters.
        /// </summary>
        private readonly int size;

        /// <summary>
        /// Initializes a new <see cref="FixedStringMarshalType"/>.
        /// </summary>
        /// <param name="size">Number of characters in the string.</param>
        public FixedStringMarshalType( int size )
        {
            this.size = size;
        }


        /// <summary>
        /// Gets the string size.
        /// </summary>
        public int Size { get { return this.size; } }

        /// <inheritdoc />
        internal override void WriteILReference( ILWriter writer )
        {
            writer.WriteKeyword( "fixed sysstring" );
            writer.WriteSymbol( '[' );
            writer.WriteInteger( this.size, IntegerFormat.Decimal );
            writer.WriteSymbol( ']' );
        }
    }
}