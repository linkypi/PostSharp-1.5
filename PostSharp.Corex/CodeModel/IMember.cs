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
    /// Defines the semantics of a type member (<see cref="IMember.DeclaringType"/>
    /// and <see cref="IMember.Name"/> properties).
    /// </summary>
    public interface IMember : IMetadataDeclaration
    {
        /// <summary>
        /// Gets the declaring type.
        /// </summary>
        /// <value>
        /// The declaring type (<see cref="IType"/>), or <b>null</b> if the 
        /// member is contained by the module.
        /// </value>
        IType DeclaringType { get; }


        /// <summary>
        /// Gets the method name.
        /// </summary>
        /// <value>
        /// The method name.
        /// </value>
        string Name { get; }
    }
}