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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace PostSharp.Collections
{
    /// <summary>
    /// Associates many values to a single key.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    /// <remarks>
    /// <para>
    /// The default implementation of this <see cref="MultiDictionary{K,V}"/> uses
    /// a <see cref="Dictionary{TKey,TValue}"/>. 
    /// However, it is possible to use other implementations by deriving this
    /// class and overriding the <see cref="InternalCreateDictionary"/>, <see cref="InternalTryGetValue"/>
    /// methods. The underlying implementation can be accessed by derived classes 
    /// thanks to the <see cref="Implementation"/>
    /// property.
    /// </para>
    /// <para>
    /// The creation of the underlying dictionary is defered to the first element
    /// addition. This allows to have empty dictionaries at relatively low cost.
    /// </para>
    /// </remarks>
    [SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly" )]
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    [SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
    public class MultiDictionary<TKey, TValue> : IMultiDictionary<TKey, TValue>
    {
        #region Fields

        /// <summary>
        /// Underlying implementation of the first-level dictionary.
        /// </summary>
        private IDictionary<TKey, LinkedListHeader> implementation;

        /// <summary>
        /// Number of elements in this collection.
        /// </summary>
        private int count;

        /// <summary>
        /// Initial capacity of the first-level dictionary.
        /// </summary>
        private int initialDictionaryCapacity = 8;

        /// <summary>
        /// Comparer of equality of keys, or <b>null</b> if the default
        /// comparer has to be used.
        /// </summary>
        private readonly IEqualityComparer<TKey> comparer;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="MultiDictionary{K,V}"/> with default capacity.
        /// </summary>
        public MultiDictionary()
        {
        }


        /// <summary>
        /// Initializes a new <see cref="MultiDictionary{K,V}"/> and sets the
        /// initial capacity of the first-level dictionary (number of keys).
        /// </summary>
        /// <param name="capacity">Initial capacity of unique keys.</param>
        public MultiDictionary( int capacity )
        {
            this.initialDictionaryCapacity = capacity;
        }

        /// <summary>
        /// Initializes a new <see cref="MultiDictionary{K,V}"/>, sets the initial
        /// capacity of the first-level dictionary (number of keys) and
        /// gives a non-default comparer of equality of keys,
        /// </summary>
        /// <param name="capacity">Initial capacity of unique keys.</param>
        /// <param name="comparer">Equality comparer for keys.</param>
        public MultiDictionary( int capacity, IEqualityComparer<TKey> comparer )
        {
            this.initialDictionaryCapacity = capacity;
            this.comparer = comparer;
        }

        #region Implementation

        internal IDictionary<TKey, LinkedListHeader> Implementation
        {
            get { return this.implementation; }
        }

        /// <summary>
        /// Creates the first-level dictionary and returns it.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        /// <returns>A new instance of the first-level dictionary.</returns>
        internal virtual IDictionary<TKey, LinkedListHeader> InternalCreateDictionary( int capacity )
        {
            return new Dictionary<TKey, LinkedListHeader>( capacity, comparer );
        }

        /// <summary>
        /// Tries to retrieve a second-level list from the first-level dictionary.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="header">List.</param>
        /// <returns><b>true</b> if <paramref name="key"/> was found, otherwise <b>false</b>.</returns>
        internal virtual bool InternalTryGetValue( TKey key, out LinkedListHeader header )
        {
            return ( (Dictionary<TKey, LinkedListHeader>) this.implementation ).TryGetValue( key, out header );
        }

        /// <summary>
        /// Gets or sets the expected number of keys.
        /// </summary>
        public int Capacity
        {
            get { return this.initialDictionaryCapacity; }
            set
            {
                if ( this.implementation == null )
                {
                    this.initialDictionaryCapacity = value;
                    this.implementation = this.InternalCreateDictionary( value );
                }
            }
        }

        #endregion

        #region IDictionary<K,V> Members

        /// <inheritdoc />
        public void Add( TKey key, TValue value )
        {
            LinkedListHeader linkedListHeader;

            if ( this.implementation == null )
            {
                this.implementation = this.InternalCreateDictionary( this.initialDictionaryCapacity );
            }

            if ( !this.InternalTryGetValue( key, out linkedListHeader ) )
            {
                this.implementation.Add( key,
                                         new LinkedListHeader
                                             {Count = 1, Head = new SimpleLinkedListNode<TValue>( value, null )} );
            }
            else
            {
                linkedListHeader.Count++;
                linkedListHeader.Head = new SimpleLinkedListNode<TValue>( value, linkedListHeader.Head );
            }


            this.count++;
        }

        /// <summary>
        /// Adds a collection of values in the current dictionary.
        /// </summary>
        /// <param name="values">A collection of key-value pairs.</param>
        public void AddRange( ICollection<KeyValuePair<TKey, TValue>> values )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( values, "values" );

            #endregion

            this.Capacity = this.count + values.Count;

            foreach ( KeyValuePair<TKey, TValue> pair in values )
            {
                this.Add( pair );
            }
        }

        /// <summary>
        /// Merges a <see cref="MultiDictionary{TKey,TValue}"/> into the current instance.
        /// </summary>
        /// <param name="values">Dictionary to be merged</param>
        public void Merge( MultiDictionary<TKey, TValue> values )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( values, "values" );

            #endregion

            this.Capacity = this.count + values.count;

            foreach ( KeyValuePair<TKey, LinkedListHeader> pair in values.implementation )
            {
                LinkedListHeader currentNode;

                if ( this.implementation.TryGetValue( pair.Key, out currentNode ) )
                {
                    SimpleLinkedListNode<TValue>.Append( ref currentNode.Head, pair.Value.Head );
                    currentNode.Count += pair.Value.Count;
                }
                else
                {
                    this.implementation.Add( pair.Key,
                                             new LinkedListHeader
                                                 {Head = pair.Value.Head.Clone(), Count = pair.Value.Count} );
                }
            }
        }

        /// <inheritdoc />
        public bool ContainsKey( TKey key )
        {
            if ( this.implementation == null )
            {
                return false;
            }
            else
            {
                return this.implementation.ContainsKey( key );
            }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys
        {
            get
            {
                if ( this.implementation == null )
                {
                    return EmptyCollection<TKey>.GetInstance();
                }
                else
                {
                    return this.implementation.Keys;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<TValue> this[ TKey key ]
        {
            get
            {
                if ( this.implementation == null )
                {
                    return EmptyCollection<TValue>.GetInstance();
                }
                else
                {
                    LinkedListHeader header;
                    if ( this.InternalTryGetValue( key, out header ) )
                    {
                        return EnumerableWrapper<TValue>.GetInstance( header.Head );
                    }
                    else
                    {
                        return EmptyCollection<TValue>.GetInstance();
                    }
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        public void Add( KeyValuePair<TKey, TValue> item )
        {
            this.Add( item.Key, item.Value );
        }

        /// <inheritdoc />
        public void Clear()
        {
            if ( this.implementation != null )
            {
                this.implementation.Clear();
                this.count = 0;
            }
        }

        /// <inheritdoc />
        public bool Contains( KeyValuePair<TKey, TValue> item )
        {
            if ( this.implementation == null )
            {
                return false;
            }
            else
            {
                LinkedListHeader header;
                if ( this.InternalTryGetValue( item.Key, out header ) )
                {
                    SimpleLinkedListNode<TValue> cursor = header.Head;
                    while ( cursor != null )
                    {
                        if ( ( cursor.Value == null && item.Value == null ) || cursor.Value.Equals( item.Value ) )
                        {
                            return true;
                        }

                        cursor = cursor.Next;
                    }
                }

                return false;
            }
        }

        /// <inheritdoc />
        public void CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count
        {
            get { return this.count; }
        }

        /// <summary>
        /// Gets the number of items registered under a given key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>The number of items registered under <paramref name="key"/>.</returns>
        public int GetCountByKey( TKey key )
        {
            if ( this.implementation == null ) return 0;

            LinkedListHeader linkedListHeader;
            return !this.InternalTryGetValue( key, out linkedListHeader ) ? 0 : linkedListHeader.Count;
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool Remove( KeyValuePair<TKey, TValue> item )
        {
            return Remove( item.Key, item.Value );
        }

        /// <inheritdoc />
        public bool Remove( TKey key, TValue value )
        {
            if ( this.implementation == null )
            {
                return false;
            }
            else
            {
                LinkedListHeader header;
                if ( this.InternalTryGetValue( key, out header ) )
                {
                    SimpleLinkedListNode<TValue> previous = null;
                    SimpleLinkedListNode<TValue> cursor = header.Head;

                    while ( cursor != null )
                    {
                        if ( ( cursor.Value == null && value == null ) || cursor.Value.Equals( value ) )
                        {
                            if ( previous == null )
                            {
                                if ( cursor.Next == null )
                                {
                                    this.implementation.Remove( key );
                                }
                                else
                                {
                                    header.Head = cursor.Next;
                                    header.Count--;
                                }
                            }
                            else
                            {
                                previous.Next = cursor.Next;
                            }
                            this.count--;
                            return true;
                        }

                        previous = cursor;
                        cursor = cursor.Next;
                    }
                }
            }

            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if ( this.implementation != null )
            {
                foreach ( KeyValuePair<TKey, LinkedListHeader> listPair in this.implementation )
                {
                    SimpleLinkedListNode<TValue> cursor = listPair.Value.Head;

                    while ( cursor != null )
                    {
                        yield return new KeyValuePair<TKey, TValue>( listPair.Key, cursor.Value );
                        cursor = cursor.Next;
                    }
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        internal class LinkedListHeader
        {
            public int Count;
            public SimpleLinkedListNode<TValue> Head;
        }
    }

    /// <summary>
    /// Factory for <see cref="MultiDictionary{K,V}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly" )]
    public sealed class MultiDictionaryFactory<TKey, TValue> : ICollectionFactory<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Default instance, using the default equality comparer.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public static readonly MultiDictionaryFactory<TKey, TValue> Default = new MultiDictionaryFactory<TKey, TValue>();

        private readonly IEqualityComparer<TKey> comparer;

        private MultiDictionaryFactory()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MultiDictionaryFactory{K,V}"/> and specifies the equality comparer.
        /// </summary>
        /// <param name="comparer">Equality comparer.</param>
        public MultiDictionaryFactory( IEqualityComparer<TKey> comparer )
        {
            this.comparer = comparer;
        }

        #region ICollectionFactory<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        public ICollection<KeyValuePair<TKey, TValue>> CreateCollection()
        {
            return new MultiDictionary<TKey, TValue>( 8, this.comparer );
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<TKey, TValue>> CreateCollection( int capacity )
        {
            return new MultiDictionary<TKey, TValue>( capacity, this.comparer );
        }

        /// <inheritdoc />
        public void EnsureCapacity( ICollection<KeyValuePair<TKey, TValue>> collection, int capacity )
        {
        }

        #endregion
    }
}