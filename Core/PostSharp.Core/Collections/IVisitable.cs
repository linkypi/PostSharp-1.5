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

namespace PostSharp.Collections
{
    /// <summary>
    /// Defines the semantics of an object that can be <i>visited</i>.
    /// </summary>
    /// <typeparam name="T">Type of items that should be visited.</typeparam>
    public interface IVisitable<T>
    {
        /// <summary>
        /// Requires a callback method (named <i>visitor</i>) to be called
        /// for each item of a given role in the current object, recursively.
        /// </summary>
        /// <param name="role">Role of items to be visited, or <b>null</b> if all items
        /// of type <typeparamref name="T"/> should be visited.</param>
        /// <param name="visitor">Delegate that should be called when an
        /// item in the given role is found.</param>
        void Visit( string role, Visitor<T> visitor );
    }

    /// <summary>
    /// Signature of the method that should be called by <see cref="IVisitable{T}.Visit"/>.
    /// </summary>
    /// <typeparam name="T">Type of items that should be visited.</typeparam>
    /// <param name="owner">Object owning the visited item.</param>
    /// <param name="role">Role of the visited item</param>
    /// <param name="item">Visited item.</param>
    public delegate void Visitor<T>( IVisitable<T> owner, string role, T item );
}