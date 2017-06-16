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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace PostSharp.Collections
{
    /// <summary>
    /// <see cref="MultiDictionary{K,V}"/> where keys are ordered.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly" )]
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    [SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
    public class SortedMultiDictionary<TKey, TValue> : MultiDictionary<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Initializes a new <see cref="SortedMultiDictionary{K,V}"/> with default capacity.
        /// </summary>
        public SortedMultiDictionary()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SortedMultiDictionary{K,V}"/> and sets the initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity of unique keys.</param>
        public SortedMultiDictionary( int capacity ) : base( capacity )
        {
        }

        /// <summary>
        /// Tries to retrieve a second-level list from the first-level dictionary.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="header">List.</param>
        /// <returns><b>true</b> if <paramref name="key"/> was found, otherwise <b>false</b>.</returns>
        internal override bool InternalTryGetValue( TKey key, out LinkedListHeader header )
        {
            return
                ( (SortedDictionary<TKey, LinkedListHeader>) this.Implementation ).TryGetValue( key, out header );
        }


        /// <inheritdoc />
        internal override IDictionary<TKey, LinkedListHeader> InternalCreateDictionary(int capacity)
        {
            return new SortedDictionary<TKey, LinkedListHeader>();
        }
    }

    /// <summary>
    /// Factory for <see cref="SortedMultiDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly" )]
    public sealed class SortedMultiDictionaryFactory<TKey, TValue> : ICollectionFactory<KeyValuePair<TKey, TValue>>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public static readonly SortedMultiDictionaryFactory<TKey, TValue> Default =
            new SortedMultiDictionaryFactory<TKey, TValue>();

        private SortedMultiDictionaryFactory()
        {
        }

        #region ICollectionFactory<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        public ICollection<KeyValuePair<TKey, TValue>> CreateCollection()
        {
            return new SortedMultiDictionary<TKey, TValue>();
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<TKey, TValue>> CreateCollection( int capacity )
        {
            return new SortedMultiDictionary<TKey, TValue>( capacity );
        }

        /// <inheritdoc />
        public void EnsureCapacity( ICollection<KeyValuePair<TKey, TValue>> collection, int capacity )
        {
        }

        #endregion
    }
}