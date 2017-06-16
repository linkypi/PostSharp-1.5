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

using System;
using System.Reflection;

namespace PostSharp.Laos
{
#if !SMALL
    /// <summary>
    /// Defines the semantics of an aspect that is not woven in itself, but can
    /// provide other aspects. The current interface extends <see cref="ILaosReflectionAspectProvider"/>
    /// but support compile-time initialization and validation.
    /// </summary>
    /// <remarks>
    /// Seems combined aspects are not weavable, they don't have to be serializable.
    /// </remarks>
    public interface ICompoundAspect : ILaosAspect, ILaosReflectionAspectProvider, ILaosAspectBuildSemantics
    {
        /// <summary>
        /// Method called at compile-time by the weaver just before the instance is serialized.
        /// </summary>
        /// <param name="element">Element (<see cref="MethodBase"/>, <see cref="FieldInfo"/> 
        /// or <see cref="Type"/> on which this instance is applied.</param>
        /// <remarks>
        /// <para>Derived classes should implement this method if they want to compute some information
        /// at compile time. This information to be stored in member variables. It shall be
        /// serialized at compile time and deserialized at runtime.
        /// </para>
        /// </remarks>
        void CompileTimeInitialize( object element );
    }
#endif
}