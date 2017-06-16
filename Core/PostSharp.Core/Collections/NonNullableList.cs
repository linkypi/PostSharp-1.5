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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections
{
    /// <summary>
    /// Wraps a list so that it is forbidden to add null items.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public class NonNullableList<T> : IList<T>
    {
        private readonly List<T> implementation;

        /// <summary>
        /// Initializes a new <see cref="NonNullableList{T}"/> with default capacity.
        /// </summary>
        public NonNullableList()
        {
            this.implementation = new List<T>();
        }

        /// <summary>
        /// Initializes a new <see cref="NonNullableList{T}"/> with given initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        public NonNullableList( int capacity )
        {
            this.implementation = new List<T>( capacity );
        }

        /// <summary>
        /// Initializes a new <see cref="NonNullableList{T}"/> and copies the content of 
        /// an existing collection.
        /// </summary>
        /// <param name="items">The collection to be copied.</param>
        public NonNullableList( ICollection<T> items )
        {
            ExceptionHelper.AssertArgumentNotNull( items, "items" );
            this.implementation = new List<T>( items.Count );
            this.AddRange( items );
        }

        /// <summary>
        /// Adds a collection of items in the list.
        /// </summary>
        /// <param name="items">The collection of items to be added.</param>
        public void AddRange( ICollection<T> items )
        {
            ExceptionHelper.AssertArgumentNotNull( items, "items" );
            this.implementation.Capacity = this.implementation.Count + items.Count;

            IEnumerator<T> enumerator = items.GetEnumerator();
            while ( enumerator.MoveNext() )
            {
                T item = enumerator.Current;
                ExceptionHelper.AssertArgumentNotNull( item, "one of the items" );
                this.implementation.Add( item );
            }
        }

        /// <summary>
        /// Gets or sets the capacity of the current list.
        /// </summary>
        public int Capacity
        {
            get { return this.implementation.Capacity; }
            set { this.implementation.Capacity = value; }
        }

        #region ICollection<T> Members

        /// <inheritdoc />
        public void Add( T item )
        {
            ExceptionHelper.AssertArgumentNotNull( item, "item" );
            this.implementation.Add( item );
        }

        /// <summary>
        /// Adds a set of items to the current collection.
        /// </summary>
        /// <param name="items">A set of items.</param>
        public void AddRange( IEnumerable<T> items )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( items, "items" );

            #endregion

            foreach ( T item in items )
            {
                ExceptionHelper.AssertArgumentNotNull( item, "item" );
                this.implementation.Add( item );
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.implementation.Clear();
        }

        /// <inheritdoc />
        public bool Contains( T item )
        {
            ExceptionHelper.AssertArgumentNotNull( item, "item" );
            return this.implementation.Contains( item );
        }

        /// <inheritdoc />
        public void CopyTo( T[] array, int arrayIndex )
        {
            this.implementation.CopyTo( array, arrayIndex );
        }

        /// <inheritdoc />
        public int Count
        {
            get { return this.implementation.Count; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool Remove( T item )
        {
            ExceptionHelper.AssertArgumentNotNull( item, "item" );
            return this.implementation.Remove( item );
        }

        #endregion

        #region IEnumerable<T> Members

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return this.implementation.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IList<T> Members

        /// <inheritdoc />
        public int IndexOf( T item )
        {
            ExceptionHelper.AssertArgumentNotNull( item, "item" );
            return this.implementation.IndexOf( item );
        }

        /// <inheritdoc />
        public void Insert( int index, T item )
        {
            ExceptionHelper.AssertArgumentNotNull( item, "item" );
            this.implementation.Insert( index, item );
        }

        /// <inheritdoc />
        public void RemoveAt( int index )
        {
            this.implementation.RemoveAt( index );
        }

        /// <inheritdoc />
        public T this[ int index ]
        {
            get { return this.implementation[index]; }
            set
            {
                ExceptionHelper.AssertArgumentNotNull( value, "value" );
                this.implementation[index] = value;
            }
        }

        #endregion
    }
}