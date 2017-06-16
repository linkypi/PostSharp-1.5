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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Encapsulates a read-only array of characters.
    /// </summary>
    /// <remarks>We use <see cref="LiteralString"/>
    /// instead of <see cref="string"/> because <see cref="string"/> sometimes transforms 
    /// the array of characters, but we require binary identity between what we read
    /// and what we writer.</remarks>
    public struct LiteralString : IEquatable<LiteralString>
    {
        /// <summary>
        /// Represents a null <see cref="LiteralString"/>.
        /// </summary>
#pragma warning disable 649
        public static readonly LiteralString Null;
#pragma warning restore 649

        /// <summary>
        /// Characters.
        /// </summary>
        /// <value>
        /// An array of characters, or <b>null</b> if the <see cref="LiteralString"/>
        /// is null.
        /// </value>
        private readonly char[] chars;

        /// <summary>
        /// Initializes a new <see cref="LiteralString"/>
        /// from an array of characters.
        /// </summary>
        /// <param name="chars">An array of characters, or <b>null</b> to construct
        /// a null <see cref="LiteralString"/>.</param>
        /// <remarks>
        /// For performance reasons, the current constructor stores a reference 
        /// to <paramref name="chars"/>, i.e. does <i>not</i> take a copy of the array.
        /// </remarks>
        public LiteralString( char[] chars )
        {
            this.chars = chars;
        }

        /// <summary>
        /// Initializes a new <see cref="LiteralString"/> from a string.
        /// </summary>
        /// <param name="text">A string, or <b>null</b> to construct a null 
        /// <see cref="LiteralString"/>.</param>
        public LiteralString( string text )
        {
            this.chars = text == null ? null : text.ToCharArray();
        }

        /// <summary>
        /// Gets the string lenght.
        /// </summary>
        /// <value>
        /// The number of characters in the array.
        /// </value>
        /// <exception cref="InvalidOperationException">The <see cref="LiteralString"/> is null.</exception>
        public int Length
        {
            get
            {
                #region Preconditions

                ExceptionHelper.Core.AssertValidOperation( this.chars != null, "LiteralStringNull" );

                #endregion

                return this.chars.Length;
            }
        }

        /// <summary>
        /// Gets a character given its position.
        /// </summary>
        /// <param name="index">The character position.</param>
        /// <returns>The character at position <paramref name="index"/>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="LiteralString"/> is null.</exception>
        public char GetChar( int index )
        {
            #region Preconditions

            ExceptionHelper.Core.AssertValidOperation( this.chars != null, "LiteralStringNull" );

            #endregion

            return this.chars[index];
        }

        /// <summary>
        /// Convert this instance to a <see cref="String"/>.
        /// </summary>
        /// <returns>The <see cref="String"/> contained in the current instance,
        /// or <b>null</b> if the current instance is null.</returns>
        public override string ToString()
        {
            return this.chars == null ? null : new string( this.chars );
        }

        /// <summary>
        /// Converts a <see cref="string"/> to a <see cref="LiteralString"/>.
        /// </summary>
        /// <param name="text">A string, or <b>null</b> to construct a null 
        /// <see cref="LiteralString"/>.</param>
        /// <returns>A <see cref="LiteralString"/>.</returns>
        public static implicit operator LiteralString( string text )
        {
            return new LiteralString( text );
        }

        /// <summary>
        /// Determines whether the current instance represents a null string.
        /// </summary>
        public bool IsNull
        {
            get { return this.chars == null; }
        }

        #region Equality

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            if ( obj is LiteralString )
            {
                return this.Equals( (LiteralString) obj );
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool Equals( LiteralString other )
        {
            if ( this.chars == null )
            {
                return other.chars == null;
            }

            if ( this.chars.Length != other.chars.Length )
            {
                return false;
            }

            for ( int i = 0; i < this.chars.Length; i++ )
            {
                if ( this.chars[i] != other.chars[i] )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether two strings are equal.
        /// </summary>
        /// <param name="left">A <see cref="LiteralString"/>.</param>
        /// <param name="right">A <see cref="LiteralString"/>.</param>
        /// <returns><b>true</b> if both strings are equal, otherwise <b>false</b>.</returns>
        public static bool operator ==( LiteralString left, LiteralString right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Determines whether two strings are different.
        /// </summary>
        /// <param name="left">A <see cref="LiteralString"/>.</param>
        /// <param name="right">A <see cref="LiteralString"/>.</param>
        /// <returns><b>true</b> if both strings are different, otherwise <b>false</b>.</returns>
        public static bool operator !=( LiteralString left, LiteralString right )
        {
            return !left.Equals( right );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.chars == null ? 0 : this.chars.GetHashCode();
        }

        #endregion
    }
}