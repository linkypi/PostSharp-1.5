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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#endregion

namespace PostSharp.ModuleWriter
{
    /// <summary>
    /// Represents a rangle, i.e. an upper bound and a lower bound.
    /// </summary>
    internal struct AddressRange
    {
        /// <summary>
        /// The complement of the lower bound, or 0 if the lower bound is undeterminate.
        /// </summary>
        private readonly int lowerBound;

        /// <summary>
        /// The complement of the upper bound, or 0 if the upper bound is undeterminate.
        /// </summary>
        private readonly int upperBound;

        /// <summary>
        /// Initializes a new <see cref="AddressRange"/>.
        /// </summary>
        /// <param name="lowerBound">Lower bound.</param>
        /// <param name="upperBound">Upper bound.</param>
        public AddressRange( int lowerBound, int upperBound )
        {
            this.lowerBound = ~lowerBound;
            this.upperBound = ~upperBound;
        }

        /// <summary>
        /// Gets the lower bound.
        /// </summary>
        public int LowerBound { get { return ~this.lowerBound; } }

        /// <summary>
        /// Gets or sets the upper bound.
        /// </summary>
        public int UpperBound { get { return ~this.upperBound; } }

        /// <summary>
        /// Determines whether the range is undeterminate, i.e. whether it is
        /// still unknown.
        /// </summary>
        public bool IsUndeterminate { get { return this.lowerBound == 0; } }

        /// <summary>
        /// Adds two ranges.
        /// </summary>
        /// <param name="a">An <see cref="AddressRange"/>.</param>
        /// <param name="b">An <see cref="AddressRange"/>.</param>
        /// <returns>The range containing all values that may result from the addition
        /// of any value of <paramref name="a"/> with any value of <paramref name="b"/>.</returns>
        public static AddressRange operator +( AddressRange a, AddressRange b )
        {
            if ( a.IsUndeterminate || b.IsUndeterminate )
            {
                return new AddressRange();
            }
            else
            {
                return new AddressRange( a.LowerBound + b.LowerBound, a.UpperBound + b.UpperBound );
            }
        }

        /// <summary>
        /// Adds a range to an integer.
        /// </summary>
        /// <param name="a">An <see cref="AddressRange"/>.</param>
        /// <param name="b">An integer.</param>
        /// <returns>The range containing all values tha may result from the addition
        /// of any value of <paramref name="a"/> with <paramref name="b"/>.</returns>
        public static AddressRange operator +( AddressRange a, int b )
        {
            if ( a.IsUndeterminate )
            {
                return new AddressRange();
            }
            else
            {
                return new AddressRange( a.LowerBound + b, a.UpperBound + b );
            }
        }

        /// <summary>
        /// Substracts two ranges.
        /// </summary>
        /// <param name="a">An <see cref="AddressRange"/>.</param>
        /// <param name="b">An <see cref="AddressRange"/>.</param>
        /// <returns>The range containing all values that may result from the substraction
        /// from any value of <paramref name="a"/> of any value of <paramref name="b"/>.</returns>
        public static AddressRange operator -( AddressRange a, AddressRange b )
        {
            if ( a.IsUndeterminate || b.IsUndeterminate )
            {
                return new AddressRange();
            }
            else
            {
                return new AddressRange( a.LowerBound - b.UpperBound, a.UpperBound - b.LowerBound );
            }
        }

        /// <summary>
        /// Substracts an integer from a range.
        /// </summary>
        /// <param name="a">An <see cref="AddressRange"/>.</param>
        /// <param name="b">An integer.</param>
        /// <returns>The range containing all values tha may result from the substraction
        /// of <paramref name="b"/> from any value of <paramref name="a"/>.</returns>
        public static AddressRange operator -( AddressRange a, int b )
        {
            return new AddressRange( a.LowerBound - b, a.UpperBound - b );
        }

        /// <summary>
        /// Determines whether addresses in the range are <see cref="AddressDistance.Near"/>
        /// (i.e. codable on 1 signed byte), <see cref="AddressDistance.Far"/>
        /// or <see cref="AddressDistance.Undeterminate"/>.
        /// </summary>
        public AddressDistance Distance
        {
            get
            {
                if ( this.IsUndeterminate )
                {
                    return AddressDistance.Undeterminate;
                }
                else
                {
                    if ( this.LowerBound >= sbyte.MinValue && this.UpperBound <= sbyte.MaxValue )
                    {
                        return AddressDistance.Near;
                    }
                    else if ( ( this.LowerBound <= sbyte.MinValue && this.UpperBound >= sbyte.MinValue ) ||
                              ( this.LowerBound <= sbyte.MaxValue && this.UpperBound >= sbyte.MaxValue ) )
                    {
                        return AddressDistance.Undeterminate;
                    }
                    else
                    {
                        return AddressDistance.Far;
                    }
                }
            }
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format( CultureInfo.InvariantCulture, "{0:x}-{1:x}", this.LowerBound,
                                  this.UpperBound );
        }
    }

    /// <summary>
    /// Determines whether addresses can be coded on a single signed byte.
    /// </summary>
    public enum AddressDistance
    {
        /// <summary>
        /// The address can be coded on a single signed byte.
        /// </summary>
        Near,

        /// <summary>
        /// The address has to be coded on a 32-bit signed integer.
        /// </summary>
        Far,

        /// <summary>
        /// It is not known whether the address is <see cref="Near"/> or <see cref="Far"/>.
        /// </summary>
        [SuppressMessage( "Microsoft.Naming", "CA1704", Justification="Spelling is correct." )] Undeterminate
    }
}