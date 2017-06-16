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
    /// Collection with uniqueness constraint and with read complexity in O(1).
    /// </summary>
    /// <typeparam name="T">Name of elements.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    [Serializable]
    public class Set<T> : ICollection<T>
    {
        /// <summary>
        /// Underlying dictionary.
        /// </summary>
        private readonly Dictionary<T, byte> dictionary;

        /// <summary>
        /// Initializes a new <see cref="Set{T}"/> and specifies the initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        public Set( int capacity ) : this( capacity, null )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Set{T}"/> with default capacity.
        /// </summary>
        public Set() : this( 16, null )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="Set{T}"/> and specifies the initial capacity
        /// and the comparer.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        /// <param name="comparer">Comparer.</param>
        public Set( int capacity, IEqualityComparer<T> comparer )
        {
            this.dictionary = new Dictionary<T, byte>( capacity, comparer );
        }

        #region ICollection<T> Members

        /// <inheritdoc />
        public void Add( T item )
        {
            this.dictionary.Add( item, 0 );
        }

        /// <summary>
        /// Adds a range of new items to the current set.
        /// </summary>
        /// <param name="items">Collection of items to be added. None of these
        /// items can be already be a part of the collection.</param>
        public void AddRange( IEnumerable<T> items )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( items, "items" );

            #endregion

            foreach ( T item in items )
            {
                this.dictionary.Add( item, 0 );
            }
        }

        /// <summary>
        /// Adds an item to the set if this set does not yet contain this item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns><b>true</b> if the item was added, or <b>false</b> if the
        /// item was already present.</returns>
        public bool AddIfAbsent( T item )
        {
            if ( !this.dictionary.ContainsKey( item ) )
            {
                this.dictionary.Add( item, 0 );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Merge another collection in the current set.
        /// </summary>
        /// <param name="items">Collection of items (eventually with
        /// duplicates or items already present in the current collection).</param>
        public void Union( IEnumerable<T> items )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( items, "items" );

            #endregion

            if ( this == items )
                return;

            foreach ( T item in items )
            {
                this.dictionary[item] = 0;
            }
        }

        /// <summary>
        /// Removes a set of items from the current set.
        /// </summary>
        /// <param name="items">Collection of items to be removed from the current set.</param>
        public void Difference( IEnumerable<T> items )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( items, "items" );

            #endregion

            if ( this == items )
            {
                this.Clear();
            }
            else
            {
                foreach ( T item in items )
                {
                    this.dictionary.Remove( item );
                }
            }
        }


        /// <inheritdoc />
        public void Clear()
        {
            this.dictionary.Clear();
        }

        /// <inheritdoc />
        public bool Contains( T item )
        {
            return this.dictionary.ContainsKey( item );
        }

        /// <inheritdoc />
        public void CopyTo( T[] array, int arrayIndex )
        {
            this.dictionary.Keys.CopyTo( array, arrayIndex );
        }

        /// <inheritdoc />
        public int Count
        {
            get { return this.dictionary.Count; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool Remove( T item )
        {
            return this.dictionary.Remove( item );
        }

        #endregion

        #region IEnumerable<T> Members

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return this.dictionary.Keys.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets the union of the current set with another.
        /// </summary>
        /// <param name="set">Second set.</param>
        /// <returns>A new set which the union between the current set and <paramref name="set"/>.</returns>
        public Set<T> GetUnion( Set<T> set )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( set, "set" );

            #endregion

            Set<T> result = this.CreateSiblingInstance( this.Count + set.Count );
            foreach ( T item in this )
            {
                result.Add( item );
            }
            foreach ( T item in set )
            {
                result.AddIfAbsent( item );
            }

            return result;
        }

        /// <summary>
        /// Gets the intersection of the current set with another.
        /// </summary>
        /// <param name="set">Second set.</param>
        /// <returns>A new set which the intersection between the current set and <paramref name="set"/>.</returns>
        public Set<T> GetIntersection( Set<T> set )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( set, "set" );

            #endregion

            int capacity;

            Set<T> little;
            Set<T> big;

            if ( this.Count > set.Count )
            {
                capacity = set.Count;
                little = set;
                big = this;
            }
            else
            {
                capacity = this.Count;
                little = this;
                big = set;
            }

            Set<T> result = this.CreateSiblingInstance( capacity );

            foreach ( T item in little )
            {
                if ( big.Contains( item ) )
                    result.Add( item );
            }

            return result;
        }

        /// <summary>
        /// Gets the difference of the current set with another.
        /// </summary>
        /// <param name="set">Second set.</param>
        /// <returns>A new set which the difference between the current set and <paramref name="set"/>.</returns>
        public Set<T> GetDifference( Set<T> set )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( set, "set" );

            #endregion

            Set<T> result = this.CreateSiblingInstance( this.Count );

            foreach ( T item in  this )
            {
                if ( !set.Contains( item ) )
                    result.Add( item );
            }

            return result;
        }

        /// <summary>
        /// Creates a well-typed instance; used by for operations
        /// <see cref="GetUnion"/>, <see cref="GetIntersection"/>
        /// and <see cref="GetDifference"/> to create the new set.
        /// </summary>
        /// <param name="capacity">Capacity of the new set.</param>
        /// <returns>A new set of capacity <paramref name="capacity"/>.</returns>
        protected virtual Set<T> CreateSiblingInstance( int capacity )
        {
            return new Set<T>( this.Count, this.dictionary.Comparer );
        }
    }
}