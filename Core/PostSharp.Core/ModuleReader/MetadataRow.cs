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

using System.Diagnostics.CodeAnalysis;

namespace PostSharp.ModuleReader
{
    /// <summary>
    /// Encapsulates a pointer to a metadata row.
    /// </summary>
    internal struct MetadataRow
    {
        /// <summary>
        /// Represents a null <see cref="MetadataRow"/>.
        /// </summary>
#pragma warning disable 649
        public static readonly MetadataRow Null;
#pragma warning restore 649

#if DEBUG
        /// <summary>
        /// Table to which the row belongs.
        /// </summary>
#pragma warning disable 219
        private MetadataTableOrdinal table;
#pragma warning restore 219
#endif

        /// <summary>
        /// Address of the first byte of the row.
        /// </summary>
        private readonly unsafe byte* address;

        /// <summary>
        /// Initializes a new <see cref="MetadataRow"/>.
        /// </summary>
        /// <param name="address">Address of the first byte of the row.</param>
        /// <param name="table">Parent table.</param>
        [SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters" )]
        internal unsafe MetadataRow( byte* address, MetadataTableOrdinal table )
        {
            this.address = address;
#if DEBUG
            this.table = table;
#endif
        }

        /// <summary>
        /// Gets the address of the first byte of the row.
        /// </summary>
        internal unsafe byte* Address { get { return this.address; } }

#if DEBUG
        //internal unsafe MetadataTableOrdinal Table { get { return this.table; } }
#endif

        /// <summary>
        /// Determines whether the current <see cref="MetadataRow"/> is null.
        /// </summary>
        public bool IsNull
        {
            get
            {
                unsafe
                {
                    return this.address == null;
                }
            }
        }

        /// <summary>
        /// Determines whether a <see cref="MetadataRow"/> is located strictly
        /// before another <see cref="MetadataRow"/>.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns><b>true</b> if <paramref name="left"/> is strictly before 
        /// <paramref name="right"/>, otherwise <b>false</b>.</returns>
        public static bool operator <( MetadataRow left, MetadataRow right )
        {
            unsafe
            {
                return left.address < right.address;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="MetadataRow"/> is located strictly
        /// after another <see cref="MetadataRow"/>.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns><b>true</b> if <paramref name="left"/> is strictly after 
        /// <paramref name="right"/>, otherwise <b>false</b>.</returns>
        public static bool operator >( MetadataRow left, MetadataRow right )
        {
            unsafe
            {
                return left.address > right.address;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="MetadataRow"/> is located
        /// before or at the same position than another <see cref="MetadataRow"/>.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns><b>true</b> if <paramref name="left"/> is location before 
        /// or at the same location than <paramref name="right"/>, 
        /// otherwise <b>false</b>.</returns>
        public static bool operator <=( MetadataRow left, MetadataRow right )
        {
            unsafe
            {
                return left.address <= right.address;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="MetadataRow"/> is located
        /// after or at the same position than another <see cref="MetadataRow"/>.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns><b>true</b> if <paramref name="left"/> is located after 
        /// or at the same location than <paramref name="right"/>, 
        /// otherwise <b>false</b>.</returns>
        public static bool operator >=( MetadataRow left, MetadataRow right )
        {
            unsafe
            {
                return left.address >= right.address;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="MetadataRow"/> is located strictly
        /// before another <see cref="MetadataRow"/>.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns><b>true</b> if <paramref name="left"/> is strictly before 
        /// <paramref name="right"/>, otherwise <b>false</b>.</returns>
        public static bool operator ==( MetadataRow left, MetadataRow right )
        {
            unsafe
            {
                return left.address == right.address;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="MetadataRow"/> is located strictly
        /// before another <see cref="MetadataRow"/>.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns><b>true</b> if <paramref name="left"/> is strictly before 
        /// <paramref name="right"/>, otherwise <b>false</b>.</returns>
        public static bool operator !=( MetadataRow left, MetadataRow right )
        {
            unsafe
            {
                return left.address != right.address;
            }
        }


        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            if ( obj == null || !( obj is MetadataRow ) )
            {
                return false;
            }

            unsafe
            {
                return this.address == ( (MetadataRow) obj ).address;
            }
        }


        /// <inheritdoc />
        public override int GetHashCode()
        {
            unsafe
            {
                return this.address->GetHashCode();
            }
        }
    }
}