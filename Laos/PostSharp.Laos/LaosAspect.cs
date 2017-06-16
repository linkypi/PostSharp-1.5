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
using System.Diagnostics.CodeAnalysis;
using PostSharp.Extensibility;
#if !SMALL
using PostSharp.Laos.Serializers;

#endif

namespace PostSharp.Laos
{
    /// <summary>
    /// Base class for all aspects that are declared using custom attributes
    /// (<see cref="MulticastAttribute"/>).
    /// </summary>
    /// <remarks>
    /// <para>All classes derived from <see cref="LaosAspect"/> should be serializable
    /// (using the <see cref="SerializableAttribute"/> custom attribute). Fields that
    /// are only used at runtime (and unknown at compile-time) should be carefully
    /// marked with the <see cref="NonSerializedAttribute"/> custom attribute.</para>
    /// </remarks>
#if !SMALL
    [Serializable]
#endif
    [SuppressMessage( "Microsoft.Naming", "CA1710" /* IdentifiersShouldHaveCorrectSuffix */ )]
    [RequirePostSharp( "PostSharp.Laos", "PostSharp.Laos" )]
    public abstract class LaosAspect : MulticastAttribute, ILaosAspect
#if !SMALL
        , ILaosAspectBuildSemantics
#endif
    {
#if !SMALL
        /// <inheritdoc />
        [CompileTimeSemantic]
        bool ILaosAspectBuildSemantics.CompileTimeValidate( object target )
        {
            return this.CompileTimeValidate( target );
        }

        /// <summary>
        /// Method invoked at compile time to ensure that the aspect has been applied to
        /// the right target.
        /// </summary>
        /// <param name="target">Target element.</param>
        /// <returns><b>true</b> if the aspect was applied to an acceptable target, otherwise
        /// <b>false</b>.</returns>
        /// <remarks>The implementation of this method is expected to emit an error message
        /// or an exception in case of error. Only returning <b>false</b> causes the aspect
        /// to be silently ignored.</remarks>
        [CompileTimeSemantic]
        public virtual bool CompileTimeValidate( object target )
        {
            return true;
        }

        bool? ILaosAspectConfiguration.RequiresReflectionWrapper
        {
            get { return null; }
        }
#endif
    }

    /// <summary>
    /// Configures an aspect of type <see cref="ILaosAspect"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public abstract class LaosAspectConfigurationAttribute : Attribute, ILaosAspectConfiguration
    {
        private bool? requiresReflectionWrapper;

        /// <summary>
        /// Determines whether the aspect requires reflection wrappers.
        /// If not, compile-time semantics of the aspect will receive
        /// normal reflection objects, requiring the enhanced assembly to
        /// be loaded in the weaver.
        /// </summary>
        public bool RequiresReflectionWrapper
        {
            get { return this.requiresReflectionWrapper ?? false; }
            set { this.requiresReflectionWrapper = value; }
        }

        /// <inheritdoc />
        bool? ILaosAspectConfiguration.RequiresReflectionWrapper
        {
            get { return this.requiresReflectionWrapper; }
        }

    }
}