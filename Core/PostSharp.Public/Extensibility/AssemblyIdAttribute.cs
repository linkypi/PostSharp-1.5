#region #region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of run-time components of PostSharp.                    *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU Lesser General Public      * 
 *   License as published by the Free Software Foundation.                     *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU Lesser General Public License  *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

using System;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Assigns a unique identifier to an assembly. This assembly identifier is used
    /// to generate unique attribute identifiers. 
    /// </summary>
    /// <remarks>
    /// By default, the assembly identifier is computed from the module name
    /// (by using the first 4 bytes of the MD5 sum of the module name).
    /// </remarks>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false, Inherited = false )]
    public sealed class AssemblyIdAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="AssemblyIdAttribute"/>.
        /// </summary>
        /// <param name="id">Assembly identifier.</param>
        public AssemblyIdAttribute( int id )
        {
        }
    }
}