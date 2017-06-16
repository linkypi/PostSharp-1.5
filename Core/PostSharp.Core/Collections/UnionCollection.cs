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

namespace PostSharp.Collections
{
    /// <summary>
    /// Wrapper that exposes the union of two collections as a single collection.
    /// </summary>
    /// <typeparam name="ItemType"></typeparam>
    public class UnionCollection<ItemType> : ICollection<ItemType>
    {
        private readonly ICollection<ItemType> first;
        private readonly ICollection<ItemType> second;

        /// <summary>
        /// Initializes a new <see cref="UnionCollection{ItemType}"/>.
        /// </summary>
        /// <param name="first">A collection.</param>
        /// <param name="second">A collection.</param>
        public UnionCollection( ICollection<ItemType> first, ICollection<ItemType> second )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( first, "b" );
            ExceptionHelper.AssertArgumentNotNull( second, "b" );

            #endregion

            this.first = first;
            this.second = second;
        }

        #region ICollection<ItemType> Members

        /// <inheritdoc />
        public void Add( ItemType item )
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool Contains( ItemType item )
        {
            return first.Contains( item ) || second.Contains( item );
        }

        /// <inheritdoc />
        public void CopyTo( ItemType[] array, int arrayIndex )
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count { get { return first.Count + second.Count; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return true; } }

        /// <inheritdoc />
        public bool Remove( ItemType item )
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<ItemType> Members

        /// <inheritdoc />
        public IEnumerator<ItemType> GetEnumerator()
        {
            foreach ( ItemType item in first )
            {
                yield return item;
            }

            foreach ( ItemType item in second )
            {
                yield return item;
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
    }
}