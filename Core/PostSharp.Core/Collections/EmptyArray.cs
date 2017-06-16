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
    /// Provides empty arrays of any type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class EmptyArray<T>
    {
        private static readonly T[] instance = new T[0];

        /// <summary>
        /// Gets an empty array of type <b><typeparamref name="T"/>[]</b>.
        /// </summary>
        /// <returns>An empty array of type <b><typeparamref name="T"/>[]</b>.</returns>
        public static T[] GetInstance()
        {
            return instance;
        }
    }
}