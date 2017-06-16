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
    /// Implementation of generic <see cref="ICollection{T}"/> marshalled by reference.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    [Serializable]
    public class MarshalByRefCollection<T> : MarshalByRefObject, ICollection<T>
    {
        /// <summary>
        /// Underlying implementation.
        /// </summary>
        private readonly ICollection<T> implementation;

        /// <summary>
        /// Initializes a new <see cref="MarshalByRefCollection{T}"/> with a <see cref="List{T}"/>
        /// as the underlying implementation.
        /// </summary>
        public MarshalByRefCollection()
            : this( new List<T>() )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MarshalByRefCollection{T}"/> and specifies the
        /// underlying implementation.
        /// </summary>
        /// <param name="implementation">The underlying implementation,
        /// or the collection to wrap.</param>
        public MarshalByRefCollection( ICollection<T> implementation )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( implementation, "implementation" );

            #endregion

            this.implementation = implementation;
        }

        #region ICollection<T> Members

        /// <inheritdoc />
        public void Add( T item )
        {
            this.implementation.Add( item );
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.implementation.Clear();
        }

        /// <inheritdoc />
        public bool Contains( T item )
        {
            return this.implementation.Contains( item );
        }

        /// <inheritdoc />
        public void CopyTo( T[] array, int arrayIndex )
        {
            this.implementation.CopyTo( array, arrayIndex );
        }

        /// <inheritdoc />
        public int Count { get { return this.implementation.Count; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return this.implementation.IsReadOnly; } }

        /// <inheritdoc />
        public bool Remove( T item )
        {
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
            return this.implementation.GetEnumerator();
        }

        #endregion
    }
}