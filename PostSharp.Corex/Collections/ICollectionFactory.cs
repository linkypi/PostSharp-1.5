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
    /// A collection factory creates instances of collections
    /// and allocates capacity.
    /// </summary>
    /// <typeparam name="T">Type of items in the collection.</typeparam>
    public interface ICollectionFactory<T>
    {
        /// <summary>
        /// Creates a collection with default capacity.
        /// </summary>
        /// <returns>A collection.</returns>
        ICollection<T> CreateCollection();

        /// <summary>
        /// Creates a collection with given capacity.
        /// </summary>
        /// <param name="capacity">Initial collection capacity.</param>
        /// <returns>A collection.</returns>
        ICollection<T> CreateCollection( int capacity );

        /// <summary>
        /// Ensures that a collection has the given capacity.
        /// </summary>
        /// <param name="collection">A collection.</param>
        /// <param name="capacity">The required capacity.</param>
        void EnsureCapacity( ICollection<T> collection, int capacity );
    }
}