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
    /// Factory of <see cref="Dictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    public sealed class DictionaryFactory<TKey, TValue> : IDictionaryFactory<TKey, TValue>
    {
        /// <summary>
        /// Default instance (using the default equality comparer).
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )]
        public static readonly DictionaryFactory<TKey, TValue> Default = new DictionaryFactory<TKey, TValue>();

        private readonly IEqualityComparer<TKey> comparer;

        private DictionaryFactory()
        {
        }

        /// <inheritdoc />
        public DictionaryFactory( IEqualityComparer<TKey> comparer )
        {
            this.comparer = comparer;
        }

        /// <inheritdoc />
        public IDictionary<TKey, TValue> CreateDictionary()
        {
            return new Dictionary<TKey, TValue>( this.comparer );
        }

        /// <inheritdoc />
        public IDictionary<TKey, TValue> CreateDictionary( int capacity )
        {
            return new Dictionary<TKey, TValue>( capacity, this.comparer );
        }

        /// <inheritdoc />
        public void EnsureCapacity( IDictionary<TKey, TValue> dictionary, int capacity )
        {
        }

        #region ICollectionFactory<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        public ICollection<KeyValuePair<TKey, TValue>> CreateCollection()
        {
            return this.CreateDictionary();
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<TKey, TValue>> CreateCollection( int capacity )
        {
            return this.CreateDictionary( capacity );
        }

        /// <inheritdoc />
        public void EnsureCapacity( ICollection<KeyValuePair<TKey, TValue>> collection, int capacity )
        {
        }

        #endregion
    }
}
