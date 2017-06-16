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
using System.Globalization;

#endregion

namespace PostSharp.Collections
{
    /// <summary>
    /// Collection of objects indexed by many properties of these objects. Objects
    /// are automatically reindexed when the key property changes.
    /// </summary>
    /// <typeparam name="T">Type of indexed values.</typeparam>
    public abstract class IndexedCollection<T> : ICollection<T>, IDisposable
    {
        /// <summary>
        /// Collection of indexes.
        /// </summary>
        private ICollection<T>[] indexes;

        private ICollectionFactory<T>[] indexFactories;
        private int capacity;

        /// <summary>
        /// Initializes a new <see cref="IndexedCollection{V}"/> with an initial capacity.
        /// </summary>
        /// <param name="indexFactories">Array of factories for individual indexes.</param>
        /// <param name="capacity">Initial capacity.</param>
        protected IndexedCollection( ICollectionFactory<T>[] indexFactories, int capacity )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( indexFactories, "indexFactories" );
            this.indexFactories = indexFactories;
            this.capacity = capacity;

#if ASSERT
            for ( int i = 0; i < indexFactories.Length; i++ )
            {
                if ( indexFactories[i] == null )
                {
                    throw new ArgumentNullException( string.Format( CultureInfo.InvariantCulture,
                                                                    "indexFactories[{0}]", i ) );
                }
            }
#endif

            // Indexes will be created lazily.

            #endregion
        }


        /// <summary>
        /// Gets or sets the index capacity.
        /// </summary>
        public int Capacity
        {
            get { return this.capacity; }
            set
            {
                if ( value > this.capacity )
                {
                    this.capacity = value;
                    if ( this.indexes != null )
                    {
                        for ( int i = 0; i < this.indexes.Length; i++ )
                        {
                            if ( this.indexes[i] != null )
                            {
                                this.indexFactories[i].EnsureCapacity( this.indexes[i], value );
                            }
                        }
                    }
                }
            }
        }

        #region ICollection<V> Members

        /// <inheritdoc />
        public void Add( T item )
        {
            #region Preconditions

            this.AssertNotDisposed();

            #endregion

            CreateFirstIndex();

            for ( int i = 0; i < this.indexes.Length; i++ )
            {
                ICollection<T> index = this.indexes[i];

                if ( index != null )
                {
                    index.Add( item );
                }
            }
        }

        private void CreateFirstIndex()
        {
            if ( this.indexes == null )
            {
                this.indexes = new ICollection<T>[indexFactories.Length];
                this.indexes[0] = this.indexFactories[0].CreateCollection( this.capacity );
            }
        }


        /// <inheritdoc />
        public void Clear()
        {
            this.AssertNotDisposed();
            if ( this.indexes != null )
            {
                for ( int i = 0; i < this.indexes.Length; i++ )
                {
                    ICollection<T> index = this.indexes[i];
                    if ( index != null )
                    {
                        index.Clear();
                    }
                }
            }
        }

