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

namespace PostSharp.Collections
{
    /// <summary>
    /// Typed pair of values.
    /// </summary>
    /// <typeparam name="T1">Type of the first value.</typeparam>
    /// <typeparam name="T2">Type of the second value.</typeparam>
    public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        /// <summary>
        /// First value.
        /// </summary>
        private readonly T1 first;

        /// <summary>
        /// Second value.
        /// </summary>
        private readonly T2 second;

        /// <summary>
        /// Initializes a new <see cref="Pair{T1,T2}"/> of values.
        /// </summary>
        /// <param name="first">First value.</param>
        /// <param name="second">Second value.</param>
        public Pair( T1 first, T2 second )
        {
            this.first = first;
            this.second = second;
        }

        /// <summary>
        /// Gets the first value.
        /// </summary>
        public T1 First
        {
            get { return this.first; }
        }

        /// <summary>
        /// Gets the second value.
        /// </summary>
        public T2 Second
        {
            get { return this.second; }
        }

        #region Equality

        /// <inheritdoc />
        public bool Equals( Pair<T1, T2> other )
        {
            return this.InternalEquals( other );
        }

        private bool InternalEquals( Pair<T1, T2> other )
        {
            return ( ( this.first == null && other.first == null ) || this.first.Equals( other.first ) ) &&
                   ( ( this.second == null && other.second == null ) || this.second.Equals( other.second ) );
        }

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            if ( obj is Pair<T1, T2> )
            {
                return this.InternalEquals( (Pair<T1, T2>) obj );
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Determines whether two instances are equal.
        /// </summary>
        /// <param name="left">A pair.</param>
        /// <param name="right">AA pair.</param>
        /// <returns><b>true</b> if both instances are equal, otherwise <b>false</b>.</returns>
        public static bool operator ==( Pair<T1, T2> left, Pair<T1, T2> right )
        {
            return left.Equals( right );
        }


        /// <summary>
        /// Determines whether two instances are different.
        /// </summary>
        /// <param name="left">A pair.</param>
        /// <param name="right">A pair.</param>
        /// <returns><b>true</b> if both instances are different, otherwise <b>false</b>.</returns>
        public static bool operator !=( Pair<T1, T2> left, Pair<T1, T2> right )
        {
            return !left.Equals( right );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hashCode = 0;
            if ( this.first != null )
            {
                hashCode = this.first.GetHashCode();
            }

            if ( this.second != null )
            {
                hashCode ^= this.second.GetHashCode();
            }

            return hashCode;
        }

        #endregion
    }
}