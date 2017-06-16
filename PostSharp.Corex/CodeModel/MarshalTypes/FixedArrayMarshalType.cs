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

using System.Runtime.InteropServices;
using PostSharp.ModuleWriter;

#endregion

namespace PostSharp.CodeModel.MarshalTypes
{
    /// <summary>
    /// Represents the type of an unmanaged array with fixed number of elements, passed by value.
    /// </summary>
    public sealed class FixedArrayMarshalType : MarshalType
    {
        #region Fields

        /// <summary>
        /// Type of array elements.
        /// </summary>
        private readonly UnmanagedType elementType;

        /// <summary>
        /// Number of array elements.
        /// </summary>
        private readonly int elementNumber;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="FixedArrayMarshalType"/>.
        /// </summary>
        /// <param name="elementType">Type of array elements.</param>
        /// <param name="elementNumber">Number of array elements.</param>
        public FixedArrayMarshalType( UnmanagedType elementType, int elementNumber )
        {
            this.elementNumber = elementNumber;
            this.elementType = elementType;
        }

        /// <summary>
        /// Gets the type of array elements.
        /// </summary>
        public UnmanagedType ElementType { get { return this.elementType; } }

        /// <summary>
        /// Gets the number of elements in the array.
        /// </summary>
        public int ElementNumber { get { return this.elementNumber; } }


        /// <inheritdoc />
        internal override void WriteILReference( ILWriter writer )
        {
            

            writer.WriteKeyword( "fixed array" );
            writer.WriteSymbol( '[' );
            writer.WriteInteger( this.elementNumber, IntegerFormat.Decimal );
            writer.WriteSymbol( ']' );

            if ((int)this.elementType != 0x50)
            {
                IntrinsicMarshalType.WriteILReference(this.elementType, writer);
            }
        }
    }
}