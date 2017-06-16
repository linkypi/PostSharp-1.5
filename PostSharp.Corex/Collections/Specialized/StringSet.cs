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

using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Collections.Specialized
{
    /// <summary>
    /// Set of strings.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class StringSet : Set<string>
    {
        /// <summary>
        /// Initializes a new <see cref="StringSet"/> and specifies the initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        public StringSet(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="StringSet"/> with default initial capacity.
        /// </summary>
        public StringSet()
        {
        }
    }
}