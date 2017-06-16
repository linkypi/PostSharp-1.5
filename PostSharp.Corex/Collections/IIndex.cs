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
    /// Specifies the interface of an index.
    /// </summary>
    /// <typeparam name="K">Type of keys.</typeparam>
    /// <typeparam name="V">Type of values.</typeparam>
    /// <remarks>
    /// This interface has the advantage to specify the index without having to put
    /// constraint on generic type parameters. This eliminates many casting problems,
    /// for instance when defining the <see cref="IndexedCollection{T}"/> class.
    /// </remarks>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public interface IIndex<K, V> : ICollection<V>
    {
        /// <summary>
        /// Gets the first value associated to a given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated to <paramref name="key"/>,
        /// or the default value of <typeparamref name="V"/> if no value
        /// is associated to <paramref name="key"/>.</returns>
        V GetFirstValueByKey( K key );


        /// <summary>
        /// Tries to gets the first value associated to a given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value associated to <paramref name="key"/>,
        /// or the default value of <typeparamref name="V"/> if no value
        /// is associated to <paramref name="key"/>.</param>
        /// <returns><b>true</b> if some value was found, otherwise <b>false</b>.</returns>
        bool TryGetFirstValueByKey( K key, out V value );

        /// <summary>
        /// Gets the collection of values associated to a given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The collection of values associated to <paramref name="key"/>.</returns>
        IEnumerable<V> GetValuesByKey( K key );

        /// <summary>
        /// Removes an item from the index after its key has changed, by providing its old key.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="oldKey">The old key, under which the item was previously registered.</param>
        /// <returns></returns>
        bool Remove( V item, K oldKey );
    }
}