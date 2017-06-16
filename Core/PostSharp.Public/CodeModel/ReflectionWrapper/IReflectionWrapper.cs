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
    /// Interface implemented by all reflection wrappers.
    /// </summary>
    public interface IReflectionWrapper
    {
        /// <summary>
        /// Gets the system object (<see cref="Type"/>, <see cref="FieldInfo"/>, <see cref="MethodInfo"/>,
        /// <see cref="ConstructorInfo"/>, <see cref="ParameterInfo"/>, ...) corresponding to the current wrapper.
        /// </summary>
        object UnderlyingSystemObject { get; }

        /// <summary>
        /// Gets the name of the assembly declaring the current element.
        /// </summary>
        IAssemblyName DeclaringAssemblyName { get;}
    }
}
#endif