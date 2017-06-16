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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections
{
    /// <summary>
    /// Specifies a dictionary possibly associating many items to a single key.
    /// </summary>
    /// <typeparam name="K">Type of keys.</typeparam>
    /// <typeparam name="V">Type of values.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly" )]
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    [SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
    public interface IMultiDictionary<K, V> : ICollection<KeyValuePair<K, V>>
    {
        /// <summary>
        /// Adds a key-value pair in the dictionary.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        void Add( K key, V value );

        /// <summary>
        /// Determines whether the dictionary contains at least one value
        /// associated to a given key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns><b>true</b> if the dictionary contains at least one value
        /// associated with <paramref name="key"/>, otherwise <b>false</b>.</returns>
        bool ContainsKey( K key );

        /// <summary>
        /// Gets the collection of keys.
        /// </summary>
        ICollection<K> Keys { get; }

        /// <summary>
        /// Gets the list of values associated with a given key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>The list of values associated with <paramref name="key"/>.</returns>
        IEnumerable<V> this[ K key ] { get; }
    }
}