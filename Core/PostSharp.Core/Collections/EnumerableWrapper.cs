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

namespace PostSharp.Collections
{
    /// <summary>
    /// Wraps an <see cref="IEnumerable{T}"/> so that the object
    /// behind becoms inaccessible, that is, read-only.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public class EnumerableWrapper<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> inner;

        /// <summary>
        /// Initializes a new <see cref="EnumerableWrapper{T}"/>.
        /// </summary>
        /// <param name="inner">Collection to wrap.</param>
        private EnumerableWrapper( IEnumerable<T> inner )
        {
            ExceptionHelper.AssertArgumentNotNull( inner, "inner" );
            this.inner = inner;
        }

        /// <summary>
        /// Wraps an enumerable into a read-only enumerable. The original
        /// enumerable becomes inaccessible.
        /// </summary>
        /// <param name="inner">The inner enumerable.</param>
        /// <returns>An equivalent enumerable.</returns>
        /// <remarks>
        /// When PostSharp is compiled not compiled in debug mode, this method
        /// simply returns <paramref name="inner"/>.
        /// </remarks>
        public static IEnumerable<T> GetInstance(IEnumerable<T> inner)
        {
#if ASSERT
            return new EnumerableWrapper<T>(inner);
#else
            return inner;
#endif
        }

        #region IEnumerable<T> Members

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return this.inner.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.inner.GetEnumerator();
        }

        #endregion
    }
}