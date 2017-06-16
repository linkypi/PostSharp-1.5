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
    /// An enumerator over an empty collection.
    /// </summary>
    /// <typeparam name="T">Type of enumerated items.</typeparam>
    public class EmptyEnumerator<T> : IEnumerator<T>
    {
        private static readonly EmptyEnumerator<T> instance = new EmptyEnumerator<T>();

        /// <summary>
        /// Get an enumerator over an empty collection.
        /// </summary>
        /// <returns>An enumerator over an empty collection.</returns>
        public static IEnumerator<T> GetInstance()
        {
            return instance;
        }

        private EmptyEnumerator()
        {
        }

        /// <inheritdoc />
        public T Current { get { throw new InvalidOperationException(); } }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        object IEnumerator.Current { get { throw new InvalidOperationException(); } }

        /// <inheritdoc />
        public bool MoveNext()
        {
            return false;
        }

        /// <inheritdoc />
        public void Reset()
        {
        }
    }
}