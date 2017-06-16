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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Determines the behavior of the <see cref="ITypeSignature.GetNakedType"/> method.
    /// </summary>
    [Flags]
    public enum TypeNakingOptions
    {
        /// <summary>
        /// No naking (unless unwrapping <see cref="TypeSpecDeclaration"/>).
        /// </summary>
        None = 0,

        /// <summary>
        /// Optional custom modifiers should be removed.
        /// </summary>
        IgnoreOptionalCustomModifiers = 1,

        /// <summary>
        /// Required custom modifiers should be removed.
        /// </summary>
        IgnoreRequiredCustomModifiers = 2,

        /// <summary>
        /// All custom modifiers should be removed.
        /// </summary>
        IgnoreAllCustomModifiers = 3,

        /// <summary>
        /// Pinned modifiers should be removed.
        /// </summary>
        IgnorePinned = 4,

        /// <summary>
        /// Obsolete.
        /// </summary>
        [Obsolete( "Use IgnoreModifiers" )] IgnoreAll = IgnoreModifiers,

        /// <summary>
        /// All modifiers (custom modifiers and <i>pinned</i> modifier) should be removed.
        /// </summary>
        IgnoreModifiers = IgnoreAllCustomModifiers | IgnorePinned,

        /// <summary>
        /// Ignore managed pointers.
        /// </summary>
        IgnoreManagedPointers = 8
    }
}