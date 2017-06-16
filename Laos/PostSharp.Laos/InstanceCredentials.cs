#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/

#endregion

using System;
using System.Security;

namespace PostSharp.Laos
{
    /// <summary>
    /// Credentials that give access to 'protected' semantics.
    /// </summary>
    /// <remarks>
    /// A protected semantic is specified by an interface with public visibility, but
    /// code can only get this interface when it provides the semantics represented
    /// by the current type. Instance semantics are typically retrieved by an instance
    /// method <b>GetInstanceCredentials</b>, which has <i>protected</i> visibility.
    /// This method is implemented by PostSharp Laos automatically.
    /// </remarks>
#if !SL
    [Serializable]
#endif
    public struct InstanceCredentials : IEquatable<InstanceCredentials>
    {
        private static readonly Random random = new Random();

        private int value;

        /// <summary>
        /// Creates a new <see cref="InstanceCredentials"/>.
        /// </summary>
        /// <returns>A new <see cref="InstanceCredentials"/>.</returns>
        public static InstanceCredentials MakeNew()
        {
            InstanceCredentials instance = new InstanceCredentials();
            do
            {
                lock ( random )
                {
                    instance.value = random.Next();
                }
            } while ( instance.value == 0 );

            return instance;
        }

        /// <summary>
        /// Gets a null (or empty) <see cref="InstanceCredentials"/>/
        /// </summary>
#pragma warning disable 649
        public static readonly InstanceCredentials Null;
#pragma warning restore 649

        #region IEquatable<Credentials> Members

        /// <inheritdoc />
        public bool Equals( InstanceCredentials other )
        {
            return this.value == other.value;
        }

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            return this.Equals( (InstanceCredentials) obj );
        }

        /// <summary>
        /// Throws an exception if given credentials are not equal
        /// to the current one.
        /// </summary>
        /// <param name="others">Other credentials.</param>
        public void AssertEquals( InstanceCredentials others )
        {
            if ( !this.Equals( others ) )
            {
                throw new SecurityException( "Invalid instance credentials." );
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        /// <summary>
        /// Determines whether two <see cref="InstanceCredentials"/> are equal.
        /// </summary>
        /// <param name="left">An <see cref="InstanceCredentials"/> object.</param>
        /// <param name="right">An <see cref="InstanceCredentials"/> object.</param>
        /// <returns><b>true</b> if <paramref name="left"/> equals <paramref name="right"/>,
        /// otherwise <b>false</b>.</returns>
        public static bool operator ==( InstanceCredentials left, InstanceCredentials right )
        {
            return left.value == right.value;
        }

        /// <summary>
        /// Determines whether two <see cref="InstanceCredentials"/> are different.
        /// </summary>
        /// <param name="left">An <see cref="InstanceCredentials"/> object.</param>
        /// <param name="right">An <see cref="InstanceCredentials"/> object.</param>
        /// <returns><b>true</b> if <paramref name="left"/> is different than <paramref name="right"/>,
        /// otherwise <b>false</b>.</returns>
        public static bool operator !=( InstanceCredentials left, InstanceCredentials right )
        {
            return left.value != right.value;
        }

        #endregion

        /// <summary>
        /// Obsolete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        [Obsolete("Use LaosUtils.GetCurrentInstanceCredentials instead", true)]
        public static InstanceCredentials GetCredentials<T>( T instance )
        {
            throw new NotSupportedException("Use LaosUtils.GetCurrentInstanceCredentials instead.");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.value.ToString();
        }
    }
}