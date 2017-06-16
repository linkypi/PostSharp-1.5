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

using System.Collections.Generic;

namespace PostSharp.Collections
{
    /// <summary>
    /// A list factory creates instances of lists
    /// and allocates capacity.
    /// </summary>
    /// <typeparam name="T">Type of items in the list.</typeparam>
    public interface IListFactory<T>
    {
        /// <summary>
        /// Creates a list with default capacity.
        /// </summary>
        /// <returns>A list.</returns>
        IList<T> CreateList();

        /// <summary>
        /// Creates a list with given capacity.
        /// </summary>
        /// <param name="capacity">Initial list capacity.</param>
        /// <returns>A collection.</returns>
        IList<T> CreateList( int capacity );

        /// <summary>
        /// Ensures that a list has the given capacity.
        /// </summary>
        /// <param name="list">A list.</param>
        /// <param name="capacity">The required capacity.</param>
        void EnsureCapacity( IList<T> list, int capacity );
    }
}