#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.

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

#if !SMALL
using System;
using System.Reflection;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    /// <summary>
    /// Represents an <see cref="Assembly"/> without giving directly the <see cref="Assembly"/>
    /// object. The object allows browsing of contained types and of custom attributes.
    /// </summary>
    public interface IAssemblyWrapper : IAssemblyName, IReflectionWrapper, ICustomAttributeProvider
    {
        /// <summary>
        /// Gets the system <see cref="Assembly"/> corresponding to the current object.
        /// </summary>
        Assembly UnderlyingSystemAssembly { get; }

        /// <summary>
        /// Gets the list of types defined in this assembly.
        /// </summary>
        /// <returns>The list of types defined in this assembly.</returns>
        /// <remarks>
        /// This method does not return nested types.
        /// </remarks>
        Type[] GetTypes();
    }
}
#endif