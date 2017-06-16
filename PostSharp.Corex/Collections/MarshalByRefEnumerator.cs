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
    /// Wraps an enumerator with a class derived from <see cref="MarshalByRefObject"/>,
    /// so that the enumerator can be used across application domain boundaries.
    /// </summary>
    /// <typeparam name="T">Type of enumerated elements.</typeparam>
    [Serializable]
    public sealed class MarshalByRefEnumerator<T> : MarshalByRefObject, IEnumerator<T>
    {
        /// <summary>
        /// Underlying enumerator.
        /// </summary>
        private readonly IEnumerator<T> enumerator;

        /// <summary>
        /// Initializes a new <see cref="MarshalByRefEnumerator{T}"/>.
        /// </summary>
        /// <param name="enumerator">Underlying enumerator.</param>
        public MarshalByRefEnumerator( IEnumerator<T> enumerator )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( enumerator, "enumerator" );

            #endregion

            this.enumerator = enumerator;
        }

        #region IEnumerator<T> Members

        /// <inheritdoc />
        public T Current { get { return this.enumerator.Current; } }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            this.enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members

        /// <inheritdoc />
        object IEnumerator.Current { get { return this.Current; } }

        /// <inheritdoc />
        public bool MoveNext()
        {
            return this.enumerator.MoveNext();
        }

        /// <inheritdoc />
        public void Reset()
        {
            this.enumerator.Reset();
        }

        #endregion
    }
}