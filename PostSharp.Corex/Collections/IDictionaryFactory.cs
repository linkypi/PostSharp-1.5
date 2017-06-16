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

namespace PostSharp.Collections
{
    /// <summary>
    /// A dictionary factory creates instances of collections
    /// and allocates capacity.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    public interface IDictionaryFactory<TKey, TValue> : ICollectionFactory<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Creates a dictionary with default capacity.
        /// </summary>
        /// <returns>A dictionary.</returns>
        IDictionary<TKey, TValue> CreateDictionary();


        /// <summary>
        /// Creates a dictionary with given capacity.
        /// </summary>
        /// <param name="capacity">Initial dictionary capacity.</param>
        /// <returns>A dictionary.</returns>
        IDictionary<TKey, TValue> CreateDictionary( int capacity );

        /// <summary>
        /// Ensures that a dictionary has the given capacity.
        /// </summary>
        /// <param name="dictionary">A dictionary.</param>
        /// <param name="capacity">The required capacity.</param>
        void EnsureCapacity( IDictionary<TKey, TValue> dictionary, int capacity );
    }
}