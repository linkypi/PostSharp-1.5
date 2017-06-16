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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections
{
    /// <summary>
    /// Efficient implementation of an <see cref="IEqualityComparer{T}"/> of strings.
    /// </summary>
    public sealed class FastStringComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public static readonly IEqualityComparer<string> Instance = new FastStringComparer();

        private FastStringComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals( string x, string y )
        {
            if ( ReferenceEquals( x, y ) )
            {
                return true;
            }

            if ( x.Length != y.Length )
            {
                return false;
            }

            return x.Equals( y, StringComparison.InvariantCulture );
        }

        /// <inheritdoc />
        public int GetHashCode( string obj )
        {
            return obj.GetHashCode();
        }
    }
}