        /// <inheritdoc />
        public bool Contains( T item )
        {
            this.AssertNotDisposed();

            if ( this.indexes != null )
            {
                return this.indexes[0].Contains( item );
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void CopyTo( T[] array, int arrayIndex )
        {
            this.AssertNotDisposed();
            if ( this.indexes != null )
            {
                this.indexes[0].CopyTo( array, arrayIndex );
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                this.AssertNotDisposed();
                if ( this.indexes != null )
                {
                    return this.indexes[0].Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return false; }
        }


        /// <inheritdoc />
        public bool Remove( T item )
        {
            this.AssertNotDisposed();

            int removed = 0;

            if ( this.indexes != null )
            {
                for ( int i = 0; i < this.indexes.Length; i++ )
                {
                    ICollection<T> index = this.indexes[i];
                    if ( index != null )
                    {
                        if ( index.Remove( item ) )
                        {
                            removed++;
                        }
                    }
                }
            }

            if ( removed == 0 )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region IEnumerable<V> Members

        /// <inheritdoc />
        public virtual IEnumerator<T> GetEnumerator()
        {
            this.AssertNotDisposed();
            if ( this.indexes == null )
            {
                return EmptyEnumerator<T>.GetInstance();
            }
            else
            {
                return this.indexes[0].GetEnumerator();
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        private ICollection<T> GetIndex( int indexOrdinal, bool create )
        {
            if ( this.indexes == null )
            {
                return null;
            }

            ICollection<T> index = this.indexes[indexOrdinal];
            if ( index == null && create )
            {
                // This index has not yet been built. Build it now.
                index = indexFactories[indexOrdinal].CreateCollection( this.indexes[0].Count );
                foreach ( T value in this.indexes[0] )
                {
                    index.Add( value );
                }
                this.indexes[indexOrdinal] = index;
            }

            return index;
        }

        /// <summary>
        /// Gets all values in the collection ordered by an index known by its ordinal.
        /// </summary>
        /// <param name="indexOrdinal">Index ordinal.</param>
        /// <returns>An ordered collection of items of type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This makes sense only if the implementation of the index of this property
        /// is an ordered dictionary.
        /// </remarks>
        protected IEnumerable<T> GetOrderedValues( int indexOrdinal )
        {
            this.AssertNotDisposed();
            ICollection<T> index = this.GetIndex( indexOrdinal, true );

            if ( index == null )
            {
                return EmptyCollection<T>.GetInstance();
            }


            return new ReadOnlyCollectionWrapper<T>( index );
        }

        /// <summary>
        /// Gets the collection of items whose a given property equals a given value. The
        /// property is known by the ordinal of its associated index.
        /// </summary>
        /// <typeparam name="K">Type of the indexed property.</typeparam>
        /// <param name="indexOrdinal">Ordinal of the index associated to this property.</param>
        /// <param name="key">Value of the indexed property for which items have to be returned.</param>
        /// <returns>The collection of items whose property at position <paramref name="indexOrdinal"/>
        /// equals <paramref name="key"/>.</returns>
        protected IEnumerable<T> GetValuesByKey<K>( int indexOrdinal, K key )
        {
            this.AssertNotDisposed();

            IIndex<K, T> index = (IIndex<K, T>) this.GetIndex( indexOrdinal, true );

            if ( index == null )
            {
                return EmptyCollection<T>.GetInstance();
            }

            return index.GetValuesByKey( key );
        }


        /// <summary>
        /// Tries to get the first item whose a given property equals a given value.
        /// The property is known by the ordinal of its associated index.
        /// </summary>
        /// <typeparam name="K">Type of the indexed property.</typeparam>
        /// <param name="indexOrdinal">Ordinal of the indexed property.</param>
        /// <param name="key">Value of the indexed property for which the first item to be returned.</param>
        /// <param name="value">Value of the first item whose property value is <paramref name="key"/>.</param>
        /// <returns><b>true</b> if at least one item corresponded to this criterion, otherwise <b>false</b>.</returns>
        protected bool TryGetFirstValueByKey<K>( int indexOrdinal, K key, out T value )
        {
            this.AssertNotDisposed();

            IIndex<K, T> index = (IIndex<K, T>) this.GetIndex( indexOrdinal, true );

            if ( index == null )
            {
                value = default( T );
                return false;
            }
            else
            {
                return index.TryGetFirstValueByKey( key, out value );
            }
        }

        /// <summary>
        /// Method invoked when an indexed property of an item of the current collection has changed.
        /// </summary>
        /// <typeparam name="K">Type of the indexed property.</typeparam>
        /// <param name="indexOrdinal">Ordinal of the index corresponding to the property.</param>
        /// <param name="item">Item.</param>
        /// <param name="oldKey">Value of the indexed property before the change.</param>
        protected void OnItemPropertyChanged<K>( int indexOrdinal, T item, K oldKey )
        {
            IIndex<K, T> index = (IIndex<K, T>) this.GetIndex( indexOrdinal, false );
            if ( index == null )
                return;

            index.Remove( item, oldKey );
            index.Add( item );
        }

        #region IDisposable Members

        private void AssertNotDisposed()
        {
            if ( this.indexFactories == null )
            {
                throw new ObjectDisposedException( this.GetType().FullName );
            }
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
                if ( this.indexes != null )
                {
                    foreach ( IDisposable index in this.indexes )
                    {
                        index.Dispose();
                    }
                    this.indexes = null;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( this.indexFactories != null )
            {
                this.Dispose( true );
                this.indexFactories = null;
                GC.SuppressFinalize( this );
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~IndexedCollection()
        {
            this.Dispose( false );
        }

        /// <summary>
        /// Determines whether the collection has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return this.indexFactories == null; }
        }

        #endregion
    }


    /// <summary>
    /// Factory for <see cref="IndexedCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public abstract class IndexedCollectionFactory<T> : ICollectionFactory<T>
    {
        #region ICollectionFactory<T> Members

        /// <inheritdoc />
        public virtual ICollection<T> CreateCollection()
        {
            return this.CreateCollection( 4 );
        }

        /// <inheritdoc />
        ICollection<T> ICollectionFactory<T>.CreateCollection( int capacity )
        {
            return this.CreateCollection( capacity );
        }

        /// <summary>
        /// Creates an <see cref="IndexedCollection{T}"/>.
        /// </summary>
        /// <param name="capacity">Initial capacity of the collection.</param>
        /// <returns>A new <see cref="IndexedCollection{T}"/> with the given initial capacity.</returns>
        protected abstract IndexedCollection<T> CreateCollection( int capacity );


        /// <inheritdoc />
        public void EnsureCapacity( ICollection<T> collection, int capacity )
        {
            ( (IndexedCollection<T>) collection ).Capacity = capacity;
        }

        #endregion
    }
}