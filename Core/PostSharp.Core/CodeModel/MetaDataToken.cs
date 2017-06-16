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
using System.Globalization;
using PostSharp.ModuleReader;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Encapsulates a metadata token, which unambigously identifies a <see cref="MetadataDeclaration"/> 
    /// inside a <see cref="ModuleDeclaration"/>.
    /// </summary>
    /// <devDoc>
    /// These tokens are composed of two parts: the first 8 bits contain the
    /// <see cref="TokenType"/> and the lower 24 bits contain the 1-based index
    /// of the declaration in the table corresponding to the token type. A value of 0
    /// in the lower 24 bits mean that the token is null.
    /// </devDoc>
    public struct MetadataToken : IComparable<MetadataToken>, IEquatable<MetadataToken>
    {
        /// <summary>
        /// Integral value.
        /// </summary>
        private uint value;

        /// <summary>
        /// Null <see cref="MetadataToken"/>.
        /// </summary>
#pragma warning disable 649
        public static readonly MetadataToken Null;
#pragma warning restore 649

        #region Construction

        /// <summary>
        /// Initializes a new <see cref="MetadataToken"/> from its integral value.
        /// </summary>
        /// <param name="value">The token value.</param>
        internal MetadataToken( uint value )
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a new <see cref="MetadataToken"/> from a <see cref="TokenType"/>
        /// and the position of the declaration in its table.
        /// </summary>
        /// <param name="tokenType">Identifies the table to which the token belongs.</param>
        /// <param name="index">Identifies the position in the table.</param>
        internal MetadataToken( TokenType tokenType, int index )
        {
            this.value = (uint) ( (int) tokenType | ( index + 1 ) );
        }

        /// <summary>
        /// Initializes a new <see cref="MetadataToken"/> from a table ordinal
        /// (<see cref="MetadataTableOrdinal"/>) and the position of the declaration in
        /// its table.
        /// </summary>
        /// <param name="table">Identifies the table to which the token belongs.</param>
        /// <param name="index">Identifies the position in the table.</param>
        internal MetadataToken( MetadataTableOrdinal table, int index )
            : this( (TokenType) ( (int) table << 24 ), index )
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the integer value of the token.
        /// </summary>
        internal uint Value { get { return this.value; } }

        /// <summary>
        /// Determines whether the current token is null.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current token is null, otherwise <b>false</b>.
        /// </value>
        public bool IsNull { get { return ( this.value & 0x00FFFFFF ) == 0; } }


        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        internal TokenType TokenType
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( !this.IsNull, "MetadataTokenIsNull" );

                #endregion

                return (TokenType) ( this.value & 0xFF000000 );
            }
        }

        internal int TableIndex { get { return ( (int) this.TokenType ) >> 24; } }

        /// <summary>
        /// Gets the 0-based index of the token in the table to which it belong.
        /// </summary>
        internal int Index
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( !this.IsNull, "MetadataTokenIsNull" );

                #endregion

                return (int) ( ( this.value & 0x00FFFFFF ) - 1 );
            }
        }

        #endregion

        #region Iteration

        /// <summary>
        /// Increments the current token.
        /// </summary>
        /// <remarks>
        /// <para>The rationale of the current method is to be able to use a <see cref="MetadataToken"/>
        /// as an iterator in a classic <b>for</b> loop.
        /// </para>
        /// <para>This method is not implemented as an operator because operators 
        /// have to be public.</para>
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// for ( MetadataToken i = tkFirstProperty ; i.IsSmallerOrEqualThan( tkLastProperty ) ; i.Increment() )
        /// ]]>
        /// </example>
        internal void Increment()
        {
            this.value++;
        }

        /// <summary>
        /// Determines whether the current token is smaller or equal to another one.
        /// </summary>
        /// <param name="other">Another <see cref="MetadataToken"/>.</param>
        /// <returns><b>true</b> if the current <see cref="MetadataToken"/> is smaller or equal
        /// than the <paramref name="other"/>, otherwise <b>false</b>.</returns>
        /// <seealso cref="Increment"/>
        internal bool IsSmallerOrEqualThan( MetadataToken other )
        {
            return this.value <= other.value;
        }

        #endregion

        #region Comparison

        /// <inheritdoc />
        public bool Equals( MetadataToken other )
        {
            return other.value == this.value;
        }

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            if ( !( obj is MetadataToken ) )
            {
                return false;
            }

            return ( (MetadataToken) obj ).value == this.value;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) this.value;
        }

        /// <inheritdoc />
        public int CompareTo( MetadataToken other )
        {
            if ( this.value == other.value )
            {
                return 0;
            }
            else if ( (long) this.value < (long) other.value )
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }


        /// <summary>
        /// Determines whether two metadata tokens are equal.
        /// </summary>
        /// <param name="left">A <see cref="MetadataToken"/>.</param>
        /// <param name="right">A <see cref="MetadataToken"/>.</param>
        /// <returns><b>true</b> if both tokens are equal, otherwise <b>false</b>.</returns>
        public static bool operator ==( MetadataToken left, MetadataToken right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Determines whether two metadata tokens are different.
        /// </summary>
        /// <param name="left">A <see cref="MetadataToken"/>.</param>
        /// <param name="right">A <see cref="MetadataToken"/>.</param>
        /// <returns><b>true</b> if tokens are different, otherwise <b>false</b>.</returns>
        public static bool operator !=( MetadataToken left, MetadataToken right )
        {
            return !left.Equals( right );
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return this.value.ToString( "x", CultureInfo.InvariantCulture );
        }
    }
}