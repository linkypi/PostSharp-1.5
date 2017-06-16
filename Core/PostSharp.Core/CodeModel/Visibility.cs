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

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Visibity of types and type members.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Public.
        /// </summary>
        Public,

        /// <summary>
        /// Family (protected).
        /// </summary>
        Family,

        /// <summary>
        /// Assembly (internal).
        /// </summary>
        Assembly,

        /// <summary>
        /// Family or assembly (protected internal).
        /// </summary>
        FamilyOrAssembly,

        /// <summary>
        /// Family and assembly (no C# equivalent: protected types inside the current assembly).
        /// </summary>
        FamilyAndAssembly,

        /// <summary>
        /// Private.
        /// </summary>
        Private,
    }
}