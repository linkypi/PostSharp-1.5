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
    /// Sorted list optimized to situations when items
    /// are added in the right order (from the smallest to the greatest).
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public class AppendingSortedList<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : IComparable<TKey>, IEquatable<TKey>
        where TValue : class
    {
        private readonly List<KeyValuePair<TKey, TValue>> items;
        private TKey maxValue;

        /// <summary>
        /// Initializes a new <see cref="AppendingSortedList{K,V}"/> with default capacity.
        /// </summary>
        public AppendingSortedList()
        {
            this.items = new List<KeyValuePair<TKey, TValue>>();
        }

        /// <summary>
        /// Initializes a new <see cref="AppendingSortedList{K,V}"/> with given initial capacity.
        /// </summary>
        /// <param name="capacity">capacity</param>
        public AppendingSortedList( int capacity )
        {
            this.items = new List<KeyValuePair<TKey, TValue>>( capacity );
        }

        /// <summary>
        /// Gets ot sets the list capacity.
        /// </summary>
        public int Capacity { get { return this.items.Capacity; } set { this.items.Capacity = value; } }

        #region IDictionary<K,V> Members

        /// <inheritdoc />
        public void Add( TKey key, TValue value )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( key, "key" );

            #endregion

            KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>( key, value );

            int comparison = int.MinValue;

            if ( this.maxValue.Equals( default( TKey ) ) ||
                (comparison = key.CompareTo(this.maxValue)) >= 0)
            {
                ExceptionHelper.Core.AssertValidArgument(comparison != 0, "key", "DuplicateDictionaryKey");

                this.items.Add( pair );
                this.maxValue = key;
            }
            else
            {
                int position = this.items.BinarySearch( pair, Comparer.Instance );

                if ( position < 0 )
                {
                    position = ~position;
                    this.items.Insert( position, pair );
                }
                else
                {
                    throw ExceptionHelper.Core.CreateArgumentException( "key", "DuplicateDictionaryKey" );
                }
            }
        }

        /// <inheritdoc />
        public bool ContainsKey( TKey key )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( key, "key" );

            #endregion

            KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>( key, default( TValue ) );
            return this.items.BinarySearch( pair, Comparer.Instance ) >= 0;
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys { get { return new KeyCollection( this ); } }

        /// <inheritdoc />
        public bool Remove( TKey key )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( key, "key" );

            #endregion

            KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>( key, default( TValue ) );
            int position = this.items.BinarySearch( pair, Comparer.Instance );
            if ( position >= 0 )
            {
                this.items.RemoveAt( position );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryGetValue( TKey key, out TValue value )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( key, "key" );

            #endregion

            KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>( key, default( TValue ) );
            int position = this.items.BinarySearch( pair, Comparer.Instance );

            if ( position >= 0 )
            {
                value = this.items[position].Value;
                return true;
            }
            else
            {
                value = default( TValue );
                return false;
            }
        }

        /// <inheritdoc />
        public ICollection<TValue> Values { get { return new ValueCollection( this ); } }

        /// <inheritdoc />
        public TValue this[ TKey key ]
        {
            get
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( key, "key" );

                #endregion

                TValue value;
                if ( this.TryGetValue( key, out value ) )
                {
                    return value;
                }
                else
                {
                    throw new ArgumentException( "Cannot find an element with the given key." );
                }
            }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( key, "key" );

                #endregion

                KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>( key, value );
                int position = this.items.BinarySearch( pair, Comparer.Instance );

                if ( position >= 0 )
                {
                    // This is an existing item.
                    this.items[position] = pair;
                }
                else if ( this.maxValue.Equals( default( TKey ) ) || key.CompareTo( this.maxValue ) >= 0 )
                {
                    // This is a new item and it is at the end of the collection.
                    this.items.Add( pair );
                    this.maxValue = key;
                }
                else
                {
                    // This is an new item in the middle of the collection.
                    this.items.Insert( ~position, pair );
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
            this.items.Clear();
            this.maxValue = default( TKey );
        }

        /// <inheritdoc />
        public bool Contains( KeyValuePair<TKey, TValue> item )
        {
            TValue value;
            if ( this.TryGetValue( item.Key, out value ) )
            {
                if ( value == null )
                    return item.Value == null;
                else 
                    return value.Equals( item.Value );
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count { get { return this.items.Count; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return false; } }

        /// <inheritdoc />
        public bool Remove( KeyValuePair<TKey, TValue> item )
        {
            int position = this.items.BinarySearch( item, Comparer.Instance );

            if ( position >= 0 )
            {
                TValue value = this.items[position].Value;
                if ( ( item.Value == null && value == null ) || item.Value.Equals( value ) )
                {
                    this.items.RemoveAt( position );
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for ( int i = 0 ; i < this.items.Count ; i++ )
            {
                yield return this.items[i];
            }
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        private class Comparer : IComparer<KeyValuePair<TKey, TValue>>
        {
            public static readonly Comparer Instance = new Comparer();

            private Comparer()
            {
            }


            public int Compare( KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y )
            {
                return x.Key.CompareTo( y.Key );
            }
        }

        private class KeyCollection : ICollection<TKey>
        {
            private readonly AppendingSortedList<TKey, TValue> parent;

            public KeyCollection( AppendingSortedList<TKey, TValue> parent )
            {
                this.parent = parent;
            }

            #region ICollection<K> Members

            public void Add( TKey item )
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains( TKey item )
            {
                throw new NotSupportedException();
            }

            public void CopyTo( TKey[] array, int arrayIndex )
            {
                throw new NotImplementedException();
            }

            public int Count { get { return this.parent.items.Count; } }

            public bool IsReadOnly { get { return true; } }

            public bool Remove( TKey item )
            {
                throw new NotSupportedException();
            }

            #endregion

            #region IEnumerable<K> Members

            public IEnumerator<TKey> GetEnumerator()
            {
                foreach ( KeyValuePair<TKey, TValue> item in this.parent.items )
                {
                    yield return item.Key;
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        private class ValueCollection : ICollection<TValue>
        {
            private readonly AppendingSortedList<TKey, TValue> parent;

            public ValueCollection( AppendingSortedList<TKey, TValue> parent )
            {
                this.parent = parent;
            }

            #region ICollection<V> Members

            public void Add( TValue item )
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains( TValue item )
            {
                throw new NotSupportedException();
            }

            public void CopyTo( TValue[] array, int arrayIndex )
            {
                throw new NotImplementedException();
            }

            public int Count { get { return this.parent.items.Count; } }

            public bool IsReadOnly { get { return true; } }

            public bool Remove( TValue item )
            {
                throw new NotSupportedException();
            }

            #endregion

            #region IEnumerable<V> Members

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach ( KeyValuePair<TKey, TValue> item in this.parent.items )
                {
                    yield return item.Value;
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }
    }

    /// <summary>
    /// Factory of <see cref="AppendingSortedList{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    public sealed class AppendingSortedListFactory<TKey, TValue> :
        IDictionaryFactory<TKey, TValue>
        where TKey : IComparable<TKey>, IEquatable<TKey>
        where TValue : class

    {
        /// <summary>
        /// Default instance.
        /// </summary>
        [SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )] public static readonly AppendingSortedListFactory<TKey, TValue> Default =
            new AppendingSortedListFactory<TKey, TValue>();

        private AppendingSortedListFactory()
        {
        }

        private static AppendingSortedList<TKey, TValue> CreateDictionary()
        {
            return new AppendingSortedList<TKey, TValue>();
        }

        private static AppendingSortedList<TKey, TValue> CreateDictionary( int capacity )
        {
            return new AppendingSortedList<TKey, TValue>( capacity );
        }

        private static void EnsureCapacity( object collection, int capacity )
        {
            ( (AppendingSortedList<TKey, TValue>) collection ).Capacity = capacity;
        }

        /// <inheritdoc />
        IDictionary<TKey, TValue> IDictionaryFactory<TKey, TValue>.CreateDictionary()
        {
            return CreateDictionary();
        }

        /// <inheritdoc />
        IDictionary<TKey, TValue> IDictionaryFactory<TKey, TValue>.CreateDictionary( int capacity )
        {
            return CreateDictionary( capacity );
        }

        /// <inheritdoc />
        void IDictionaryFactory<TKey, TValue>.EnsureCapacity( IDictionary<TKey, TValue> collection, int capacity )
        {
            EnsureCapacity( collection, capacity );
        }

        #region ICollectionFactory<KeyValuePair<K,V>> Members

        /// <inheritdoc />
        ICollection<KeyValuePair<TKey, TValue>> ICollectionFactory<KeyValuePair<TKey, TValue>>.CreateCollection()
        {
            return CreateDictionary();
        }

        /// <inheritdoc />
        ICollection<KeyValuePair<TKey, TValue>> ICollectionFactory<KeyValuePair<TKey, TValue>>.CreateCollection(
            int capacity )
        {
            return CreateDictionary( capacity );
        }

        /// <inheritdoc />
        void ICollectionFactory<KeyValuePair<TKey, TValue>>.EnsureCapacity(
            ICollection<KeyValuePair<TKey, TValue>> collection, int capacity )
        {
            EnsureCapacity( collection, capacity );
        }

        #endregion
    }
}