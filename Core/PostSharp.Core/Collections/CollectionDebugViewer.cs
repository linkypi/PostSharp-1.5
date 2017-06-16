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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections
{
    /// <summary>
    /// Debugger viewer that presents any collection (<see cref="ICollection"/>)
    /// according to its semantics instead of its implementation, i.e. it displays
    /// the list of items exposed by the interface.
    /// </summary>
    public sealed class CollectionDebugViewer
    {
        private readonly object[] items;

        /// <summary>
        /// Initializes a new <see cref="CollectionDebugViewer"/>.
        /// </summary>
        /// <param name="collection">The collection to be viewed.</param>
        public CollectionDebugViewer( ICollection collection )
        {
            ExceptionHelper.AssertArgumentNotNull( collection, "collection" );

            this.items = new object[collection.Count];
            IEnumerator enumerator = collection.GetEnumerator();
            int i = 0;
            while ( enumerator.MoveNext() )
            {
                this.items[i] = enumerator.Current;
                i++;
            }
        }

        /// <summary>
        /// Gets the array of items.
        /// </summary>
        /// <remarks>
        /// This array is displayed as the content of the collection.
        /// </remarks>
        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
        [SuppressMessage( "Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays" )]
        public object[] Items { get { return this.items; } }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count { get { return this.items.Length; } }
    }
}