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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections
{
    /// <summary>
    /// Implementation of generic <see cref="IDictionary{K,V}"/> marshalled by reference.
    /// </summary>
    /// <typeparam name="V">Type of values.</typeparam>
    /// <typeparam name="K">Type of keys.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    [Serializable]
    public class MarshalByRefDictionary<K, V> : MarshalByRefObject, IDictionary<K, V>
    {
        /// <summary>
        /// Underlying implementation.
        /// </summary>
        private readonly IDictionary<K, V> implementation;


        /// <summary>
        /// Initializes a new <see cref="MarshalByRefDictionary{K,V}"/> with a <see cref="Dictionary{K,V}"/>
        /// as the underlying implementation.
        /// </summary>
        public MarshalByRefDictionary()
            : this( new Dictionary<K, V>() )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MarshalByRefDictionary{K,V}"/> and specifies the
        /// underlying implementation.
        /// </summary>
        /// <param name="implementation">The underlying implementation,
        /// or the collection to wrap.</param>
        public MarshalByRefDictionary( IDictionary<K, V> implementation )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( implementation, "implementation" );

            #endregion

            this.implementation = implementation;
        }

        #region IDictionary<K,V> Members

        /// <inheritdoc />
        public void Add( K key, V value )
        {
            this.implementation.Add( key, value );
        }

        /// <inheritdoc />
        public bool ContainsKey( K key )
        {
            return this.ContainsKey( key );
        }

        /// <inheritdoc />
        public ICollection<K> Keys { get { return new MarshalByRefCollection<K>( this.implementation.Keys ); } }

        /// <inheritdoc />
        public bool Remove( K key )
        {
            return this.implementation.Remove( key );
        }

        /// <inheritdoc />
        public bool TryGetValue( K key, out V value )
        {
            return this.implementation.TryGetValue( key, out value );
        }

        /// <inheritdoc />
        public ICollection<V> Values { get { return new MarshalByRefCollection<V>( this.implementation.Values ); } }

        /// <inheritdoc />
        public V this[ K key ] { get { return this.implementation[key]; } set { this.implementation[key] = value; } }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        public void Add( KeyValuePair<K, V> item )
        {
            this.implementation.Add( item );
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.implementation.Clear();
        }

        /// <inheritdoc />
        public bool Contains( KeyValuePair<K, V> item )
        {
            return this.implementation.Contains( item );
        }

        /// <inheritdoc />
        public void CopyTo( KeyValuePair<K, V>[] array, int arrayIndex )
        {
            this.implementation.CopyTo( array, arrayIndex );
        }

        /// <inheritdoc />
        public int Count { get { return this.implementation.Count; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return this.implementation.IsReadOnly; } }

        /// <inheritdoc />
        public bool Remove( KeyValuePair<K, V> item )
        {
            return this.implementation.Remove( item );
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this.implementation.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.implementation.GetEnumerator();
        }

        #endregion
    }
}