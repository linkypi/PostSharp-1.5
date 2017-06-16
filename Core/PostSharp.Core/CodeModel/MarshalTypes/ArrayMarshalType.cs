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
    /// Represents the type of an unmanaged LPArray, i.e. a pointer to a standard C array.
    /// </summary>
    /// <remarks>
    /// The number of elements in the array is determined by the following formula:
    /// <list type="bullet">
    ///		<item>
    ///			If <see cref="AdditionalSizeParameter"/> = 0, the size is <see cref="FixedArraySize"/>.
    ///		</item>
    ///		<item>
    ///			Otherwise, the size is @<see cref="AdditionalSizeParameter"/> + <see cref="FixedArraySize"/>,
    ///			where @<see cref="AdditionalSizeParameter"/> is the value of the parameter
    ///			whose 0-based index is <see cref="AdditionalSizeParameter"/>-1.
    ///		</item>
    /// </list>
    /// </remarks>
    public sealed class ArrayMarshalType : MarshalType
    {
        #region Fields

        /// <summary>
        /// Type of array elements.
        /// </summary>
        private readonly UnmanagedType elementType;

        /// <summary>
        /// Ordinal of the parameter containing the additional size.
        /// </summary>
        private readonly int additionalSizeParameter;

        //private readonly int elementNumberMultiplier;
        /// <summary>
        /// Fixed array size.
        /// </summary>
        private readonly int fixedArraySize;

        #endregion

        /// <summary>
        /// When applies to the <see cref="ElementType"/> property, means that the
        /// element type is not known.
        /// </summary>
        public const UnmanagedType UnknownElementType = (UnmanagedType) 0x50;

        /// <summary>
        /// Initializes a new instance of <see cref="ArrayMarshalType"/>.
        /// </summary>
        /// <param name="elementType">Type of array elements.</param>
        /// <param name="fixedArraySize">Fixed array size, or -1 if the size is not specified.</param>
        /// <param name="additionalSizeParameter">0-based index of the parameter containing
        /// the additional number of array elements, or -1 is the array size is solely
        /// determined by <paramref name="fixedArraySize"/>.</param>
        public ArrayMarshalType(
            UnmanagedType elementType,
            int fixedArraySize,
            int additionalSizeParameter /*,
			int elementNumberMultiplier*/ )
        {
            this.elementType = elementType;
            this.additionalSizeParameter = additionalSizeParameter;
            //this.elementNumberMultiplier = elementNumberMultiplier;
            this.fixedArraySize = fixedArraySize;
        }


        /// <summary>
        /// Gets the type of elements.
        /// </summary>
        public UnmanagedType ElementType { get { return this.elementType; } }

        /// <summary>
        /// Gets the index of the parameter containing the array size.
        /// </summary>
        /// <value>
        /// The 1-based index of the parameter containing the array size, or 0 if
        /// the array size is fixed and set to <see cref="FixedArraySize"/>.
        /// </value>
        public int AdditionalSizeParameter { get { return additionalSizeParameter; } }

        //public int ElementNumberMultiplier { get { return this.elementNumberMultiplier; } }

        /// <summary>
        /// Gets the fixed array size.
        /// </summary>
        public int FixedArraySize { get { return this.fixedArraySize; } }


        /// <inheritdoc />
        internal override void WriteILReference( ILWriter writer )
        {
            /*
			 * Samples
			  .method public hidebysig newslot abstract virtual
          instance int32  Next(int32 celt,
                               [out] valuetype System.Runtime.InteropServices.ComTypes.STATDATA[]  marshal([ + 0]) rgelt,
                               [out] int32[]  marshal([1]) pceltFetched) cil managed preservesig
		{
		}
			 * */
            if ( this.elementType != UnknownElementType )
            {
                IntrinsicMarshalType.WriteILReference( this.elementType, writer );
            }
            writer.WriteSymbol( '[' );


            if ( this.fixedArraySize != -1 )
            {
                // Fixed size.

                writer.WriteInteger( this.fixedArraySize, IntegerFormat.Decimal );
            }

            if ( this.additionalSizeParameter != -1 &&
                 !( this.additionalSizeParameter == 0 && this.fixedArraySize != -1 ) )
            {
                // Parameter.

                writer.WriteSymbol( '+' );
                writer.WriteInteger( this.additionalSizeParameter, IntegerFormat.Decimal );
            }


            writer.WriteSymbol( ']' );
        }
    }
}