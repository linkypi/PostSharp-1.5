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

using System.Reflection;
using PostSharp.Extensibility;

namespace PostSharp.Laos
{
    /// <summary>
    /// Defines the run-time semantics of aspects applied to fields.
    /// </summary>
    /// <seealso cref="ILaosMethodLevelAspectBuildSemantics"/>
    public interface ILaosFieldLevelAspect : ILaosWeavableAspect
    {
        /// <summary>
        /// Method called at runtime just before the instance is deserialized.
        /// </summary>
        /// <param name="field">Field on which this instance is applied.</param>
        void RuntimeInitialize( FieldInfo field );
    }

#if !SMALL
    /// <summary>
    /// Compile-time semantics of <see cref="ILaosFieldLevelAspect"/>.
    /// </summary>
    public interface ILaosFieldLevelAspectBuildSemantics : ILaosAspectBuildSemantics
    {
        /// <summary>
        /// Method called at compile-time by the weaver just before the instance is serialized.
        /// </summary>
        /// <param name="field">Field on which this instance is applied.</param>
        /// <remarks>
        /// <para>Derived classes should implement this method if they want to compute some information
        /// at compile time. This information to be stored in member variables. It shall be
        /// serialized at compile time and deserialized at runtime.
        /// </para>
        /// <para>
        /// You cannot store and serialize the <paramref name="field"/> parameter because it is basically
        /// a runtime object. You shall receive the <see cref="FieldInfo"/> at runtime by the
        /// <see cref="ILaosFieldLevelAspect.RuntimeInitialize"/> method.
        /// </para>
        /// </remarks>
        [CompileTimeSemantic]
        void CompileTimeInitialize(FieldInfo field);
    }
#endif
}