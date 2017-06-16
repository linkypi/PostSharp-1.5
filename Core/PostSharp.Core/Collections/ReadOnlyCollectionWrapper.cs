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

#endregion

namespace PostSharp.Collections
{
    /// <summary>
    /// Wraps a collection (<see cref="ICollection"/>) and makes it read-only.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    internal sealed class ReadOnlyCollectionWrapper<T> : ICollection<T>
    {
        /// <summary>
        /// Underlying collection.
        /// </summary>
        private readonly ICollection<T> inner;

        /// <summary>
        /// Initializes a new <see cref="ReadOnlyCollectionWrapper{T}"/>.
        /// </summary>
        /// <param name="inner">The wrapper collection.</param>
        public ReadOnlyCollectionWrapper( ICollection<T> inner )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( inner, "inner" );

            #endregion

            this.inner = inner;
        }

        #region ICollection<T> Members

        /// <inheritdoc />
        public void Add( T item )
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool Contains( T item )
        {
            return this.inner.Contains( item );
        }

        /// <inheritdoc />
        public void CopyTo( T[] array, int arrayIndex )
        {
            this.inner.CopyTo( array, arrayIndex );
        }

        /// <inheritdoc />
        public int Count { get { return this.inner.Count; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return true; } }


        /// <inheritdoc />
        public bool Remove( T item )
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T> Members

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return this.inner.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}