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

namespace PostSharp
{
    /// <summary>
    /// Three-state boolean: <see cref="True"/>, <see cref="False"/> and <see cref="Null"/>.
    /// </summary>
    /// <remarks>
    /// As in any three-state logic implementation, the <see cref="Null"/> element is absorbing for all operations.
    /// </remarks>
    public struct NullableBool : IEquatable<NullableBool>
    {
        private readonly int value;
        private const int valueTrue = 1;
        private const int valueFalse = 2;
        private const int valueNull = 0;

        private NullableBool( int value )
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a new <see cref="NullableBool"/> from a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">Initial value.</param>
        public NullableBool( bool value )
        {
            this.value = value ? valueTrue : valueFalse;
        }

        /// <summary>
        /// True.
        /// </summary>
        public static readonly NullableBool True = new NullableBool( valueTrue );

        /// <summary>
        /// False.
        /// </summary>
        public static readonly NullableBool False = new NullableBool( valueFalse );

        /// <summary>
        /// Null.
        /// </summary>
        public static readonly NullableBool Null = new NullableBool( valueNull );

        /// <summary>
        /// Determines whether the current instance is null.
        /// </summary>
        public bool IsNull
        {
            get { return this.value == valueNull; }
        }

        /// <summary>
        /// Determines whether the current instance equals another.
        /// </summary>
        /// <param name="other">A <see cref="NullableBool"/>.</param>
        /// <returns><b>true</b> if both instances are equal or <b>false</b> if they are different or
        /// at least one is <see cref="Null"/>.</returns>
        public bool Equals( NullableBool other )
        {
            return this.value != valueNull && other.value != valueNull && this.value == other.value;
        }

        /// <summary>
        /// Determines whether the current instance equals another.
        /// </summary>
        /// <param name="obj">A <see cref="NullableBool"/>.</param>
        /// <returns><b>true</b> if both instances are equal or <b>false</b> if they are different or
        /// at least one is <see cref="Null"/>.</returns>
        public override bool Equals( object obj )
        {
            if ( obj is NullableBool )
            {
                return this.Equals( (NullableBool) obj );
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.value;
        }

        /// <summary>
        /// Determines whether a <see cref="NullableBool"/> is <see cref="True"/>.
        /// </summary>
        /// <param name="value">A <see cref="NullableBool"/>.</param>
        /// <returns><b>true</b> if <paramref name="value"/> is <see cref="True"/>,
        /// false if it is <see cref="False"/> or <see cref="Null"/>.</returns>
        public static bool operator true( NullableBool value )
        {
            return value.value == valueTrue;
        }

        /// <summary>
        /// Determines whether a <see cref="NullableBool"/> is <see cref="False"/>.
        /// </summary>
        /// <param name="value">A <see cref="NullableBool"/>.</param>
        /// <returns><b>true</b> if <paramref name="value"/> is <see cref="False"/>,
        /// false if it is <see cref="True"/> or <see cref="Null"/>.</returns>
        public static bool operator false( NullableBool value )
        {
            return value.value == valueFalse;
        }

        /// <summary>
        /// Converts a <see cref="bool"/> into a <see cref="NullableBool"/>.
        /// </summary>
        /// <param name="value">A <see cref="bool"/>.</param>
        /// <returns><see cref="True"/> or <see cref="False"/>.</returns>
        public static implicit operator NullableBool( bool value )
        {
            return new NullableBool( value );
        }

        /// <summary>
        /// Converts a <see cref="NullableBool"/> into a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">A <see cref="NullableBool"/></param>
        /// <returns><b>true</b> if <paramref name="value"/> equals <see cref="NullableBool.True"/>,
        /// otherwise <b>false</b>.</returns>
        public static implicit operator bool( NullableBool value )
        {
            return value.value == valueTrue;
        }

        /// <summary>
        /// Determines whether two instances of <see cref="NullableBool"/> are equal.
        /// </summary>
        /// <param name="left">A <see cref="NullableBool"/>.</param>
        /// <param name="right">A <see cref="NullableBool"/>.</param>
        /// <returns><see cref="True"/> if both instances are equal, <see cref="False"/> if they are different or
        /// <see cref="Null"/> if any of them is <see cref="Null"/>.</returns>
        public static NullableBool operator ==( NullableBool left, NullableBool right )
        {
            if ( left.IsNull || right.IsNull )
            {
                return Null;
            }
            else
            {
                return left.value == right.value;
            }
        }


        /// <summary>
        /// Determines whether two instances of <see cref="NullableBool"/> are different.
        /// </summary>
        /// <param name="left">A <see cref="NullableBool"/>.</param>
        /// <param name="right">A <see cref="NullableBool"/>.</param>
        /// <returns><see cref="True"/> if instances are different, <see cref="False"/> if they are equal or
        /// <see cref="Null"/> if any of them is <see cref="Null"/>.</returns>
        public static NullableBool operator !=( NullableBool left, NullableBool right )
        {
            if ( left.IsNull || right.IsNull )
            {
                return Null;
            }
            else
            {
                return left.value != right.value;
            }
        }

        /// <summary>
        /// Determines whether any of two instances is <see cref="True"/>.
        /// </summary>
        /// <param name="left">A <see cref="NullableBool"/>.</param>
        /// <param name="right">A <see cref="NullableBool"/>.</param>
        /// <returns><see cref="True"/> if any instance is <see cref="True"/>, <see cref="False"/> if both instances
        /// is <see cref="False"/> or <see cref="Null"/> if any instance is <see cref="Null"/>.
        /// </returns>
        public static NullableBool operator |( NullableBool left, NullableBool right )
        {
            if ( left.value == valueTrue || right.value == valueTrue )
            {
                return True;
            }
            else if ( left.IsNull || right.IsNull )
            {
                return Null;
            }
            else
            {
                return False;
            }
        }


        /// <summary>
        /// Determines whether both instances are <see cref="True"/>.
        /// </summary>
        /// <param name="left">A <see cref="NullableBool"/>.</param>
        /// <param name="right">A <see cref="NullableBool"/>.</param>
        /// <returns><see cref="True"/> if both instances are <see cref="True"/>, <see cref="False"/> if any instance
        /// is <see cref="False"/> or <see cref="Null"/> if any instance is <see cref="Null"/>.
        /// </returns>
        public static NullableBool operator &( NullableBool left, NullableBool right )
        {
            if ( left.value == valueTrue && right.value == valueTrue )
            {
                return True;
            }
            else if ( left.IsNull || right.IsNull )
            {
                return Null;
            }
            else
            {
                return False;
            }
        }

        /// <summary>
        /// Inverses the value of a <see cref="NullableBool"/>.
        /// </summary>
        /// <param name="value">A <see cref="NullableBool"/>.</param>
        /// <returns><see cref="True"/> if the instance is <see cref="False"/>, <see cref="False"/> if it is <see cref="True"/>
        /// or <see cref="Null"/> if it is <see cref="Null"/>.</returns>
        public static NullableBool operator !( NullableBool value )
        {
            switch ( value.value )
            {
                case valueNull:
                    return Null;

                case valueTrue:
                    return False;

                case valueFalse:
                    return True;

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( value.value, "value.value" );
            }
        }


        /// <inheritdoc />
        public override string ToString()
        {
            switch ( this.value )
            {
                case valueNull:
                    return "Null";

                case valueTrue:
                    return "True";

                case valueFalse:
                    return "False";

                default:
                    throw ExceptionHelper.CreateInvalidEnumerationValueException( this.value, "this.value" );
            }
        }
    }
}