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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace PostSharp.Collections
{
    /// <summary>
    /// Dictionary of objects indexed by a property of these objects. Objects
    /// are automatically reindexed when the key property changes.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    /// <remarks>
    /// If a collection of items should have many indexes (many indexed properties),
    /// it is preferable to use the <see cref="IndexedCollection{T}"/> class.
    /// </remarks>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public abstract class Index<TKey, TValue> : IIndex<TKey, TValue>
        where TValue : class
    {
        private ICollectionFactory<KeyValuePair<TKey, TValue>> dictionaryFactory;

        /// <summary>
        /// Underlying implementation (either an <see cref="IDictionary"/>,
        /// either an <see cref="IMultiDictionary{K,V}"/>).
        /// </summary>
        private ICollection<KeyValuePair<TKey, TValue>> dictionary;

        private int capacity = 4;

        /// <summary>
        /// Initializes a new <see cref="Index{K,V}"/>.
        /// </summary>
        /// <param name="collectionFactory">Factory of the underlying implentation of this index.</param>
        protected Index( ICollectionFactory<KeyValuePair<TKey, TValue>> collectionFactory )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( collectionFactory, "collectionFactory" );

            #endregion

            this.dictionaryFactory = collectionFactory;
        }


        /// <summary>
        /// Gets or sets the index capacity.
        /// </summary>
        public int Capacity
        {
            get { return capacity; }
            set
            {
                #region Preconditions

                if ( value < 0 )
                {
                    throw new ArgumentOutOfRangeException( "value" );
                }

                #endregion

                capacity = value;

                if ( this.dictionary != null )
                {
                    this.dictionaryFactory.EnsureCapacity( this.dictionary, value );
                }
            }
        }


        private void EnsureDictionaryAllocated()
        {
            if ( this.dictionary == null )
            {
                this.dictionary = this.dictionaryFactory.CreateCollection( this.capacity );
            }
        }

        /// <summary>
        /// Notifies the current <see cref="Index{TKey,TValue}"/> that the key of an item contained
        /// in the index has changed.
        /// </summary>
        /// <param name="oldKey">Old key.</param>
        /// <param name="newKey">New key.</param>
        /// <param name="value">Item whose key has changed.</param>
        protected void NotifyKeyChanged( TKey oldKey, TKey newKey, TValue value )
        {
            if ( oldKey != null )
            {
                if ( !this.dictionary.Remove( new KeyValuePair<TKey, TValue>( oldKey, value ) ) )
                {
                    throw ExceptionHelper.Core.CreateInvalidOperationException( "OldItemNotFoundInIndex", oldKey );
                }
            }
            this.dictionary.Add( new KeyValuePair<TKey, TValue>( newKey, value ) );
        }

        /// <summary>
        /// Gets the key of an item.
        /// </summary>
        /// <param name="value">Item.</param>
        /// <returns>The key of <paramref name="value"/>.</returns>
        protected abstract TKey GetItemKey( TValue value );

        #region ICollection<V> Members

        /// <inheritdoc />
        public void Add( TValue item )
        {
            this.AssertNotDisposed();
            this.EnsureDictionaryAllocated();

            TKey key = this.GetItemKey( item );
            if ( key != null )
            {
                this.dictionary.Add( new KeyValuePair<TKey, TValue>( key, item ) );
            }
            else
            {
                throw new ArgumentException( "Cannot insert an item with a null key.", "item" );
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.AssertNotDisposed();

            if ( this.dictionary != null )
            {
                this.dictionary.Clear();
            }
        }

        /// <inheritdoc />
        public bool Contains( TValue item )
        {
            this.AssertNotDisposed();
            if ( this.dictionary != null )
            {
                return
                    this.dictionary.Contains(
                        new KeyValuePair<TKey, TValue>( this.GetItemKey( item ), item ) );
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void CopyTo( TValue[] array, int arrayIndex )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( array, "array" );
            int i = 0;
            foreach ( KeyValuePair<TKey, TValue> item in this.dictionary )
            {
                array[i] = item.Value;
                i++;
            }

            #endregion
        }

        /// <inheritdoc />
        public int Count
        {
            get { return this.dictionary == null ? 0 : this.dictionary.Count; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return false; }
        }


        /// <inheritdoc />
        public bool Remove( TValue item )
        {
            this.AssertNotDisposed();
            if ( this.dictionary != null )
            {
                if (
                    this.dictionary.Remove(
                        new KeyValuePair<TKey, TValue>( this.GetItemKey( item ), item ) ) )
                {
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

        #region IEnumerable<V> Members

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator()
        {
            this.AssertNotDisposed();

            if ( this.dictionary != null )
            {
                return new IndexEnumerator( this.dictionary.GetEnumerator() );
            }
            else
            {
                return EmptyEnumerator<TValue>.GetInstance();
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <inheritdoc />
        public IEnumerable<TValue> GetValuesByKey( TKey key )
        {
            this.AssertNotDisposed();

            if ( this.dictionary != null )
            {
                IMultiDictionary<TKey, TValue> multiDictionary = this.dictionary as IMultiDictionary<TKey, TValue>;

                if ( multiDictionary != null )
                {
                    return multiDictionary[key];
                }
                else
                {
                    IDictionary<TKey, TValue> singleDictionary = (IDictionary<TKey, TValue>) this.dictionary;

                    if ( singleDictionary.ContainsKey( key ) )
                    {
                        return new Singleton<TValue>( singleDictionary[key], true );
                    }
                    else
                    {
                        return EmptyCollection<TValue>.GetInstance();
                    }
                }
            }
            else
            {
                return EmptyCollection<TValue>.GetInstance();
            }
        }

        /// <inheritdoc />
        public bool Remove( TValue item, TKey oldKey )
        {
            this.AssertNotDisposed();

            if ( this.dictionary != null )
            {
                if (
                    this.dictionary.Remove( new KeyValuePair<TKey, TValue>( oldKey, item ) ) )
                {
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

        /// <inheritdoc />
        public TValue GetFirstValueByKey( TKey key )
        {
            TValue value;
            this.TryGetFirstValueByKey( key, out value );
            return value;
        }

        /// <inheritdoc />
        public bool TryGetFirstValueByKey( TKey key, out TValue value )
        {
            this.AssertNotDisposed();

            if ( this.dictionary != null )
            {
                IMultiDictionary<TKey, TValue> multiDictionary = this.dictionary as IMultiDictionary<TKey, TValue>;

                if ( multiDictionary != null )
                {
                    IEnumerator<TValue> enumerator = multiDictionary[key].GetEnumerator();
                    if ( enumerator.MoveNext() )
                    {
                        value = enumerator.Current;
                        return true;
                    }
                }
                else
                {
                    IDictionary<TKey, TValue> singleDictionary = (IDictionary<TKey, TValue>) this.dictionary;

                    return singleDictionary.TryGetValue( key, out value );
                }
            }

            value = default( TValue );
            return false;
        }

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        /// <param name="disposing"><b>true</b> if the method is called because of
        /// an explicit call of <see cref="Dispose()"/>, <b>false</b> if it is
        /// called inside the destructor.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this.Clear();
            }
        }


        /// <summary>
        /// Throws an exception if the current instance has been disposed.
        /// </summary>
        [Conditional( "ASSERT" )]
        protected void AssertNotDisposed()
        {
            if ( this.dictionaryFactory == null )
            {
                throw new ObjectDisposedException( this.GetType().FullName );
            }
        }

        /// <summary>
        /// Determines whether the current instance has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return this.dictionaryFactory == null; }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( this.dictionaryFactory != null )
            {
                this.Dispose( true );
                this.dictionaryFactory = null;
                GC.SuppressFinalize( this );
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Index()
        {
            this.Dispose( false );
        }

        private sealed class IndexEnumerator : IEnumerator<TValue>
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> backend;

            public IndexEnumerator( IEnumerator<KeyValuePair<TKey, TValue>> backend )
            {
                this.backend = backend;
            }

            #region IEnumerator<TValue> Members

            public TValue Current
            {
                get { return this.backend.Current.Value; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                this.backend.Dispose();
            }

            #endregion

            #region IEnumerator Members

            public bool MoveNext()
            {
                return this.backend.MoveNext();
            }

            public void Reset()
            {
                this.backend.Reset();
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            #endregion
        }
    }

    /// <summary>
    /// Factory for <see cref="Index{K,V}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    public abstract class IndexFactory<TKey, TValue> : ICollectionFactory<TValue>
        where TValue : class
    {
        #region ICollectionFactory<T> Members

        /// <inheritdoc />
        public abstract ICollection<TValue> CreateCollection();

        /// <inheritdoc />
        public virtual ICollection<TValue> CreateCollection( int capacity )
        {
            ICollection<TValue> collection = this.CreateCollection();
            this.EnsureCapacity( collection, capacity );
            return collection;
        }

        /// <inheritdoc />
        public void EnsureCapacity( ICollection<TValue> collection, int capacity )
        {
            ( (Index<TKey, TValue>) collection ).Capacity = capacity;
        }

        #endregion
    }
}